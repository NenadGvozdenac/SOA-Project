<template>
  <div class="tours-list-container">
    <h2>All Tours</h2>
    <div v-if="loading" class="loading">Loading tours...</div>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="tours.length" class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card" @click="goToCheckpoint(tour.id)" style="cursor:pointer;">
        <h3>{{ tour.name }}</h3>
        <p><strong>Description:</strong> {{ tour.description }}</p>
        <p><strong>Difficulty:</strong> {{ tour.difficulty }}</p>
        <p><strong>Status:</strong> {{ tour.status }}</p>
        <p><strong>Price:</strong> {{ tour.price }} €</p>
        <div v-if="tour.tags && tour.tags.length">
          <strong>Tags:</strong>
          <span v-for="tag in tour.tags" :key="tag" class="tag">{{ tag }}</span>
        </div>
      </div>
    </div>
    <div v-else-if="!loading && !error" class="no-tours">No tours found.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { TOURS_URL } from '../services/const_service.js';
import { useRouter } from 'vue-router';

const tours = ref([]);
const loading = ref(true);
const error = ref('');
const router = useRouter();

const goToCheckpoint = (tourId) => {
  router.push({ path: '/map-checkpoint', query: { tourId } });
};

onMounted(async () => {
  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.get(`http://localhost:8082/api/tours`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    console.log('Tours fetched successfully:', response.data.value);
    tours.value = response.data.value;
  } catch (err) {
    error.value = 'Failed to load tours.';
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.tours-list-container {
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
</style>
