<template>
  <div class="active-tour">
    <Navbar />
    <div class="container">
      <div v-if="!activeTour" class="no-active-tour">
        <h2>No Active Tour</h2>
        <p>You don't have any active tours running.</p>
        <router-link to="/tours-for-tourist" class="btn btn-primary">Browse Tours</router-link>
      </div>
      
      <div v-else class="tour-execution-view">
        <div class="tour-header">
          <h1>{{ tourInfo?.name || 'Active Tour' }}</h1>
          <p class="tour-description">{{ tourInfo?.description }}</p>
          <div class="tour-stats">
            <span class="stat">
              <strong>Started:</strong> {{ formatDateTime(activeTour.startTime) }}
            </span>
            <span class="stat">
              <strong>Checkpoints:</strong> {{ completedCheckpointsHistory.size }} / {{ totalCheckpoints }}
            </span>
            <span class="stat">
              <strong>Progress:</strong> {{ Math.round((completedCheckpointsHistory.size / totalCheckpoints) * 100) }}%
            </span>
          </div>
        </div>

        <div class="proximity-status" :class="{ 'near-checkpoint': lastProximityCheck?.isNearCheckpoint }">
          <div v-if="lastProximityCheck?.isNearCheckpoint" class="checkpoint-alert">
            <h3>📍 You're near a checkpoint!</h3>
            <p><strong>{{ lastProximityCheck.checkpointName }}</strong></p>
            <p>Distance: {{ Math.round(lastProximityCheck.distanceMeters) }} meters</p>
            <div v-if="lastProximityCheck.checkpointCompleted" class="completed-alert">
              ✅ Checkpoint completed!
            </div>
          </div>
          <div v-else class="status-info">
            <p>Looking for nearby checkpoints...</p>
            <p><small>Last check: {{ formatTime(lastProximityCheck?.lastActivity || activeTour.lastActivity) }}</small></p>
          </div>
        </div>

        <div class="map-container">
          <div id="activeTourMap" style="height: 400px; width: 100%;"></div>
        </div>

        <div class="checkpoint-progress">
          <h3>Checkpoint Progress</h3>
          
          <!-- Show completion status -->
          <div v-if="completedCheckpointsHistory.size === totalCheckpoints && totalCheckpoints > 0" 
               class="tour-completed-alert">
            <h4>🏆 All Checkpoints Completed!</h4>
            <p>You have successfully visited all {{ totalCheckpoints }} checkpoints on this tour!</p>
          </div>
          
          <div v-if="checkpoints.length === 0" class="loading">Loading checkpoints...</div>
          <div v-else class="checkpoint-list">
            <div v-for="checkpoint in checkpoints" :key="checkpoint.id" 
                 :class="['checkpoint-item', { 'completed': isCheckpointCompleted(checkpoint.id) }]">
              <div class="checkpoint-info">
                <h4>{{ checkpoint.name }}</h4>
                <p>{{ checkpoint.description }}</p>
                <div class="coordinates">
                  Lat: {{ checkpoint.latitude.toFixed(6) }}, Lng: {{ checkpoint.longitude.toFixed(6) }}
                </div>
                <div v-if="completedCheckpointsHistory.has(checkpoint.id)" class="completion-time">
                  ✅ Completed: {{ formatDateTime(completedCheckpointsHistory.get(checkpoint.id).completedAt) }}
                </div>
              </div>
              <div class="checkpoint-status">
                <span v-if="isCheckpointCompleted(checkpoint.id)" class="completed-badge">
                  ✅ Completed
                </span>
                <span v-else class="pending-badge">
                  ⏳ Pending
                </span>
              </div>
            </div>
          </div>
        </div>

        <div class="tour-actions">
          <button @click="completeTour" 
                  :disabled="completedCheckpointsHistory.size === totalCheckpoints && totalCheckpoints > 0"
                  class="btn btn-success">
            {{ completedCheckpointsHistory.size === totalCheckpoints && totalCheckpoints > 0 ? 'Auto-Completing...' : 'Complete Tour' }}
          </button>
          <button @click="abandonTour" class="btn btn-danger">Abandon Tour</button>
        </div>
      </div>

      <div v-if="message" class="message" :class="messageType">{{ message }}</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import Navbar from './Navbar.vue';

