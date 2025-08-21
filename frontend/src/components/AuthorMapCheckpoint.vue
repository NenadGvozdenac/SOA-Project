<template>
  <div class="map-checkpoint-container">
    <h2>Add Key Checkpoint to Tour</h2>
    <div style="display: flex; gap: 2rem; align-items: flex-start;">
      <div style="flex: 2;">
        <div id="map" class="map"></div>
      </div>
      <div style="flex: 1;">
        <form @submit.prevent="editingCheckpoint ? submitEdit() : submitCheckpoint()" class="checkpoint-form">
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
          <button type="submit" class="btn btn-primary">{{ editingCheckpoint ? 'Update' : 'Add' }} Checkpoint</button>
          <button v-if="editingCheckpoint" type="button" class="btn btn-secondary" @click="cancelEdit" style="margin-left:1rem;">Cancel</button>
        </form>
        <div v-if="message" class="message">{{ message }}</div>
        <div class="checkpoint-list">
          <h3>All Checkpoints</h3>
          <div v-if="checkpoints.length === 0">No checkpoints for this tour.</div>
          <div v-for="cp in checkpoints" :key="cp.id" class="checkpoint-card">
            <div><strong>{{ cp.name }}</strong></div>
            <div>{{ cp.description }}</div>
            <div>Lat: {{ cp.latitude }}, Lng: {{ cp.longitude }}</div>
            <div v-if="cp.imageBase64">
              <label>Image:</label>
              <img :src="`data:image/jpeg;base64,${cp.imageBase64}`" alt="Preview" style="max-width:200px;max-height:200px;" />
            </div>
            <button class="btn btn-edit" @click="editCheckpoint(cp)">Edit</button>
            <button class="btn btn-delete" @click="deleteCheckpoint(cp.id)">Delete</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const deleteCheckpoint = async (id) => {
  try {
    const jwt = localStorage.getItem('token');
    await axios.delete(`http://localhost:8082/api/tours/checkpoint/delete/${id}`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    message.value = 'Checkpoint deleted successfully!';
    await fetchCheckpoints();
  } catch (err) {
    message.value = err?.response?.data?.message || err?.message || 'Error deleting checkpoint.';
  }
};

import { useRoute } from 'vue-router';
import { ref, onMounted } from 'vue';
import axios from 'axios';
const route = useRoute();
const checkpoints = ref([]);
const editingCheckpoint = ref(null);
let polyline;

const checkpoint = ref({
  name: '',
  description: '',
  latitude: '',
  longitude: '',
  imageBase64: ''
});
const message = ref('');
let map, marker;

function editCheckpoint(cp) {
  editingCheckpoint.value = { ...cp };
  checkpoint.value = { ...cp };
  // Selektovanje pozicije na mapi za izmenu: klik na mapu menja koordinate
  if (map) {
    map.off('click');
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
}
function cancelEdit() {
  editingCheckpoint.value = null;
  checkpoint.value = { name: '', description: '', latitude: '', longitude: '', imageBase64: '' };
  if (map) {
    map.off('click');
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
}
const submitEdit = async () => {
  try {
    const jwt = localStorage.getItem('token');
    await axios.put(`http://localhost:8082/api/tours/checkpoint/update/${editingCheckpoint.value.id}`, checkpoint.value, {
      headers: { Authorization: `Bearer ${jwt}`, 'Content-Type': 'application/json' }
    });
    console.log('Checkpoint updated:', checkpoint.value);
    message.value = 'Checkpoint updated successfully!';
    editingCheckpoint.value = null;
    checkpoint.value = { name: '', description: '', latitude: '', longitude: '', imageBase64: '' };
    if (map) {
      map.off('click');
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
    await fetchCheckpoints();
  } catch (err) {
    message.value = err?.response?.data?.message || err?.message || 'Error updating checkpoint.';
  }
};
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
    
    await fetchCheckpoints();
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
    script.onload = () => { initMap(); fetchCheckpoints(); };
    document.body.appendChild(script);
  } else {
    initMap();
    fetchCheckpoints();
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

const fetchCheckpoints = async () => {
  const jwt = localStorage.getItem('token');
  const tourId = route.query.tourId;
  const response = await axios.get(`http://localhost:8082/api/tours/checkpoints/${tourId}`, {
    headers: { Authorization: `Bearer ${jwt}` }
  });
  console.log('Fetched checkpoints:', response.data.value);
  checkpoints.value = Array.isArray(response.data.value) ? response.data.value : response.data.results || [];
  drawTourPolyline();
  drawMarkers();
};

function drawTourPolyline() {
  if (polyline) {
    map.removeLayer(polyline);
  }
const latlngs = checkpoints.value.map(cp => [Number(cp.latitude), Number(cp.longitude)]);
  if (latlngs.length > 1) {
    polyline = window.L.polyline(latlngs, { color: 'blue' }).addTo(map);
  }
}
function drawMarkers() {
  if (window.checkpointMarkers) {
    window.checkpointMarkers.forEach(m => map.removeLayer(m));
  }
  window.checkpointMarkers = [];
  checkpoints.value.forEach(cp => {
    if (typeof cp.latitude === 'number' && typeof cp.longitude === 'number') {
      const m = window.L.marker([cp.latitude, cp.longitude]).addTo(map)
        .bindPopup(cp.name);
      window.checkpointMarkers.push(m);
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
