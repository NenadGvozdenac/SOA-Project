<template>
  <div class="map-checkpoint-container">
    <div v-if="tourInfo" class="tour-info">
      <h2>{{ tourInfo.name }}</h2>
      <div><strong>Opis:</strong> {{ tourInfo.description }}</div>
      <div><strong>Težina:</strong> {{ difficultyMap[tourInfo.difficulty] }}</div>
      <div><strong>Status:</strong> {{ statusMap[tourInfo.status] }}</div>
      <div><strong>Cena:</strong> {{ tourInfo.price }} EUR</div>
      <div><strong>Tagovi:</strong> <span v-for="tag in tourInfo.tags" :key="tag">{{ tag }}</span></div>
      <div v-if="tourInfo.publishedAt"><strong>Objavljena:</strong> {{ formatDate(tourInfo.publishedAt) }}</div>
      <div v-if="tourInfo.archivedAt"><strong>Arhivirana:</strong> {{ formatDate(tourInfo.archivedAt) }}</div>
    </div>
    <div v-if="actionMessage" class="action-message">{{ actionMessage }}</div>
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
        <div v-if="tourLength > 0" style="margin-bottom:1rem;">
          <strong>Dužina ture (putem):</strong> {{ tourLength.toFixed(2) }} km
        </div>
        <div v-if="checkpoints.length > 0" style="margin-top:2rem;">
          <button class="btn btn-publish" @click="showPublishModal = true">Objavi turu</button>
          <button class="btn btn-archive" @click="archiveTour()">Arhiviraj turu</button>
        </div>

        <!-- Modal za unos cene -->
        <div v-if="showPublishModal" class="modal-overlay">
          <div class="modal-content">
            <h3>Unesi cenu za objavljivanje ture</h3>
            <div class="publish-info" style="margin-bottom:1rem;">
              <strong>Uslovi za objavu ture:</strong>
              <ul>
                <li>Tura mora imati naziv, opis, težinu i bar jedan tag.</li>
                <li>Tura mora imati bar dve ključne tačke.</li>
                <li>Vreme obilaska se automatski računa na osnovu dužine ture i tipa prevoza:</li>
                <ul>
                  <li>Peške: {{ getVisitDuration(tourLength).walking }} min</li>
                  <li>Biciklom: {{ getVisitDuration(tourLength).bicycle }} min</li>
                  <li>Automobilom: {{ getVisitDuration(tourLength).car }} min</li>
                </ul>
              </ul>
              <div v-if="!canPublishTour" style="color:#ff5858;font-weight:600;">
                Tura trenutno ne ispunjava sve uslove za objavu!
              </div>
            </div>
            <input type="number" v-model="publishPrice" min="0" step="0.01" placeholder="Cena (EUR)" />
            <div style="margin-top:1rem;">
              <button class="btn btn-primary" @click="publishTour" :disabled="!canPublishTour">Objavi</button>
              <button class="btn btn-secondary" @click="showPublishModal = false" style="margin-left:1rem;">Otkaži</button>
            </div>
            <div v-if="publishMessage" class="message">{{ publishMessage }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>


import { useRoute } from 'vue-router';
import { ref, onMounted, watch, computed } from 'vue';
import axios from 'axios';
import { AuthService } from '../services/auth_service.js';
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
const tourLength = ref(0);
const showPublishModal = ref(false);
const publishPrice = ref(0);
const publishMessage = ref('');
const tourInfo = ref(null);
const actionMessage = ref('');
const difficultyMap = { 0: 'Easy', 1: 'Medium', 2: 'Hard', 'Easy': 'Easy', 'Medium': 'Medium', 'Hard': 'Hard' };
const statusMap = { 0: 'Draft', 1: 'Published', 2: 'Archived', 'Draft': 'Draft', 'Published': 'Published', 'Archived': 'Archived' };

// Function to format date from timestamp to readable format
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('sr-RS', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const deleteCheckpoint = async (id) => {
  try {
    const jwt = localStorage.getItem('token');
    const userInfo = AuthService.decode(jwt);
    // Koristi novi gRPC endpoint preko gateway-net sa query parametrima
    await axios.delete(`http://localhost:8084/api/checkpoints/${id}?user_id=${userInfo?.id || ''}&auth_token=${jwt}`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    message.value = 'Checkpoint deleted successfully!';
    await fetchCheckpoints();
  } catch (err) {
    message.value = err?.response?.data?.message || err?.message || 'Error deleting checkpoint.';
  }
};

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

onMounted(async () => {
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

  const jwt = localStorage.getItem('token');
  const tourId = route.query.tourId;
  try {
    const toursResponse = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    const tours = toursResponse.data.value || [];
    const tour = tours.find(t => t.id == tourId);
    if (tour) tourInfo.value = tour;
  } catch (err) {
    actionMessage.value = 'Greška pri dohvatanju informacija o turi.';
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

async function getRouteDistance(lat1, lon1, lat2, lon2) {
  const apiKey = 'eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjM4Zjk2ZDdlNTlhZDQwYWQ4NDg3N2UwMDhhNjhmODE1IiwiaCI6Im11cm11cjY0In0='; // zameni sa svojim ključem
  const url = `https://api.openrouteservice.org/v2/directions/driving-car?api_key=${apiKey}&start=${lon1},${lat1}&end=${lon2},${lat2}`;
  const response = await fetch(url);
  const data = await response.json();
  return data.features[0].properties.summary.distance / 1000; // u km
}

async function calculateTourLength(checkpoints) {
  // Sortiraj checkpointove po CreatedAt od najstarijeg do najnovijeg
  const sortedCheckpoints = [...checkpoints].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
  console.log('Sorted checkpoints:', sortedCheckpoints);
  let length = 0;
  for (let i = 1; i < sortedCheckpoints.length; i++) {
    length += await getRouteDistance(
      sortedCheckpoints[i-1].latitude,
      sortedCheckpoints[i-1].longitude,
      sortedCheckpoints[i].latitude,
      sortedCheckpoints[i].longitude
    );
  }
  return length;
}

async function drawRoutePolyline(checkpoints) {
  if (polyline) map.removeLayer(polyline);
  let routeCoords = [];
  const apiKey = 'eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjM4Zjk2ZDdlNTlhZDQwYWQ4NDg3N2UwMDhhNjhmODE1IiwiaCI6Im11cm11cjY0In0='; // zameni sa svojim ključem
  for (let i = 1; i < checkpoints.length; i++) {
    const url = `https://api.openrouteservice.org/v2/directions/driving-car?api_key=${apiKey}&start=${checkpoints[i-1].longitude},${checkpoints[i-1].latitude}&end=${checkpoints[i].longitude},${checkpoints[i].latitude}`;
    const response = await fetch(url);
    const data = await response.json();
    if (data.features && data.features[0]) {
      const coords = data.features[0].geometry.coordinates.map(c => [c[1], c[0]]);
      // Izbegni duplikate na spojevima
      if (routeCoords.length > 0 && coords.length > 0 && routeCoords[routeCoords.length-1][0] === coords[0][0] && routeCoords[routeCoords.length-1][1] === coords[0][1]) {
        coords.shift();
      }
      routeCoords = routeCoords.concat(coords);
    }
  }
  if (routeCoords.length > 1) {
    polyline = window.L.polyline(routeCoords, { color: 'blue' }).addTo(map);
  }
}

watch(checkpoints, async (val) => {
  if (val.length >= 2) {
    tourLength.value = await calculateTourLength(val);
    await drawRoutePolyline(val);

    // Pozovi update ture
    const jwt = localStorage.getItem('token');
    const tourId = route.query.tourId;
    // Prvo dohvati sve ture autora
    const toursResponse = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    const tours = toursResponse.data.value || [];
    console.log('Dohvaćene ture:', tours);
    // Nadji turu po id-u
    const tour = tours.find(t => t.id == tourId);
    console.log('TURA:', tour);
    if (tour) {
      const updateBody = {
        name: tour.name,
        description: tour.description,
        difficulty: difficultyMap[tour.difficulty],
        tags: tour.tags,
        price: tour.price,
        status: statusMap[tour.status],
        lengthKm: tourLength.value
      };
      console.log('updateBody:', updateBody);
      await axios.put(`http://localhost:8082/api/tours/update/${tourId}`, updateBody, {
        headers: { Authorization: `Bearer ${jwt}` }
      });
    }



    
  } else {
    tourLength.value = 0;
    if (polyline) map.removeLayer(polyline);
  }
});

const archiveTour = async () => {
  try {
    const jwt = localStorage.getItem('token');
    const userInfo = AuthService.decode(jwt);
    const tourId = route.query.tourId;
    // Koristi novi gRPC endpoint preko gateway-net sa query parametrima
    await axios.patch(`http://localhost:8084/api/tours/${tourId}/archive?user_id=${userInfo?.id || ''}&auth_token=${jwt}`, {}, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    actionMessage.value = 'Tura je uspešno arhivirana!';
    // Osveži info
    const toursResponse = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    const tours = toursResponse.data.value || [];
    const tour = tours.find(t => t.id == tourId);
    if (tour) tourInfo.value = tour;
  } catch (err) {
    actionMessage.value = err?.response?.data?.message || err?.message || 'Greška pri arhiviranju ture.';
  }
};

const publishTour = async () => {
  try {
    const jwt = localStorage.getItem('token');
    const tourId = route.query.tourId;
    await axios.post(`http://localhost:8082/api/tours/publish/${tourId}/${publishPrice.value}`, {}, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    actionMessage.value = 'Tura je uspešno objavljena!';
    showPublishModal.value = false;
    // Osveži info
    const toursResponse = await axios.get('http://localhost:8082/api/tours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    const tours = toursResponse.data.value || [];
    const tour = tours.find(t => t.id == tourId);
    if (tour) tourInfo.value = tour;
  } catch (err) {
    actionMessage.value = err?.response?.data?.message || err?.message || 'Greška pri objavljivanju ture.';
  }
};

function getVisitDuration(lengthKm) {
  return {
    walking: Math.ceil(lengthKm / 5 * 60),      // minuta
    bicycle: Math.ceil(lengthKm / 15 * 60),     // minuta
    car: Math.ceil(lengthKm / 80 * 60)          // minuta
  };
}
const canPublishTour = computed(() => {
  if (!tourInfo.value) return false;
  return (
    tourInfo.value.name &&
    tourInfo.value.description &&
    tourInfo.value.difficulty !== undefined &&
    tourInfo.value.tags && tourInfo.value.tags.length > 0 &&
    checkpoints.value.length >= 2
  );
});
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
.btn-publish {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 8px;
  font-weight: 600;
  margin-right: 1rem;
  cursor: pointer;
}
.btn-archive {
  background: linear-gradient(135deg, #ff5858 0%, #f09819 100%);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}
.message {
  margin-top: 1rem;
  color: #764ba2;
  font-weight: 600;
}
.tour-length {
  margin-top: 1rem;
  font-size: 1.1rem;
  font-weight: 500;
}
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0,0,0,0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.modal-content {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.12);
  min-width: 320px;
}
.tour-info {
  background: #f7f7fa;
  border-radius: 8px;
  padding: 1rem 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.07);
}
.tour-info h2 {
  margin-bottom: 0.5rem;
}
.tour-info div {
  margin-bottom: 0.3rem;
}
.action-message {
  margin-bottom: 1rem;
  color: #43e97b;
  font-weight: 600;
  font-size: 1.1rem;
}
</style>