const router = useRouter();
const activeTour = ref(null);
const tourInfo = ref(null);
const checkpoints = ref([]);
const completedCheckpoints = ref([]);
const completedCheckpointsHistory = ref(new Map()); // Persistent history: checkpointId -> completion info
const totalCheckpoints = ref(0);
const lastProximityCheck = ref(null);
const message = ref('');
const messageType = ref('');
let map = null;
let currentMarker = null;
let checkpointMarkers = [];
let proximityCheckInterval = null;

// Initialize map
const initMap = () => {
  if (!activeTour.value || !window.L) return;

  const centerLat = activeTour.value.currentLatitude || activeTour.value.startLatitude;
  const centerLng = activeTour.value.currentLongitude || activeTour.value.startLongitude;
  
  map = window.L.map('activeTourMap').setView([centerLat, centerLng], 15);
  
  window.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map);

  // Add current position marker
  updateCurrentPositionMarker(centerLat, centerLng);
  
  // Add checkpoint markers
  drawCheckpointMarkers();
};

const updateCurrentPositionMarker = (lat, lng) => {
  if (currentMarker) {
    map.removeLayer(currentMarker);
  }
  
  const icon = window.L.divIcon({
    html: '<div style="background-color: blue; width: 12px; height: 12px; border-radius: 50%; border: 2px solid white;"></div>',
    iconSize: [16, 16],
    className: 'current-position-marker'
  });
  
  currentMarker = window.L.marker([lat, lng], { icon })
    .addTo(map)
    .bindPopup('Your current position');
};

const drawCheckpointMarkers = () => {
  checkpointMarkers.forEach(marker => map.removeLayer(marker));
  checkpointMarkers = [];
  
  checkpoints.value.forEach(checkpoint => {
    const isCompleted = isCheckpointCompleted(checkpoint.id);
    const icon = window.L.divIcon({
      html: `<div style="background-color: ${isCompleted ? 'green' : 'red'}; color: white; width: 20px; height: 20px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; border: 2px solid white;">${isCompleted ? '✓' : '?'}</div>`,
      iconSize: [24, 24],
      className: 'checkpoint-marker'
    });
    
    const marker = window.L.marker([checkpoint.latitude, checkpoint.longitude], { icon })
      .addTo(map)
      .bindPopup(`<strong>${checkpoint.name}</strong><br>${checkpoint.description}<br>Status: ${isCompleted ? 'Completed' : 'Pending'}`);
    
    checkpointMarkers.push(marker);
  });
};

const isCheckpointCompleted = (checkpointId) => {
  // Check persistent history first
  if (completedCheckpointsHistory.value.has(checkpointId)) {
    return true;
  }
  // Fallback to original array check
  return completedCheckpoints.value.some(cp => cp.checkpointId === checkpointId);
};

const getCurrentPosition = () => {
  const position = window.getCurrentTouristPosition?.();
  if (position) {
    return position;
  }
  // Fallback to current tour position
  return {
    latitude: activeTour.value?.currentLatitude || activeTour.value?.startLatitude,
    longitude: activeTour.value?.currentLongitude || activeTour.value?.startLongitude
  };
};

// Helper functions for persistent checkpoint history
const saveCompletedCheckpointsToStorage = () => {
  if (!activeTour.value) return;
  
  const historyKey = `completedCheckpoints_${activeTour.value.id}`;
  const historyData = Array.from(completedCheckpointsHistory.value.entries());
  localStorage.setItem(historyKey, JSON.stringify(historyData));
};

