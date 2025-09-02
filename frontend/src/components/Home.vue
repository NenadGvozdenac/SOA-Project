<template>
  <div>
    <!-- Use the updated Navbar component -->
    <Navbar />

    <!-- Hero Section -->
    <section class="hero">
      <div class="container">
        <h1>Welcome to TourismHub</h1>
        <p>Your ultimate microservices platform for tourism, blogs, stakeholders, and followers management</p>
        <div class="hero-buttons" v-if="!isLoggedIn()">
          <router-link to="/register" class="btn btn-primary">Get Started</router-link>
          <router-link to="/login" class="btn btn-secondary">Sign In</router-link>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features">
      <div class="container">
        <!-- Logged in users see functional cards -->
        <template v-if="isLoggedIn()">
          <div v-if="isAuthor()" class="feature-card" @click="goToTours" style="cursor:pointer;">
            <h3>🗺️ Tour Management</h3>
            <p>Create, edit and publish your tours with advanced checkpoint mapping, route calculation, and pricing.
              Manage your entire tour catalog with interactive maps and real-time updates.</p>
          </div>

          <div v-if="isTourist()" class="feature-card" @click="goToToursForTourist" style="cursor:pointer;">
            <h3>🌍 Explore Tours</h3>
            <p>Discover amazing published tours from verified guides. Browse destinations, read reviews, and book your
              next adventure with detailed route maps and pricing information.</p>
          </div>

          <div class="feature-card" v-if="isTourist()" style="cursor:pointer;">
            <h3>🛒 Shopping Cart</h3>
            <p>Review your selected tours, manage your bookings, and complete your purchases securely. 
              Track your tour reservations and payment history.</p>
            <router-link to="/purchased-tours" class="btn btn-primary" style="margin-top: 1rem; display: inline-block;">My Purchased Tours</router-link>
          </div>

          <div class="feature-card" @click="goToBlogs" style="cursor:pointer;">
            <h3>📝 Travel Blogs</h3>
            <p>Share your travel experiences and read inspiring stories from fellow travelers. Create engaging blog
              content about your adventures and discover new destinations.</p>
          </div>

          <div class="feature-card" @click="goToProfiles" style="cursor:pointer;">
            <h3>👥 Users & Followers</h3>
            <p>Find users to follow, manage your followers and discover new profiles.
              Follow interesting travel bloggers and expand your network.</p>
          </div>

          <div class="feature-card" @click="goToFollowedBlogs" style="cursor:pointer;">
            <h3>📖 Blogs from Followed Users</h3>
            <p>Read the latest blogs from users you follow. Stay updated with their travels
              and get personalized content recommendations.</p>
          </div>

          <div class="feature-card" @click="goToProfile" style="cursor:pointer;">
            <h3>👤 My Profile</h3>
            <p>Edit your personal information, update your password, and manage your profile settings.
              Keep your information up to date for better user experience.</p>
          </div>
        </template>

        <!-- Non-logged in users see all feature descriptions -->
        <template v-else>
          <div class="feature-section">
            <h2 class="section-title">🗺️ For Tour Guides</h2>
            <div class="container">
              <div class="feature-card preview-card">
                <h3>Tour Management</h3>
                <p>Create, edit and publish your tours with advanced checkpoint mapping, route calculation, and pricing.
                  Manage your entire tour catalog with interactive maps and real-time updates.</p>
              </div>
            </div>
          </div>

          <div class="feature-section">
            <h2 class="section-title">🌍 For Tourists</h2>
            <div class="container">
              <div class="feature-card preview-card">
                <h3>Explore Tours</h3>
                <p>Discover amazing published tours from verified guides. Browse destinations, read reviews, and book
                  your
                  next adventure with detailed route maps and pricing information.</p>
              </div>
              <div class="feature-card preview-card">
                <h3>Shopping Cart & Purchased Tours</h3>
                <p>Review your selected tours, manage your bookings, and complete your purchases securely.
                  Track your tour reservations and payment history.</p>
              </div>
            </div>
          </div>

          <div class="feature-section">
            <h2 class="section-title">📝 For All Users</h2>
            <div class="container">
              <div class="feature-card preview-card">
                <h3>Travel Blogs</h3>
                <p>Share your travel experiences and read inspiring stories from fellow travelers. Create engaging blog
                  content about your adventures and discover new destinations.</p>
              </div>
              <div class="feature-card preview-card">
                <h3>Social Features</h3>
                <p>Find users to follow, manage your followers and discover new profiles.
                  Follow interesting travel bloggers and expand your network.</p>
              </div>
              <div class="feature-card preview-card">
                <h3>Personalized Content</h3>
                <p>Read the latest blogs from users you follow. Stay updated with their travels
                  and get personalized content recommendations.</p>
              </div>
            </div>
          </div>
        </template>
      </div>
    </section>
  </div>
</template>

<script setup>
import Navbar from './Navbar.vue'

const isLoggedIn = () => {
  return localStorage.getItem('token') !== null;
}

const isAdmin = () => {
  return localStorage.getItem('userRole') == 'Admin';
}

const isAuthor = () => {
  return localStorage.getItem('userRole') === 'Guide';
}

const isTourist = () => {
  return localStorage.getItem('userRole') === 'Tourist';
}

const goToTours = () => {
  window.location.href = '/tours';
}

const goToToursForTourist = () => {
  window.location.href = '/tours-for-tourist';
}

const goToBlogs = () => {
  window.location.href = '/blogs';
}

const goToProfiles = () => {
  window.location.href = '/profiles';
}

const goToFollowedBlogs = () => {
  window.location.href = '/followed-blogs';
}

const goToProfile = () => {
  window.location.href = '/profile';
}

</script>

<style scoped>

/* Preview card styles for non-logged users */
.feature-section {
  margin-bottom: 3rem;
}

.feature-section .container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
}

.section-title {
  color: #2c3e50;
  font-size: 1.5rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
  padding-bottom: 0.5rem;
  border-bottom: 2px solid #3498db;
  display: inline-block;
}

.preview-card {
  opacity: 0.8;
  border: 2px dashed #bdc3c7;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  position: relative;
  transition: all 0.3s ease;
  margin-bottom: 1rem;
}

.preview-card:hover {
  opacity: 1;
  border-color: #3498db;
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(52, 152, 219, 0.15);
}

.preview-card h3 {
  color: #34495e;
}

.preview-card p {
  color: #5a6c7d;
  margin-bottom: 1.5rem;
}

.login-prompt {
  text-align: center;
  margin-top: 1rem;
}

.btn-outline {
  background: transparent;
  color: #3498db;
  border: 2px solid #3498db;
  padding: 0.6rem 1.5rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.3s ease;
  display: inline-block;
}

.btn-outline:hover {
  background: #3498db;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(52, 152, 219, 0.3);
}
</style>
