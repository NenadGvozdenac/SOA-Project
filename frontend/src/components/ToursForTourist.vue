<template>
  <div>
    <Navbar />
    <div class="tours-tourist-container">
      <h2>Tours</h2>
      
      <!-- Toggle buttons for different views -->
      <div class="view-toggle">
        <button 
          :class="['toggle-btn', { 'active': currentView === 'my-tours' }]"
          @click="currentView = 'my-tours'"
        >
          🗺️ My Tours
        </button>
        <button 
          :class="['toggle-btn', { 'active': currentView === 'browse-tours' }]"
          @click="currentView = 'browse-tours'"
        >
          🔍 Browse Tours
        </button>
      </div>
    <div v-if="loading" class="loading">Loading tours...</div>
    <div v-if="error" class="error">{{ error }}</div>
    
    <!-- My Tours View -->
    <div v-if="currentView === 'my-tours'">
      <div v-if="filteredTours.length" class="tours-grid">
        <div v-for="tour in filteredTours" :key="tour.id" class="tour-card">
          <h3>{{ tour.name }}</h3>
          <p><strong>Description:</strong> {{ tour.description }}</p>
          <p><strong>Difficulty:</strong> {{ difficultyLabels[tour.difficulty] || 'Unknown' }}</p>
          <p><strong>Price:</strong> {{ tour.price }} €</p>
          <div v-if="tour.tags && Array.isArray(tour.tags) && tour.tags.length > 0">
            <strong>Tags:</strong>
            <span v-for="tag in tour.tags" :key="tag" class="tag">{{ tag }}</span>
          </div>
          <div v-if="firstCheckpoint[tour.id]">
            <strong>First Key Checkpoint:</strong>
            <div class="checkpoint-card">
              <div><strong>{{ firstCheckpoint[tour.id].name }}</strong></div>
              <div>{{ firstCheckpoint[tour.id].description }}</div>
              <div>Lat: {{ firstCheckpoint[tour.id].latitude }}, Lng: {{ firstCheckpoint[tour.id].longitude }}</div>
              <div v-if="firstCheckpoint[tour.id].imageBase64">
                <img :src="`data:image/jpeg;base64,${firstCheckpoint[tour.id].imageBase64}`" alt="Preview" style="max-width:200px;max-height:200px;" />
              </div>
            </div>
          </div>
          <TourReviews :tour-id="tour.id" />
          <div class="tour-actions">
            <button 
              @click="startTour(tour)"
              class="btn btn-success"
            >Start Tour</button>
            <span class="purchased-label">✅ Purchased</span>
          </div>
        </div>
      </div>
      <div v-else-if="!loading && !error" class="no-tours">
        You haven't purchased any tours yet. Switch to "Browse Tours" to find and purchase tours.
      </div>
    </div>

    <!-- Browse Tours View -->
    <div v-if="currentView === 'browse-tours'">
      <div v-if="availableTours.length" class="tours-grid">
        <div v-for="tour in availableTours" :key="tour.id" class="tour-card">
          <h3>{{ tour.name }}</h3>
          <p><strong>Description:</strong> {{ tour.description }}</p>
          <p><strong>Difficulty:</strong> {{ difficultyLabels[tour.difficulty] || 'Unknown' }}</p>
          <p><strong>Price:</strong> {{ tour.price }} €</p>
          <div v-if="tour.tags && Array.isArray(tour.tags) && tour.tags.length > 0">
            <strong>Tags:</strong>
            <span v-for="tag in tour.tags" :key="tag" class="tag">{{ tag }}</span>
          </div>
          <div v-if="firstCheckpoint[tour.id]">
            <strong>First Key Checkpoint:</strong>
            <div class="checkpoint-card">
              <div><strong>{{ firstCheckpoint[tour.id].name }}</strong></div>
              <div>{{ firstCheckpoint[tour.id].description }}</div>
              <div>Lat: {{ firstCheckpoint[tour.id].latitude }}, Lng: {{ firstCheckpoint[tour.id].longitude }}</div>
              <div v-if="firstCheckpoint[tour.id].imageBase64">
                <img :src="`data:image/jpeg;base64,${firstCheckpoint[tour.id].imageBase64}`" alt="Preview" style="max-width:200px;max-height:200px;" />
              </div>
            </div>
          </div>
          <TourReviews :tour-id="tour.id" />
          <div class="tour-actions">
            <button 
              v-if="!tour.isArchived && !isTourPurchased(tour.id)" 
              @click="addToCart(tour)"
              class="btn btn-primary"
            >Add to Cart</button>
            <button 
              v-if="isTourPurchased(tour.id)" 
              @click="startTour(tour)"
              class="btn btn-success"
            >Start Tour</button>
            <span v-if="isTourPurchased(tour.id)" class="purchased-label">✅ Purchased</span>
          </div>
        </div>
      </div>
      <div v-else-if="!loading && !error" class="no-tours">No tours available for browsing.</div>
    </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import { AuthService } from '../services/auth_service.js';