const loadCompletedCheckpointsFromStorage = () => {
  if (!activeTour.value) return;
  
  const historyKey = `completedCheckpoints_${activeTour.value.id}`;
  const savedHistory = localStorage.getItem(historyKey);
  
  if (savedHistory) {
    try {
      const historyData = JSON.parse(savedHistory);
      completedCheckpointsHistory.value = new Map(historyData);
      
      // Also populate the original array for compatibility
      completedCheckpoints.value = Array.from(completedCheckpointsHistory.value.values());
    } catch (error) {
      console.error('Error loading checkpoint history:', error);
      completedCheckpointsHistory.value = new Map();
    }
  } else {
    completedCheckpointsHistory.value = new Map();
  }
};

const clearCompletedCheckpointsFromStorage = () => {
  if (!activeTour.value) return;
  
  const historyKey = `completedCheckpoints_${activeTour.value.id}`;
  localStorage.removeItem(historyKey);
};

// Check if all checkpoints are completed and auto-complete tour
const checkForTourCompletion = () => {
  if (!activeTour.value || checkpoints.value.length === 0) return;
  
  const totalCheckpoints = checkpoints.value.length;
  const completedCount = completedCheckpointsHistory.value.size;
  
  console.log(`Checkpoint completion check: ${completedCount}/${totalCheckpoints}`);
  
  if (completedCount === totalCheckpoints && completedCount > 0) {
    showMessage('🎉 All checkpoints completed! Tour will be automatically completed in 3 seconds...', 'success');
    
    // Auto-complete after 3 seconds
    setTimeout(() => {
      autoCompleteTour();
    }, 3000);
  }
};

// Automatically complete tour when all checkpoints are done
const autoCompleteTour = async () => {
  if (!activeTour.value) return;

  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.post('http://localhost:8082/api/tours/execution/complete', {
      tourExecutionId: activeTour.value.id,
      isAbandoned: false
    }, {
      headers: { Authorization: `Bearer ${jwt}` }
    });

    // Clear persistent checkpoint history since tour is completed
    clearCompletedCheckpointsFromStorage();
    localStorage.removeItem('activeTourExecution');
    showMessage('🏆 Congratulations! Tour completed successfully! All checkpoints visited! 🎉', 'success');
    
    setTimeout(() => {
      router.push('/tours-for-tourist');
    }, 3000);

  } catch (error) {
    console.error('Error auto-completing tour:', error);
    showMessage(error.response?.data?.message || 'Error completing tour automatically', 'error');
  }
};

const checkCheckpointProximity = async () => {
  if (!activeTour.value) return;

  try {
    const position = getCurrentPosition();
    if (!position) return;

    const jwt = localStorage.getItem('token');
    const response = await axios.post('http://localhost:8082/api/tours/execution/checkproximity', {
      tourExecutionId: activeTour.value.id,
      currentLatitude: position.latitude,
      currentLongitude: position.longitude
    }, {
      headers: { Authorization: `Bearer ${jwt}` }
    });

    lastProximityCheck.value = response.data.value;
    
    // Update current position on map
    if (map) {
      updateCurrentPositionMarker(position.latitude, position.longitude);
      map.setView([position.latitude, position.longitude], map.getZoom());
    }

    // If checkpoint was just completed, update our local state
    if (lastProximityCheck.value?.checkpointCompleted && lastProximityCheck.value?.checkpointId) {
      const checkpointId = lastProximityCheck.value.checkpointId;
      
      // Add to persistent history if not already there
      if (!completedCheckpointsHistory.value.has(checkpointId)) {
        completedCheckpointsHistory.value.set(checkpointId, {
          checkpointId: checkpointId,
          checkpointName: lastProximityCheck.value.checkpointName,
          completedAt: new Date(),
          tourExecutionId: activeTour.value.id
        });
        
        // Save to localStorage for persistence across sessions
        saveCompletedCheckpointsToStorage();
        
        // Also add to the original array for compatibility
        completedCheckpoints.value.push({
          checkpointId: checkpointId,
          completedAt: new Date()
        });
        
        drawCheckpointMarkers(); // Redraw markers with updated status
        showMessage(`Checkpoint '${lastProximityCheck.value.checkpointName}' completed! 🎉`, 'success');
        
        // Check if all checkpoints are completed
        checkForTourCompletion();
      }
    }

  } catch (error) {
    console.error('Error checking proximity:', error);
  }
};

