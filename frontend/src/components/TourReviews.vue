<template>
  <div class="tour-reviews">
    <h4>Reviews</h4>
    <div v-if="reviews.length === 0">No reviews</div>
    <div v-for="review in reviews" :key="review.id" class="review">
      <div><b>Rating:</b> {{ review.rating }}</div>
      <div><b>Comment:</b> {{ review.comment }}</div>
    </div>
    <div v-if="averageRating > 0" class="average-rating">
      <b>Average Rating:</b> {{ averageRating.toFixed(2) }}
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

    async function fetchReviews() {
      const jwt = localStorage.getItem('token')
      const response = await axios.get(`http://localhost:8082/api/tours/tourreviews/${props.tourId}`, {
        headers: { Authorization: `Bearer ${jwt}` }
      })
      reviews.value = response.data.value || []
      averageRating.value = response.data.averageRating || 0
    }

    onMounted(fetchReviews)
    watch(() => props.tourId, fetchReviews)

    return { reviews, averageRating }
  }
}
</script>

<style scoped>
.tour-reviews {
  margin-top: 1rem;
  background: #f9f9f9;
  padding: 1rem;
  border-radius: 8px;
}
.review {
  margin-bottom: 0.5rem;
}
.average-rating {
  margin-top: 0.5rem;
  font-weight: bold;
}
</style>
