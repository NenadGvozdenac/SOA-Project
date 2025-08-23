<template>
  <div class="auth-container">
    <div class="auth-card">
      <h2>Join us</h2>
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label for="name">First Name</label>
          <input 
            type="text" 
            id="name"
            v-model="form.name"
            placeholder="Enter your first name"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="surname">Last Name</label>
          <input 
            type="text" 
            id="surname"
            v-model="form.surname"
            placeholder="Enter your last name"
            required
          />
        </div>
        
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
          <label for="username">Username</label>
          <input 
            type="text" 
            id="username"
            v-model="form.username"
            placeholder="Choose a username"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="roleId">Role</label>
          <select 
            id="roleId"
            v-model="form.role_id"
            required
          >
            <option value="">Select your role</option>
            <option value="1">Guide</option>
            <option value="2">Tourist</option>
          </select>
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            type="password" 
            id="password"
            v-model="form.password"
            placeholder="Create a strong password"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="confirmPassword">Confirm Password</label>
          <input 
            type="password" 
            id="confirmPassword"
            v-model="form.confirm_password"
            placeholder="Confirm your password"
            required
          />
        </div>
        
        <button type="submit" class="btn btn-primary" style="width: 100%;" :disabled="isLoading">
          {{ isLoading ? 'Creating Account...' : 'Create Account' }}
        </button>
        
        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>
      
      <div class="auth-links">
        <p>Already have an account? 
          <router-link to="/login">Sign in here</router-link>
        </p>
        <p>
          <router-link to="/">← Back to Home</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService } from '../services/auth_service.js'
import axios from 'axios'

const router = useRouter()
const isLoading = ref(false)
const error = ref('')

const form = reactive({
  name: '',
  surname: '',
  email: '',
  username: '',
  role_id: '',
  password: '',
  confirm_password: ''
})

const handleRegister = async () => {
  error.value = ''
  
  // Password confirmation validation
  if (form.password !== form.confirm_password) {
    error.value = 'Passwords do not match!'
    return
  }
  
  // Role validation
  if (!form.role_id) {
    error.value = 'Please select a role!'
    return
  }
  
  try {
    isLoading.value = true
    
    console.log('Zapoceta registracija')
    const registerResponse = await AuthService.register(
      form.name,
      form.surname,
      form.email,
      form.password,
      form.confirm_password,
      form.username,
      parseInt(form.role_id)
    )
    console.log('Registracija uspesna:', registerResponse)
    console.log('Zavrsena registracija')
    try {
      // Iskoristi AuthService.decode za JWT token
      let userId = null;
      if (registerResponse?.data) {
        const decoded = AuthService.decode(registerResponse.data.token);
        userId = decoded?.userID;
        console.log('UserId iz tokena:', userId);
      }
      if (userId) {
        const cartRes = await axios.post('http://localhost:8082/api/tours/shoppingcart/' + userId, {}, {
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${registerResponse.data.token}`
          }
        })
        console.log('Shopping cart kreiran:', cartRes)
      } else {
        console.error('Nije pronađen userId u tokenu, ne mogu da kreiram korpu!')
      }
    } catch (e) {
      console.error('Greška pri kreiranju shopping cart-a:', e)
    }

// Registration successful, redirect to login
await router.push('/login')
  } catch (err) {
    error.value = err.message || 'Registration failed. Please try again.'
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
  input[type="password"],
  input[type="text"],
  select {
    width: 100%;
    box-sizing: border-box;
    padding: 0.5rem;
    margin-top: 0.25rem;
    border: 1px solid #ccc;
    border-radius: 6px;
    font-size: 1rem;
  }
</style>
