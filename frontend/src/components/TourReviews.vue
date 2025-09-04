<template>
  <div class="tour-reviews">
    <h4>Tour Reviews</h4>
    <div v-if="reviews.length === 0" class="no-reviews">No reviews yet</div>
    <div v-for="review in reviews" :key="review.id" class="review-card">
      <div class="review-header">
        <div class="reviewer-info">
          <strong>{{ review.reviewerId || 'Anonymous' }}</strong>
          <span class="review-date">{{ formatDate(review.createdAt) }}</span>
        </div>
        <div class="rating">
          <span class="stars">
            <span v-for="star in 5" :key="star" :class="{ active: star <= review.rating }">★</span>
          </span>
          <span class="rating-number">{{ review.rating }}/5</span>
        </div>
      </div>
      
      <div class="review-content">
        <p><strong>Review:</strong> {{ review.comment }}</p>
        <p v-if="review.checkTourDate" class="visit-date">
          <strong>Visited on:</strong> {{ formatDate(review.checkTourDate) }}
        </p>
        
        <div v-if="review.imageBase64" class="review-image">
          <img :src="`data:image/jpeg;base64,${review.imageBase64}`" alt="Review photo" />
        </div>
      </div>
    </div>
    
    <div v-if="averageRating > 0" class="average-rating">
      <strong>Average Rating: {{ averageRating.toFixed(1) }}/5</strong>
      <div class="stars-average">
        <span v-for="star in 5" :key="star" :class="{ active: star <= Math.round(averageRating) }">★</span>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue'
import axios from 'axios';

export default {
  props: {
    tourId: { type: Number, required: true }
  },
  setup(props) {
    const reviews = ref([])
    const averageRating = ref(0)

    // Format date for display
    const formatDate = (dateString) => {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    }

    async function fetchReviews() {
      try {
        const jwt = localStorage.getItem('token')
        const response = await axios.get(`http://localhost:8082/api/tours/tourreviews/${props.tourId}`, {
          headers: { Authorization: `Bearer ${jwt}` }
        })
        console.log('Fetched reviews:', response.data)
        reviews.value = response.data.value || []
        averageRating.value = response.data.averageRating || 0
      } catch (error) {
        console.error('Error fetching reviews:', error)
        reviews.value = []
        averageRating.value = 0
      }
    }

    onMounted(fetchReviews)
    watch(() => props.tourId, fetchReviews)

    return { reviews, averageRating, formatDate }
  }
}
</script>

<style scoped>
.tour-reviews {
  margin-top: 1rem;
  background: #f9f9f9;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.no-reviews {
  text-align: center;
  color: #666;
  font-style: italic;
  padding: 2rem;
}

.review-card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  border-left: 4px solid #667eea;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.reviewer-info {
  display: flex;
  flex-direction: column;
}

.reviewer-info strong {
  color: #333;
  font-size: 1.1rem;
}

.review-date {
  color: #666;
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.rating {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.stars {
  display: flex;
  gap: 0.1rem;
}

.stars span {
  color: #ddd;
  font-size: 1.2rem;
}

.stars span.active {
  color: #ffd700;
}

.rating-number {
  font-weight: 600;
  color: #667eea;
}

.review-content p {
  margin: 0.5rem 0;
  line-height: 1.5;
}

.visit-date {
  color: #666;
  font-size: 0.9rem;
}

.review-image {
  margin-top: 1rem;
}

.review-image img {
  max-width: 200px;
  max-height: 200px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  object-fit: cover;
}

.average-rating {
  background: #667eea;
  color: white;
  padding: 1rem;
  border-radius: 8px;
  text-align: center;
  margin-top: 1.5rem;
}

.stars-average {
  display: flex;
  justify-content: center;
  gap: 0.1rem;
  margin-top: 0.5rem;
}

.stars-average span {
  color: rgba(255,255,255,0.3);
  font-size: 1.5rem;
}

.stars-average span.active {
  color: #ffd700;
}

@media (max-width: 768px) {
  .review-header {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .rating {
    align-self: flex-start;
  }
}
</style>
