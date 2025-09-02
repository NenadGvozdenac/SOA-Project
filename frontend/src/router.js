import { createRouter, createWebHistory } from 'vue-router'
import Home from './components/Home.vue'
import Login from './components/Login.vue'
import Register from './components/Register.vue'
import UsersList from './components/UsersList.vue'
import AuthorMapCheckpoint from './components/AuthorMapCheckpoint.vue'
import PositionSimulator from './components/PositionSimulator.vue'
import ActiveTour from './components/ActiveTour.vue'
import Tours from './components/Tours.vue'
import ToursForTourist from './components/ToursForTourist.vue'
import ShoppingCart from './components/ShoppingCart.vue'
import TourReviews from './components/TourReviews.vue'
import Profiles from './components/Profiles.vue'
import FollowedBlogs from './components/FollowedBlogs.vue'
import Profile from './components/Profile.vue'

// Helper function to check if user is authenticated
function isAuthenticated() {
  const token = localStorage.getItem('token');
  if (!token) return false;
  
  try {
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    const currentTime = Date.now() / 1000;
    return decoded.exp > currentTime;
  } catch (error) {
    return false;
  }
}

const routes = [
  { path: '/', component: Home },
  { path: '/login', component: Login },
  { path: '/register', component: Register },
  { path: '/users', component: UsersList },
  { path: '/profiles', component: Profiles },
  { path: '/profile', component: Profile, meta: { requiresAuth: true } },
  { path: '/followed-blogs', component: FollowedBlogs },
  { path: '/map-checkpoint', component: AuthorMapCheckpoint },
  { path: '/position-simulator', component: PositionSimulator, meta: { requiresAuth: true } },
  { path: '/active-tour', component: ActiveTour, meta: { requiresAuth: true } },
  { path: '/tours', component: Tours },
  { path: '/tours-for-tourist', component: ToursForTourist },
  { path: '/shopping-cart', component: ShoppingCart },
  { path: '/tour-reviews', component: TourReviews },
  { path: '/purchased-tours', component: () => import('./components/PurchasedTours.vue') },
  { 
    path: '/blogs', 
    component: () => import('./components/Blogs.vue'),
    meta: { requiresAuth: true }
  },
  { 
    path: '/blog/:id', 
    component: () => import('./components/BlogDetail.vue'),
    meta: { requiresAuth: true }
  },
  // Catch-all route for 404 pages
  { 
    path: '/:pathMatch(.*)*', 
    redirect: '/' 
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard to check authentication
router.beforeEach((to, from, next) => {
  if (to.meta.requiresAuth && !isAuthenticated()) {
    next('/login');
  } else {
    next();
  }
});

export default router