const loadActiveTour = async () => {
  // This would need to be implemented - get active tour execution for current user
  // For now, we'll check localStorage or make an API call
  const savedTour = localStorage.getItem('activeTourExecution');
  if (savedTour) {
    activeTour.value = JSON.parse(savedTour);
    // Load persistent checkpoint history
    loadCompletedCheckpointsFromStorage();
    await loadTourDetails();
    await loadCheckpoints();
  }
};

const loadTourDetails = async () => {
  if (!activeTour.value) return;

  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.get(`http://localhost:8082/api/tours`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    const tours = response.data.value || response.data.tours || [];
    tourInfo.value = tours.find(t => t.id === activeTour.value.tourId);
  } catch (error) {
    console.error('Error loading tour details:', error);
  }
};

const loadCheckpoints = async () => {
  if (!activeTour.value) return;

  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.get(`http://localhost:8082/api/tours/checkpoints/${activeTour.value.tourId}`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    checkpoints.value = response.data.value || response.data.results || [];
    totalCheckpoints.value = checkpoints.value.length;
    
    // Load any existing completed checkpoints from tour execution
    const existingCompleted = activeTour.value.checkpointProgresses || [];
    
    // Merge with persistent history
    existingCompleted.forEach(cp => {
      if (!completedCheckpointsHistory.value.has(cp.checkpointId)) {
        completedCheckpointsHistory.value.set(cp.checkpointId, {
          checkpointId: cp.checkpointId,
          completedAt: cp.completedAt,
          tourExecutionId: activeTour.value.id
        });
      }
    });
    
    // Update the original array for compatibility
    completedCheckpoints.value = Array.from(completedCheckpointsHistory.value.values());
    
    // Save merged history
    saveCompletedCheckpointsToStorage();
    
    // Check if tour should be auto-completed (in case user refreshed page)
    setTimeout(() => {
      checkForTourCompletion();
    }, 1000);
  } catch (error) {
    console.error('Error loading checkpoints:', error);
  }
};

const completeTour = async () => {
  if (!activeTour.value) return;

  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.post('http://localhost:8082/api/tours/execution/complete', {
      tourExecutionId: activeTour.value.id,
      isAbandoned: false
    }, {
      headers: { Authorization: `Bearer ${jwt}` }
    });

    // Clear persistent checkpoint history since tour is completed
    clearCompletedCheckpointsFromStorage();
    localStorage.removeItem('activeTourExecution');
    showMessage('Tour completed successfully! 🎉', 'success');
    
    setTimeout(() => {
      router.push('/tours-for-tourist');
    }, 2000);

  } catch (error) {
    console.error('Error completing tour:', error);
    showMessage(error.response?.data?.message || 'Error completing tour', 'error');
  }
};

const abandonTour = async () => {
  if (!confirm('Are you sure you want to abandon this tour?')) return;

  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.post('http://localhost:8082/api/tours/execution/complete', {
      tourExecutionId: activeTour.value.id,
      isAbandoned: true
    }, {
      headers: { Authorization: `Bearer ${jwt}` }
    });

    // Clear persistent checkpoint history since tour is abandoned
    clearCompletedCheckpointsFromStorage();
    localStorage.removeItem('activeTourExecution');
    showMessage('Tour abandoned', 'warning');
    
    setTimeout(() => {
      router.push('/tours-for-tourist');
    }, 2000);

  } catch (error) {
    console.error('Error abandoning tour:', error);
    showMessage(error.response?.data?.message || 'Error abandoning tour', 'error');
  }
};

