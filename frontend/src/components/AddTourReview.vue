<template>
  <div class="add-tour-review">
    <div class="container">
      <h1>Rate Your Tour Experience</h1>
      
      <!-- Tour Information Display -->
      <div v-if="tourInfo" class="tour-info">
        <h2>{{ tourInfo.name }}</h2>
        <p><strong>Description:</strong> {{ tourInfo.description }}</p>
        <p><strong>Author ID:</strong> {{ tourInfo.authorId }}</p>
        <p><strong>Difficulty:</strong> {{ tourInfo.difficulty }}</p>
        <p><strong>Tags:</strong> {{ tourInfo.tags && tourInfo.tags.length ? tourInfo.tags.join(', ') : 'None' }}</p>
      </div>

      <!-- Review Form -->
      <div class="review-form">
        <h3>Share Your Experience</h3>
        <form @submit.prevent="submitReview">
          
          <!-- Rating -->
          <div class="form-group">
            <label for="rating">Rating (1-5 stars):</label>
            <div class="star-rating">
              <span 
                v-for="star in 5" 
                :key="star"
                @click="setRating(star)"
                :class="{ active: star <= review.rating }"
                class="star"
              >
                ★
              </span>
            </div>
            <span class="rating-text">{{ review.rating }}/5</span>
          </div>

          <!-- Tourist Information -->
          <div class="form-group">
            <label for="comment">Your Review:</label>
            <textarea 
              v-model="review.comment" 
              id="comment"
              rows="6"
              placeholder="Share your experience with this tour..."
              required
            ></textarea>
          </div>

          <!-- Visit Date -->
          <div class="form-group">
            <label for="checkTourDate">When did you visit this tour?</label>
            <input 
              type="date" 
              v-model="review.checkTourDate" 
              id="checkTourDate"
              required
            />
          </div>

          <!-- Images Upload -->
          <div class="form-group">
            <label for="images">Upload ONE Photo from Your Tour:</label>
            <input 
              type="file" 
              @change="onImageChange" 
              accept="image/*"
              id="images"
            />
            <div v-if="review.imageBase64" class="image-preview-single">
              <img :src="`data:image/jpeg;base64,${review.imageBase64}`" alt="Tour photo" />
              <button type="button" @click="removeImage" class="remove-image">×</button>
            </div>
          </div>

          <!-- Submit Button -->
          <div class="form-group">
            <button type="submit" class="submit-btn" :disabled="!isFormValid">
              Submit Review
            </button>
          </div>

        </form>

        <!-- Success/Error Messages -->
        <div v-if="message" :class="['message', messageType]">
          {{ message }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';
import { AuthService } from '../services/auth_service.js';

const route = useRoute();
const router = useRouter();

const tourInfo = ref(null);
const message = ref('');
const messageType = ref('');

const review = ref({
  rating: 0,
  comment: '',
  checkTourDate: '',
  imageBase64: ''
});

// Computed property to check if form is valid
const isFormValid = computed(() => {
  return review.value.rating > 0 && 
         review.value.comment.trim() && 
         review.value.checkTourDate;
});

// Set rating from star click
const setRating = (rating) => {
  review.value.rating = rating;
};

// Handle single image upload
const onImageChange = (event) => {
  const file = event.target.files[0];
  if (!file) return;
  
  const reader = new FileReader();
  reader.onload = () => {
    review.value.imageBase64 = reader.result.split(',')[1]; // Only base64 string
  };
  reader.readAsDataURL(file);
};

// Remove image from preview
const removeImage = () => {
  review.value.imageBase64 = '';
};

// Fetch tour information
const fetchTourInfo = async () => {
  try {
    const jwt = localStorage.getItem('token');
    const tourId = route.query.tourId;
    
    // Get all purchased tours and find the specific one
    const response = await axios.get('http://localhost:8082/api/tours/boughttours', {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    
    const tours = response.data.value || [];
    const tour = tours.find(t => t.id == tourId);
    if (tour) {
      tourInfo.value = tour;
    }
  } catch (err) {
    message.value = 'Error loading tour information.';
    messageType.value = 'error';
  }
};

// Submit review
const submitReview = async () => {
  try {
    const jwt = localStorage.getItem('token');
    const userInfo = AuthService.decode(jwt);
    const tourId = route.query.tourId;

    const reviewData = {
      tourId: parseInt(tourId),
      rating: review.value.rating,
      comment: review.value.comment,
      checkTourDate: new Date(review.value.checkTourDate + 'T00:00:00.000Z').toISOString(),
      imageBase64: review.value.imageBase64 || null
    };
    console.log('Submitting review data:', reviewData);
    await axios.post('http://localhost:8082/api/tours/review', reviewData, {
      headers: { 
        Authorization: `Bearer ${jwt}`,
        'Content-Type': 'application/json'
      }
    });

    message.value = 'Review submitted successfully!';
    messageType.value = 'success';
    
    // Redirect back to purchased tours after a delay
    setTimeout(() => {
      router.push('/purchased-tours');
    }, 2000);

  } catch (err) {
    message.value = err?.response?.data?.message || err?.message || 'Error submitting review.';
    messageType.value = 'error';
  }
};

onMounted(() => {
  fetchTourInfo();
});
</script>

<style scoped>
.add-tour-review {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem 0;
}

.container {
  max-width: 800px;
  margin: 0 auto;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.1);
  overflow: hidden;
}

h1 {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  margin: 0;
  padding: 2rem;
  text-align: center;
  font-size: 2rem;
}

.tour-info {
  padding: 1.5rem 2rem;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.tour-info h2 {
  margin: 0 0 1rem 0;
  color: #333;
}

.review-form {
  padding: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
}

input[type="date"],
textarea {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #e9ecef;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

input[type="date"]:focus,
textarea:focus {
  outline: none;
  border-color: #667eea;
}

.star-rating {
  display: inline-flex;
  gap: 0.25rem;
  margin-right: 1rem;
}

.star {
  font-size: 2rem;
  color: #ddd;
  cursor: pointer;
  transition: color 0.2s;
  user-select: none;
}

.star.active {
  color: #ffd700;
}

.star:hover {
  color: #ffd700;
}

.rating-text {
  font-weight: 600;
  color: #667eea;
}

.image-preview-single {
  margin-top: 1rem;
  position: relative;
  display: inline-block;
  border-radius: 8px;
  overflow: hidden;
  max-width: 200px;
}

.image-preview-single img {
  width: 100%;
  height: auto;
  max-height: 200px;
  object-fit: cover;
}

.remove-image {
  position: absolute;
  top: 0.25rem;
  right: 0.25rem;
  background: rgba(255, 0, 0, 0.8);
  color: white;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  cursor: pointer;
  font-size: 1rem;
  line-height: 1;
}

.submit-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 8px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s;
  width: 100%;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.message {
  margin-top: 1rem;
  padding: 1rem;
  border-radius: 8px;
  font-weight: 600;
  text-align: center;
}

.message.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.message.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

@media (max-width: 768px) {
  .container {
    margin: 0 1rem;
  }
  
  h1 {
    font-size: 1.5rem;
    padding: 1.5rem;
  }
  
  .review-form {
    padding: 1rem;
  }
}
</style>
