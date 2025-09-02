<template>
  <nav class="nav">
    <div class="nav-container">
      <router-link to="/" class="logo">TourismHub</router-link>
      <div class="nav-links">
        <router-link to="/" class="nav-link">
          <i class="icon">🏠</i>
          Home
        </router-link>
        <router-link to="/profile" class="nav-link" v-if="isLoggedIn()">
          <i class="icon">👤</i>
          My Profile
        </router-link>
        <router-link to="/position-simulator" class="nav-link" v-if="isLoggedIn() && getUserRole() === 'Tourist'">
          <i class="icon">📍</i>
          Position Simulator
        </router-link>
        <router-link to="/active-tour" class="nav-link" v-if="isLoggedIn() && getUserRole() === 'Tourist'">
          <i class="icon">🚶</i>
          Active Tour
        </router-link>
        <router-link to="/tours-for-tourist" class="nav-link" v-if="isLoggedIn() && getUserRole() === 'Tourist'">
          <i class="icon">🗺️</i>
          Tours
        </router-link>
        <!-- Shopping Cart for Tourists -->
        <ShoppingCart v-if="isLoggedIn() && getUserRole() === 'Tourist'" />
        <button class="btn btn-secondary" v-if="isLoggedIn()" @click="handleLogout">
          <i class="icon">🚪</i>
          Logout
        </button>
        <router-link to="/login" class="btn btn-primary" v-if="!isLoggedIn()">
          <i class="icon">🔑</i>
          Login
        </router-link>
      </div>
    </div>
  </nav>
</template>

<script setup>
import ShoppingCart from './ShoppingCart.vue';

const isLoggedIn = () => {
  return localStorage.getItem('token') !== null;
}

const getUserRole = () => {
  return localStorage.getItem('userRole');
}

const handleLogout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('userName');
  localStorage.removeItem('userRole');
  localStorage.removeItem('userID');
  localStorage.removeItem('userEmail');

  // reload
  location.reload();
}
</script>

<style scoped>
/* Navigation */
.nav {
  background: white;
  border-bottom: 1px solid #e2e8f0;
  position: fixed;
  width: 100%;
  top: 0;
  z-index: 1000;
  padding: 0 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.nav-container {
  height: 4rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  max-width: 1200px;
  margin: 0 auto;
}

.logo {
  font-weight: 600;
  font-size: 1.25rem;
  color: #2563eb;
  text-decoration: none;
  transition: color 0.2s ease;
}

.logo:hover {
  color: #1d4ed8;
}

.nav-links {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  color: #64748b;
  text-decoration: none;
  border-radius: 0.375rem;
  transition: all 0.2s ease;
  font-weight: 500;
}

.nav-link:hover {
  color: #2563eb;
  background: #f1f5f9;
}

.icon {
  font-size: 1rem;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  text-decoration: none;
  transition: all 0.15s ease;
  border: none;
  cursor: pointer;
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-primary:hover {
  background: #1d4ed8;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}

.btn-secondary {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.btn-secondary:hover {
  border-color: #2563eb;
  color: #2563eb;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

@media (max-width: 768px) {
  .nav-container {
    padding: 0;
  }
  
  .nav-links {
    gap: 0.5rem;
  }
  
  .nav-link span {
    display: none;
  }
  
  .btn {
    padding: 0.5rem;
  }
  
  .btn span {
    display: none;
  }
}
</style>