const showMessage = (msg, type = 'info') => {
  message.value = msg;
  messageType.value = type;
  setTimeout(() => {
    message.value = '';
    messageType.value = '';
  }, 5000);
};

const formatDateTime = (dateString) => {
  return new Date(dateString).toLocaleString();
};

const formatTime = (dateString) => {
  return new Date(dateString).toLocaleTimeString();
};

onMounted(async () => {
  await loadActiveTour();
  
  // Load Leaflet if not already loaded
  if (!window.L) {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://unpkg.com/leaflet/dist/leaflet.css';
    document.head.appendChild(link);
    
    const script = document.createElement('script');
    script.src = 'https://unpkg.com/leaflet/dist/leaflet.js';
    script.onload = () => {
      if (activeTour.value) {
        initMap();
      }
    };
    document.body.appendChild(script);
  } else if (activeTour.value) {
    initMap();
  }

  // Start proximity checking every 10 seconds
  if (activeTour.value) {
    proximityCheckInterval = setInterval(checkCheckpointProximity, 10000);
    // Initial check
    checkCheckpointProximity();
  }
});

onUnmounted(() => {
  if (proximityCheckInterval) {
    clearInterval(proximityCheckInterval);
  }
});
</script>

<style scoped>
.active-tour {
  padding: 20px;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.no-active-tour {
  text-align: center;
  padding: 40px 20px;
}

.tour-header {
  background-color: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.tour-stats {
  display: flex;
  gap: 20px;
  margin-top: 10px;
}

.stat {
  background-color: white;
  padding: 5px 10px;
  border-radius: 4px;
  font-size: 14px;
}

.proximity-status {
  margin: 20px 0;
  padding: 15px;
  border-radius: 8px;
  border: 2px solid #dee2e6;
}

.proximity-status.near-checkpoint {
  border-color: #28a745;
  background-color: #d4edda;
}

.checkpoint-alert h3 {
  color: #155724;
  margin: 0 0 10px 0;
}

.completed-alert {
  background-color: #28a745;
  color: white;
  padding: 5px 10px;
  border-radius: 4px;
  display: inline-block;
  margin-top: 5px;
}

.map-container {
  margin: 20px 0;
  border: 1px solid #ddd;
  border-radius: 8px;
  overflow: hidden;
}

.checkpoint-progress {
  margin: 20px 0;
}

.tour-completed-alert {
  background-color: #d4edda;
  border: 2px solid #28a745;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  text-align: center;
}

.tour-completed-alert h4 {
  color: #155724;
  margin: 0 0 10px 0;
  font-size: 18px;
}

.tour-completed-alert p {
  color: #155724;
  margin: 0;
  font-size: 14px;
}

.checkpoint-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.checkpoint-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #f8f9fa;
}

.checkpoint-item.completed {
  background-color: #d4edda;
  border-color: #c3e6cb;
}

.checkpoint-info h4 {
  margin: 0 0 5px 0;
}

.checkpoint-info p {
  margin: 0 0 5px 0;
  color: #6c757d;
}

.coordinates {
  font-size: 12px;
  color: #6c757d;
}

.completion-time {
  font-size: 12px;
  color: #28a745;
  font-weight: bold;
  margin-top: 5px;
}

.completed-badge {
  background-color: #28a745;
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.pending-badge {
  background-color: #ffc107;
  color: #212529;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.tour-actions {
  display: flex;
  gap: 10px;
  margin: 20px 0;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  text-align: center;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-success {
  background-color: #28a745;
  color: white;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn:hover {
  opacity: 0.9;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.message {
  padding: 10px;
  margin: 20px 0;
  border-radius: 5px;
}

.message.success {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.message.error {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.message.warning {
  background-color: #fff3cd;
  color: #856404;
  border: 1px solid #ffeaa7;
}
</style>
