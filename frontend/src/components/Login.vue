<template>
  <div>
    <Navbar />
    <div class="auth-container">
      <div class="auth-card">
        <h2>Welcome Back</h2>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="email">Email</label>
          <input 
            type="email" 
            id="email"
            v-model="form.email"
            placeholder="Enter your email"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            type="password" 
            id="password"
            v-model="form.password"
            placeholder="Enter your password"
            required
          />
        </div>
        
        <button type="submit" class="btn btn-primary" :disabled="isLoading">
          {{ isLoading ? 'Signing In...' : 'Sign In' }}
        </button>
        
        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>
      
      <div class="auth-links">
        <p>Don't have an account? 
          <router-link to="/register">Create one here</router-link>
        </p>
        <p>
          <router-link to="/">← Back to Home</router-link>
        </p>
      </div>
      </div>
    </div>
  </div>
</template><script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService } from '../services/auth_service.js'
import Navbar from './Navbar.vue'

const router = useRouter()
const isLoading = ref(false)
const error = ref('')

const form = reactive({
  email: '',
  password: ''
})

const handleLogin = async () => {
  error.value = ''
  
  try {
    isLoading.value = true
    
    const response = await AuthService.login(form.email, form.password)

    const token = response.data.token;

    // Store auth token/user data if needed
    localStorage.setItem('token', token)

    const { userName, userRole, userID, userEmail } = await AuthService.decode(token)

    localStorage.setItem('userName', userName)
    localStorage.setItem('userRole', userRole)
    localStorage.setItem('userID', userID)
    localStorage.setItem('userEmail', userEmail)

    // Login successful, redirect to home
    router.push('/')
  } catch (err) {
    if (err.response?.status === 403) {
      error.value = err.response.data.message || 'Your account has been blocked. Please contact administrator.'
    } else {
      error.value = err.message || 'Login failed. Please check your credentials.'
    }
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
}

.auth-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem 2.5rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 16px rgba(0,0,0,0.08);
  min-width: 350px;
}

.auth-card h2 {
  text-align: center;
  margin-bottom: 1.5rem;
}

form {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.btn {
  margin: 1rem auto 0 auto;
  display: block;
}

  .form-group {
    width: 100%;
  }

  input[type="email"],
  input[type="password"] {
    width: 100%;
    box-sizing: border-box;
    padding: 0.5rem;
    margin-top: 0.25rem;
    margin-bottom: 0.75rem;
    border: 1px solid #ccc;
    border-radius: 6px;
    font-size: 1rem;
  }
</style>
