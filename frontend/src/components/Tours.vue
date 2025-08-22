<template>
  <div class="tours-list-container">
    <h2>All Tours</h2>

    <div class="add-tour-form">
      <h3>Add New Tour</h3>
      <form @submit.prevent="addTour">
        <input v-model="newTour.name" placeholder="Name" required />
        <textarea v-model="newTour.description" placeholder="Description" required></textarea>
        <select v-model="newTour.difficulty" required>
          <option disabled value="">Select Difficulty</option>
          <option value="Easy">Easy</option>
          <option value="Medium">Medium</option>
          <option value="Hard">Hard</option>
        </select>
        <input v-model="tagsInput" placeholder="Enter tags separated by comma" />
        <div>
          <span v-for="tag in newTour.tags" :key="tag" class="tag">{{ tag }}</span>
        </div>
        <button type="submit">Add Tour</button>
      </form>
      <div v-if="addError" class="error">{{ addError }}</div>
    </div>

    <div v-if="loading" class="loading">Loading tours...</div>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="tours.length" class="tours-grid">
      <div v-for="tour in tours" :key="tour.id" class="tour-card" @click="goToCheckpoint(tour.id)" style="cursor:pointer;">
        <h3>{{ tour.name }}</h3>
        <p><strong>Description:</strong> {{ tour.description }}</p>
        <p><strong>Difficulty:</strong>
          <span :class="'difficulty-' + (difficultyLabels[tour.difficulty]?.toLowerCase() || 'unknown')">
            {{ difficultyLabels[tour.difficulty] || 'Unknown' }}
          </span>
        </p>
        <p>
          <strong>Status:</strong>
          <span :class="'status-' + (statusLabels[tour.status]?.toLowerCase() || 'unknown')">
            {{ statusLabels[tour.status] || 'Unknown' }}
          </span>
        </p>
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

const statusLabels = {
  0: 'Draft',
  1: 'Published',
  2: 'Archived',
  'Draft': 'Draft',
  'Published': 'Published',
  'Archived': 'Archived'
};
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
const newTour = ref({
  name: '',
  description: '',
  difficulty: '',
  price: 0,
  tags: []
});
const tagsInput = ref('');
const addError = ref('');

const addTour = async () => {
  try {
    const jwt = localStorage.getItem('token');
    // Parse tags from input
    newTour.value.tags = tagsInput.value
      .split(',')
      .map(tag => tag.trim())
      .filter(tag => tag.length > 0);
    const payload = {
      name: newTour.value.name,
      description: newTour.value.description,
      difficulty: newTour.value.difficulty,
      tags: newTour.value.tags
    };
    const response = await axios.post('http://localhost:8082/api/tours', payload, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    addError.value = '';
    await fetchTours();
    newTour.value = { name: '', description: '', difficulty: '', price: 0, tags: [] };
    tagsInput.value = '';
    // Prebaci na AuthorMapCheckpoint za novu turu
    if (response.data && response.data.value && response.data.value.id) {
      goToCheckpoint(response.data.value.id);
    }
  } catch (err) {
    addError.value = err?.response?.data?.message || 'Failed to add tour.';
  }
};

const fetchTours = async () => {
  loading.value = true;
  try {
    const jwt = localStorage.getItem('token');
    const response = await axios.get(`http://localhost:8082/api/tours`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    tours.value = response.data.value;
    error.value = '';
  } catch (err) {
    error.value = 'Failed to load tours.';
  } finally {
    loading.value = false;
  }
};

const goToCheckpoint = (tourId) => {
  router.push({ path: '/map-checkpoint', query: { tourId } });
};

onMounted(fetchTours);
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
.add-tour-form {
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: #f3f4f6;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.add-tour-form input,
.add-tour-form textarea,
.add-tour-form select {
  display: block;
  width: 100%;
  margin-bottom: 1rem;
  padding: 0.7rem;
  border-radius: 6px;
  border: 1px solid #d1d5db;
}
.add-tour-form button {
  background: #667eea;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.7rem 1.5rem;
  cursor: pointer;
  font-weight: 600;
}
.status-draft {
  color: orange;
  font-weight: bold;
}
.status-published {
  color: green;
  font-weight: bold;
}
.status-archived {
  color: gray;
  font-weight: bold;
}
.add-tour-form .difficulty-easy {
  color: #2ecc40;
  font-weight: bold;
}
.add-tour-form .difficulty-medium {
  color: #f1c40f;
  font-weight: bold;
}
.add-tour-form .difficulty-hard {
  color: #e74c3c;
  font-weight: bold;
}
.tour-card .difficulty-easy {
  color: #2ecc40;
  font-weight: bold;
}
.tour-card .difficulty-medium {
  color: #f1c40f;
  font-weight: bold;
}
.tour-card .difficulty-hard {
  color: #e74c3c;
  font-weight: bold;
}
</style>
