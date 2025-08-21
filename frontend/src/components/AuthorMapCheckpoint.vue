<template>
  <div class="map-checkpoint-container">
    <h2>Add Key Checkpoint to Tour</h2>
    <div id="map" class="map"></div>
    <form @submit.prevent="submitCheckpoint" class="checkpoint-form">
      <div>
        <label>Name:</label>
        <input v-model="checkpoint.name" required />
      </div>
      <div>
        <label>Description:</label>
        <textarea v-model="checkpoint.description" required></textarea>
      </div>
      <div>
        <label>Image:</label>
        <input type="file" @change="onImageChange" accept="image/*" />
        <div v-if="checkpoint.imageBase64">
          <label>PREVIEW:</label>
          <img :src="`data:image/jpeg;base64,${checkpoint.imageBase64}`" alt="Preview" style="max-width:200px;max-height:200px;" />
        </div>
      </div>
      <div>
        <label>Latitude:</label>
        <input v-model="checkpoint.latitude" readonly />
      </div>
      <div>
        <label>Longitude:</label>
        <input v-model="checkpoint.longitude" readonly />
      </div>
      <button type="submit" class="btn btn-primary">Add Checkpoint</button>
    </form>
    <div v-if="message" class="message">{{ message }}</div>
  </div>
</template>

<script setup>

import { useRoute } from 'vue-router';
import { ref, onMounted } from 'vue';
import axios from 'axios';
const route = useRoute();

const checkpoint = ref({
  name: '',
  description: '',
  latitude: '',
  longitude: '',
  imageBase64: ''
});
const message = ref('');
let map, marker;

const onImageChange = (e) => {
  const file = e.target.files[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onload = () => {
    checkpoint.value.imageBase64 = reader.result.split(',')[1]; // samo base64 string
  };
  reader.readAsDataURL(file);
  console.log('checkpoint.imageBase64:', checkpoint.imageBase64);
};

const submitCheckpoint = async () => {
  try {
    console.log('USLO');
    const jwt = localStorage.getItem('token');
    const data = {
      tourId: route.query.tourId,
      name: checkpoint.value.name,
      description: checkpoint.value.description,
      latitude: checkpoint.value.latitude,
      longitude: checkpoint.value.longitude,
      imageBase64: checkpoint.value.imageBase64
    };
    console.log('Sending checkpoint data:', data);
    //await ToursService.addCheckpoint(data, jwt);
    const response = await axios.post(`http://localhost:8082/api/tours/checkpoint`, data, {
                headers: {
                    Authorization: `Bearer ${jwt}`,
                    'Content-Type': 'application/json'
                }
            });
    console.log('Response:', response.data);
    message.value = 'Checkpoint added successfully!';
    checkpoint.value = { name: '', description: '', latitude: '', longitude: '', imageBase64: '' };
    if (marker) marker.setLatLng([0,0]);
  } catch (err) {
    console.error('Error:', err);
    message.value = err?.response?.data?.message || err?.message || 'Error adding checkpoint.';
  }
};

onMounted(() => {
  // Use Leaflet for map rendering
  if (!window.L) {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://unpkg.com/leaflet/dist/leaflet.css';
    document.head.appendChild(link);
    const script = document.createElement('script');
    script.src = 'https://unpkg.com/leaflet/dist/leaflet.js';
    script.onload = initMap;
    document.body.appendChild(script);
  } else {
    initMap();
  }
});

function initMap() {
  map = window.L.map('map').setView([44.7866, 20.4489], 13); // Default: Belgrade
  window.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map);
  map.on('click', function(e) {
    const { lat, lng } = e.latlng;
    checkpoint.value.latitude = lat;
    checkpoint.value.longitude = lng;
    if (!marker) {
      marker = window.L.marker([lat, lng]).addTo(map);
    } else {
      marker.setLatLng([lat, lng]);
    }
  });
}
</script>

<style scoped>
.map-checkpoint-container {
  max-width: 600px;
  margin: 2rem auto;
  padding: 2rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.08);
}
.map {
  width: 100%;
  height: 350px;
  margin-bottom: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.07);
}
.checkpoint-form > div {
  margin-bottom: 1rem;
}
.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 8px;
  font-weight: 600;
  box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);
  cursor: pointer;
}
.message {
  margin-top: 1rem;
  color: #764ba2;
  font-weight: 600;
}
</style>
