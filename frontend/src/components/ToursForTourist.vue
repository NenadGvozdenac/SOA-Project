<template>
  <div class="tours-tourist-container">
    <h2>Published Tours</h2>
    <div v-if="loading" class="loading">Loading tours...</div>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="tours.length" class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card">
        <h3>{{ tour.name }}</h3>
        <p><strong>Description:</strong> {{ tour.description }}</p>
        <p><strong>Difficulty:</strong> {{ difficultyLabels[tour.difficulty] || 'Unknown' }}</p>
        <p><strong>Price:</strong> {{ tour.price }} €</p>
        <div v-if="tour.tags && tour.tags.length">
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
      </div>
    </div>
    <div v-else-if="!loading && !error" class="no-tours">No published tours found.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
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

onMounted(async () => {
  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    // Filtriraj samo objavljene ture
    tours.value = (response.data.value || []).filter(t => t.status === 1 || t.status === 'Published');
    // Za svaku turu dohvati prvu ključnu tačku
    for (const tour of tours.value) {
      try {
        const cpRes = await axios.get(`http://localhost:8082/api/tours/checkpoints/${tour.id}`, {
          headers: { Authorization: `Bearer ${jwt}` }
        });
        const checkpoints = Array.isArray(cpRes.data.value) ? cpRes.data.value : cpRes.data.results || [];
        if (checkpoints.length > 0) {
          // Sortiraj po CreatedAt ako postoji
          checkpoints.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
          firstCheckpoint.value[tour.id] = checkpoints[0];
        }
      } catch {}
    }
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
</style>