import TourReviews from './TourReviews.vue';
import Navbar from './Navbar.vue';

const router = useRouter();
const tours = ref([]);
const boughtTours = ref([]);
const loading = ref(true);
const error = ref('');
const firstCheckpoint = ref({});
const difficultyLabels = {
  0: 'Easy',
  1: 'Medium',
  2: 'Hard',
  'Easy': 'Easy',
  'Medium': 'Medium',
  'Hard': 'Hard'
};
const purchasedTourIds = ref([]);
const currentView = ref('my-tours'); // 'my-tours' or 'browse-tours'

// Computed property to show purchased tours
const filteredTours = computed(() => {
  return boughtTours.value;
});

// Computed property to show all available tours
const availableTours = computed(() => {
  return tours.value; // Shows all tours, with different actions based on purchase status
});

async function fetchPurchasedTours() {
  const jwt = localStorage.getItem('token');
  try {
    const response = await axios.get('http://localhost:8082/api/tours/boughttours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    console.log('GetBoughtTours API response:', response.data);
    
    // Handle the API response structure: { value: [...] }
    const boughtToursData = response.data.value || response.data;
    
    // Store the full tour data and extract IDs
    boughtTours.value = boughtToursData;
    purchasedTourIds.value = boughtToursData.map(tour => tour.id);
    
    // Extract first checkpoints from bought tours
    boughtToursData.forEach(tour => {
      if (tour.checkpoints && tour.checkpoints.length > 0) {
        // Sort checkpoints by creation date and get the first one
        const sortedCheckpoints = tour.checkpoints.sort((a, b) => new Date(a.createdAt || 0) - new Date(b.createdAt || 0));
        firstCheckpoint.value[tour.id] = sortedCheckpoints[0];
      }
    });
    
    console.log('Purchased tour IDs:', purchasedTourIds.value);
    console.log('Bought tours:', boughtTours.value);
  } catch (error) {
    console.error('Error fetching purchased tours:', error);
    console.error('Error response:', error.response?.data);
    boughtTours.value = [];
    purchasedTourIds.value = []; // Set empty array on error
  }
}

function isTourPurchased(tourId) {
  return purchasedTourIds.value.includes(tourId);
}

async function startTour(tour) {
  try {
    // First, get current position from position simulator
    const currentPosition = window.getCurrentTouristPosition?.();
    
    if (!currentPosition) {
      alert('Please set your current position using the Position Simulator first!');
      router.push('/position-simulator');
      return;
    }

    const jwt = localStorage.getItem('token');
    
    // Start tour execution
    const response = await axios.post('http://localhost:8082/api/tours/execution/start', {
      tourId: tour.id,
      currentLatitude: currentPosition.latitude,
      currentLongitude: currentPosition.longitude
    }, {
      headers: { Authorization: `Bearer ${jwt}` }
    });

    const tourExecution = response.data.value;
    
    // Save active tour execution to localStorage
    localStorage.setItem('activeTourExecution', JSON.stringify(tourExecution));
    
    // Navigate to active tour view
    router.push('/active-tour');

  } catch (error) {
    console.error('Error starting tour:', error);
    console.error('Response data:', error.response?.data);
    const errorMessage = error.response?.data?.message || error.response?.data?.error || 'Error starting tour';
    alert(errorMessage);
  }
}

async function addToCart(tour) {
  const jwt = localStorage.getItem('token');
  const token = localStorage.getItem('token');
  console.log('addToCart called with tourId:', tour.id);
  console.log('Token:', token);
  try {
    const response = await axios.post(
      'http://localhost:8082/api/tours/addtourtocart',
      {
        tourId: tour.id,
        price: tour.price,
        tourName: tour.name
      },
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      }
    );
    console.log('addToCart response:', response);
    alert(`Tour "${tour.name}" added to cart successfully!`);
    
    // Trigger cart refresh
    window.dispatchEvent(new CustomEvent('cart-updated'));
    
    // Refresh purchased tours to update the UI
    await fetchPurchasedTours();
  } catch (error) {
    console.error('Error adding tour to cart:', error);
    const errorMessage = error.response?.data?.message || error.response?.data?.error || 'Error adding tour to cart';
    alert(errorMessage);
  }
}

