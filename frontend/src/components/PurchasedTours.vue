<template>
  <div class="purchased-tours">
    <h1>Purchased Tours</h1>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <div v-if="tours.length === 0">You have no purchased tours.</div>
      <div v-else>
        <div v-for="tour in tours.value" :key="tour.id" class="tour-details">
          <h2>{{ tour.name }}</h2>
          <p><strong>Description:</strong> {{ tour.description }}</p>
          <p><strong>Author ID:</strong> {{ tour.authorId }}</p>
          <p><strong>Difficulty:</strong> {{ tour.difficulty }}</p>
          <p><strong>Tags:</strong> {{ tour.tags && tour.tags.length ? tour.tags.join(', ') : 'None' }}</p>
          <p><strong>Status:</strong> {{ tour.status }}</p>
          <p><strong>Price:</strong> {{ tour.price }} RSD</p>
          <p><strong>Published:</strong> {{ tour.publishedAt }}</p>
          <p><strong>Archived:</strong> {{ tour.archivedAt }}</p>
          <p><strong>Length (km):</strong> {{ tour.lengthKm }}</p>
          <p><strong>Created:</strong> {{ tour.createdAt }}</p>
          <button @click="goToReview(tour.id)" class="review-btn">Rate Tour</button>
          <div v-if="tour.checkpoints && tour.checkpoints.length">
            <h3>Checkpoints:</h3>
            <ul>
              <li v-for="cp in tour.checkpoints" :key="cp.id">
                <strong>{{ cp.name }}</strong> - {{ cp.description }}<br>
                <span>Lat: {{ cp.latitude }}, Lon: {{ cp.longitude }}</span>
                <div v-if="cp.imageBase64">
                  <img :src="'data:image/png;base64,' + cp.imageBase64" alt="Checkpoint Image" style="max-width:150px; margin-top:5px;" />
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { useRouter } from 'vue-router';

const tours = ref([]);
const loading = ref(true);
const router = useRouter();

onMounted(async () => {
  try {
    const token = localStorage.getItem('token');
    const response = await axios.get('http://localhost:8082/api/tours/boughttours', {
      headers: { Authorization: `Bearer ${token}` }
    });
    tours.value = response.data;
  } catch (error) {
    tours.value = [];
  } finally {
    loading.value = false;
  }
});

const goToReview = (tourId) => {
  router.push({ path: '/add-tour-review', query: { tourId } });
};
</script>

<style scoped>
.purchased-tours {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}
.tour-details {
  border: 1px solid #ccc;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 2rem;
  background: #f9f9f9;
}
h2 {
  margin-top: 0;
}
.review-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  margin-top: 1rem;
  transition: transform 0.2s;
}
.review-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}
</style>
