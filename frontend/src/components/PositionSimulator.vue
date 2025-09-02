<template>
  <div class="position-simulator">
    <Navbar />
    <div class="container">
      <h1>Position Simulator</h1>
      <p>Click on the map to set your current location</p>
      
      <div class="position-info" v-if="currentPosition">
        <h3>Current Position:</h3>
        <p><strong>Latitude:</strong> {{ currentPosition.latitude.toFixed(6) }}</p>
        <p><strong>Longitude:</strong> {{ currentPosition.longitude.toFixed(6) }}</p>
        <button @click="clearPosition" class="btn btn-warning">Clear Position</button>
      </div>
      
      <div id="map" style="height: 500px; width: 100%; margin-top: 20px;"></div>
      
      <div v-if="message" class="message">{{ message }}</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import Navbar from './Navbar.vue';

const currentPosition = ref(null);
const message = ref('');
let map = null;
let currentMarker = null;

// Load position from localStorage on component mount
const loadSavedPosition = () => {
  const saved = localStorage.getItem('touristPosition');
  if (saved) {
    currentPosition.value = JSON.parse(saved);
    return currentPosition.value;
  }
  return null;
};

// Save position to localStorage
const savePosition = (lat, lng) => {
  const position = { latitude: lat, longitude: lng };
  currentPosition.value = position;
  localStorage.setItem('touristPosition', JSON.stringify(position));
  message.value = 'Position updated successfully!';
  setTimeout(() => message.value = '', 3000);
};

// Clear current position
const clearPosition = () => {
  currentPosition.value = null;
  localStorage.removeItem('touristPosition');
  if (currentMarker) {
    map.removeLayer(currentMarker);
    currentMarker = null;
  }
  message.value = 'Position cleared!';
  setTimeout(() => message.value = '', 3000);
};

// Initialize map
const initMap = () => {
  const savedPosition = loadSavedPosition();
  const centerLat = savedPosition ? savedPosition.latitude : 44.7866;
  const centerLng = savedPosition ? savedPosition.longitude : 20.4489;
  
  map = window.L.map('map').setView([centerLat, centerLng], 13);
  
  window.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map);

  // Show saved position marker if exists
  if (savedPosition) {
    currentMarker = window.L.marker([savedPosition.latitude, savedPosition.longitude])
      .addTo(map)
      .bindPopup('Your current position')
      .openPopup();
  }

  // Add click handler to map
  map.on('click', function(e) {
    const { lat, lng } = e.latlng;
    
    // Remove existing marker
    if (currentMarker) {
      map.removeLayer(currentMarker);
    }
    
    // Add new marker
    currentMarker = window.L.marker([lat, lng])
      .addTo(map)
      .bindPopup('Your current position')
      .openPopup();
    
    // Save position
    savePosition(lat, lng);
  });
};

// Get current position for external use (API for other components)
window.getCurrentTouristPosition = () => {
  return currentPosition.value;
};

onMounted(async () => {
  // Load Leaflet if not already loaded
  if (!window.L) {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://unpkg.com/leaflet/dist/leaflet.css';
    document.head.appendChild(link);
    
    const script = document.createElement('script');
    script.src = 'https://unpkg.com/leaflet/dist/leaflet.js';
    script.onload = () => {
      initMap();
    };
    document.body.appendChild(script);
  } else {
    initMap();
  }
});
</script>

<style scoped>
.position-simulator {
  padding: 20px;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.position-info {
  background-color: #f8f9fa;
  padding: 15px;
  border-radius: 5px;
  margin: 20px 0;
}

.message {
  padding: 10px;
  margin: 20px 0;
  border-radius: 5px;
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.btn-warning {
  background-color: #ffc107;
  color: #212529;
}

.btn-warning:hover {
  background-color: #e0a800;
}

#map {
  border: 1px solid #ddd;
  border-radius: 5px;
}
</style>
