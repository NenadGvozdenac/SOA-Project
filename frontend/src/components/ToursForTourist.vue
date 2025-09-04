<template>
  <div class="tours-tourist-container">
    <h2>Published Tours</h2>
  <!-- ShoppingCart is now global in App.vue -->
    <div v-if="loading" class="loading">Loading tours...</div>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="tours.length" class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card">
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
        <button 
          v-if="!tour.isArchived && !isTourPurchased(tour.id)" 
          @click="addToCart(tour)"
        >Add to Cart</button>
        <span v-if="isTourPurchased(tour.id)" class="purchased-label">Purchased</span>
      </div>
    </div>
    <div v-else-if="!loading && !error" class="no-tours">No published tours found.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { AuthService } from '../services/auth_service.js';
// Use native event for cart open
import TourReviews from './TourReviews.vue';
const tours = ref([]);
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

async function fetchPurchasedTours() {
  const jwt = localStorage.getItem('token');
  try {
    const response = await axios.get('http://localhost:8082/api/tours/gettoursfromcart', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    purchasedTourIds.value = (response.data.value || []).map(item => item.tourId);
  } catch {}
}

function isTourPurchased(tourId) {
  return purchasedTourIds.value.includes(tourId);
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
        
        window.dispatchEvent(new Event('cart-updated'));
        
        
        
      } catch (error) {
        console.error('Error adding tour to cart:', error);
        if (error.response) {
          console.error('Response data:', error.response.data);
          console.error('Response status:', error.response.status);
          console.error('Response headers:', error.response.headers);
        } else if (error.request) {
          console.error('No response received:', error.request);
        } else {
          console.error('Error setting up request:', error.message);
        }
        alert('Failed to add tour to cart!');
      }
    }

onMounted(async () => {
  try {
    const jwt = localStorage.getItem('token');
    const userInfo = AuthService.decode(jwt);
    const userRole = userInfo?.userRole || 'Tourist'; // Default to Tourist for this component
    
    // gRPC call via gateway-net - NEW 
    const response = await axios.get(`http://localhost:8084/api/tours?user_id=${userInfo?.id || ''}&auth_token=${jwt}`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    // RPC call - OLD (using gateway)
    // const response = await axios.get(`http://localhost:3001/api/rpc/tours/rpc?role=${userRole}`, {
    //   headers: { Authorization: `Bearer ${jwt}` }
    // });
    
    // REST call - BACKUP (commented)
    // const response = await axios.get('http://localhost:8082/api/tours', {
    // const response = await axios.get('http://localhost:3001/api/tours', {
    //   headers: { Authorization: `Bearer ${jwt}` }
    // });
    
    //tours.value = response.data.value || [];
    tours.value = response.data.tours || []; //POTRENCIJALNO?
    // Note: RPC already filters for published tours based on Tourist role
    // No need for additional filtering like: .filter(t => t.status === 1 || t.status === 'Published')
    
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
  } catch (err) {
    error.value = 'Failed to load published tours.';
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
</style>
