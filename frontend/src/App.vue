

<template>
  <router-view />
  <ShoppingCart v-if="isTouristLoggedIn" />
</template>

<script setup>
import { ref, onMounted } from 'vue';
import ShoppingCart from './components/ShoppingCart.vue';
import { AuthService } from './services/auth_service.js';

const isTouristLoggedIn = ref(false);

onMounted(() => {
  const token = localStorage.getItem('token');
  const decoded = AuthService.decode(token);
  isTouristLoggedIn.value = decoded?.userRole === 'Tourist';
});
</script>
