<template>
  <div>
    <!-- Navigation -->
    <nav class="nav">
      <div class="nav-container">
        <router-link to="/" class="logo">TourismHub</router-link>
        <div class="nav-links">
          <router-link to="/login" class="btn btn-primary" v-if="!isLoggedIn()">Login</router-link>
          <router-link to="/register" class="btn btn-secondary" v-if="!isLoggedIn()">Register</router-link>
          
          <!-- Admin Dropdown -->
          <div v-if="isAdmin()" class="dropdown">
            <button class="btn btn-admin dropdown-toggle">
              <i class="admin-icon">⚙️</i>
              Admin Panel
              <i class="dropdown-arrow">▼</i>
            </button>
            <div class="dropdown-menu">
              <div class="dropdown-header">Administration</div>
              <router-link to="/users" class="dropdown-item">
                <i class="item-icon">👥</i>
                All Users
              </router-link>
            </div>
          </div>
          
          <button class="btn btn-secondary" v-if="isLoggedIn()" @click="handleLogout">Logout</button>
        </div>
      </div>
    </nav>

    <!-- Hero Section -->
    <section class="hero">
      <div class="container">
        <h1>Welcome to TourismHub</h1>
        <p>Your ultimate microservices platform for tourism, blogs, stakeholders, and followers management</p>
        <div class="hero-buttons">
          <router-link to="/register" class="btn btn-primary">Get Started</router-link>
          <router-link to="/login" class="btn btn-secondary">Sign In</router-link>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features">
      <div class="container">
        <div class="feature-card">
          <h3>🏢 Stakeholder Management</h3>
          <p>Comprehensive stakeholder management system with advanced features for tracking and managing business
            relationships, partnerships, and key contacts.</p>
        </div>

        <div class="feature-card">
          <h3>👥 Followers Network</h3>
          <p>Build and manage your follower network with sophisticated tools for engagement, analytics, and community
            building across multiple platforms.</p>
        </div>

        <div class="feature-card">
          <h3>📝 Blog Platform</h3>
          <p>Create, publish, and manage engaging blog content with our powerful content management system designed for
            tourism and travel professionals.</p>
        </div>

        <div class="feature-card">
          <h3>🌍 Tourism Services</h3>
          <p>Complete tourism management solution with booking systems, itinerary planning, destination management, and
            customer experience optimization.</p>
        </div>

        <div class="feature-card">
          <h3>📊 Analytics Dashboard</h3>
          <p>Real-time analytics and insights to track performance, engagement metrics, and business intelligence across
            all your tourism operations.</p>
        </div>

        <div class="feature-card">
          <h3>🔒 Enterprise Security</h3>
          <p>Bank-level security with end-to-end encryption, multi-factor authentication, and compliance with
            international data protection standards.</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>

const isLoggedIn = () => {
  return localStorage.getItem('token') !== null;
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

const isAdmin = () => {
  return localStorage.getItem('userRole') == 'Admin';
}

</script>

<style scoped>
.dropdown {
  position: relative;
  display: inline-block;
}

.dropdown-toggle {
  cursor: pointer;
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.btn-admin {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 8px;
  font-weight: 600;
  box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);
}

.btn-admin:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.admin-icon {
  font-size: 1.1rem;
}

.dropdown-arrow {
  font-size: 0.7rem;
  transition: transform 0.3s ease;
}

.dropdown:hover .dropdown-arrow {
  transform: rotate(180deg);
}

.dropdown-menu {
  display: none;
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border: none;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  min-width: 220px;
  z-index: 1000;
  overflow: hidden;
  animation: slideDown 0.2s ease;
}

.dropdown:hover .dropdown-menu,
.dropdown-menu:hover {
  display: block;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.dropdown-header {
  padding: 0.8rem 1rem;
  font-size: 0.8rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.8rem 1rem;
  color: #374151;
  text-decoration: none;
  border-bottom: 1px solid #f3f4f6;
  transition: all 0.2s ease;
  font-weight: 500;
}

.dropdown-item:hover {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  transform: translateX(4px);
}

.dropdown-item:last-child {
  border-bottom: none;
}

.item-icon {
  font-size: 1rem;
  opacity: 0.8;
}

.dropdown-item:hover .item-icon {
  opacity: 1;
}
</style>