onMounted(async () => {
  try {
    const jwt = localStorage.getItem('token');
    const userInfo = AuthService.decode(jwt);
    const userRole = userInfo?.userRole || 'Tourist'; // Default to Tourist for this component
    
    // Load tours from the tours service
    const response = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    tours.value = response.data.value || [];
    
    for (const tour of tours.value) {
      try {
        const cpRes = await axios.get(`http://localhost:8082/api/tours/checkpoints/${tour.id}`, {
          headers: { Authorization: `Bearer ${jwt}` }
        });
        const checkpoints = Array.isArray(cpRes.data.value) ? cpRes.data.value : cpRes.data.results || [];
        if (checkpoints.length > 0) {
          checkpoints.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
          firstCheckpoint.value[tour.id] = checkpoints[0];
        }
      } catch {}
    }
    await fetchPurchasedTours();
    
    // Listen for tour purchases to refresh the UI
    window.addEventListener('tours-purchased', fetchPurchasedTours);
  } catch (err) {
    error.value = 'Failed to load your purchased tours.';
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.tours-tourist-container {
  max-width: 900px;
  margin: 2rem auto;
  padding: 2rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.08);
}

.view-toggle {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  justify-content: center;
}

.toggle-btn {
  padding: 0.75rem 1.5rem;
  border: 2px solid #667eea;
  background: #fff;
  color: #667eea;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
}

.toggle-btn:hover {
  background: #f0f4ff;
}

.toggle-btn.active {
  background: #667eea;
  color: #fff;
}

.tours-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 1.5rem;
}
.tour-card {
  background: #f9fafb;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.07);
  padding: 1.2rem 1rem;
  transition: box-shadow 0.2s;
}
.tour-card:hover {
  box-shadow: 0 6px 24px rgba(102,126,234,0.13);
}
.tag {
  display: inline-block;
  background: #667eea;
  color: #fff;
  border-radius: 6px;
  padding: 0.2rem 0.7rem;
  margin: 0 0.3rem;
  font-size: 0.85rem;
}
.loading, .error, .no-tours {
  text-align: center;
  margin-top: 2rem;
  color: #764ba2;
  font-weight: 600;
}
.checkpoint-card {
  background: #f3f4f6;
  border-radius: 8px;
  padding: 0.7rem 1rem;
  margin-top: 0.7rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.purchased-label {
  color: #38a169;
  font-weight: bold;
  margin-left: 1rem;
}

.tour-actions {
  display: flex;
  gap: 10px;
  align-items: center;
  margin-top: 15px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
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

.btn:hover {
  opacity: 0.9;
}
</style>
