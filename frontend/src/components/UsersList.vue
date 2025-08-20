<template>
    <div class="users-container">
        <!-- Navigation -->

        <!-- Navigation -->
        <nav class="nav">
            <div class="nav-container">
                <router-link to="/" class="logo">TourismHub</router-link>
                <div class="nav-links">
                    <button class="btn btn-secondary" v-if="isLoggedIn()" @click="handleLogout">Logout</button>
                </div>
            </div>
        </nav>

        <!-- Users List Section -->
        <section class="users-section">
            <div class="container">
                <div class="page-header">
                    <h1>👥 All Users Management</h1>
                    <p>Manage and view all registered users in the system</p>
                </div>

                <!-- Loading State -->
                <div v-if="isLoading" class="loading-state">
                    <div class="loading-spinner"></div>
                    <p>Loading users...</p>
                </div>

                <!-- Error State -->
                <div v-if="error" class="error-state">
                    <div class="error-icon">⚠️</div>
                    <h3>Error Loading Users</h3>
                    <p>{{ error }}</p>
                    <button @click="fetchUsers" class="btn btn-primary">Try Again</button>
                </div>

                <!-- Users Table -->
                <div v-if="!isLoading && !error" class="users-table-container">
                    <div class="table-header">
                        <h2>Users ({{ users.length }})</h2>
                        <div class="table-actions">
                            <input v-model="searchQuery" type="text" placeholder="Search users..." class="search-input">
                            <button @click="fetchUsers" class="btn btn-primary">
                                🔄 Refresh
                            </button>
                        </div>
                    </div>

                    <div class="table-wrapper">
                        <table class="users-table">
                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Name</th>
                                    <th>Username</th>
                                    <th>Email</th>
                                    <th>Role</th>
                                    <th>Status</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="user in filteredUsers" :key="user.id" class="user-row">
                                    <td class="user-id">{{ user.ID }}</td>
                                    <td class="user-name">
                                        <div class="user-info">
                                            <div class="user-avatar">{{ getInitials(user.name, user.surname) }}</div>
                                            <div>
                                                <div class="name">{{ user.name }} {{ user.surname }}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td class="username">{{ user.username }}</td>
                                    <td class="email">{{ user.email }}</td>
                                    <td class="role">
                                        <span :class="getRoleClass(user.role_id)">
                                            {{ getRoleName(user.role_id) }}
                                        </span>
                                    </td>
                                    <td class="status">
                                        <span class="status-active">Active</span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- Empty State -->
                    <div v-if="filteredUsers.length === 0" class="empty-state">
                        <div class="empty-icon">👤</div>
                        <h3>No Users Found</h3>
                        <p>{{ searchQuery ? 'No users match your search criteria.' : 'No users registered yet.' }}</p>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { STAKEHOLDERS_URL } from '../services/const_service.js'

const router = useRouter()
const users = ref([])
const isLoading = ref(false)
const error = ref('')
const searchQuery = ref('')

const filteredUsers = computed(() => {
    if (!searchQuery.value) return users.value

    const query = searchQuery.value.toLowerCase()
    return users.value.filter(user =>
        user.name.toLowerCase().includes(query) ||
        user.surname.toLowerCase().includes(query) ||
        user.username.toLowerCase().includes(query) ||
        user.email.toLowerCase().includes(query)
    )
})

const fetchUsers = async () => {
    isLoading.value = true
    error.value = ''

    try {
        const token = localStorage.getItem('token')
        const response = await axios.get(`${STAKEHOLDERS_URL}/users`, {
            headers: {
                Authorization: `Bearer ${token}`
            }
        })

        users.value = response.data.data || []
    } catch (err) {
        error.value = err.response?.data?.message || 'Failed to fetch users'
        console.error('Error fetching users:', err)
    } finally {
        isLoading.value = false
    }
}

const getInitials = (name, surname) => {
    return `${name?.charAt(0) || ''}${surname?.charAt(0) || ''}`.toUpperCase()
}

const getRoleName = (roleId) => {
    const roles = {
        0: 'Admin',
        1: 'User',
        2: 'Moderator'
    }
    return roles[roleId] || 'Unknown'
}

const getRoleClass = (roleId) => {
    const classes = {
        0: 'role-admin',
        1: 'role-user',
        2: 'role-moderator'
    }
    return `role-badge ${classes[roleId] || 'role-unknown'}`
}

const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('userName')
    localStorage.removeItem('userRole')
    localStorage.removeItem('userID')
    localStorage.removeItem('userEmail')

    router.push('/')
}

const isLoggedIn = () => {
  return localStorage.getItem('token') !== null;
}

onMounted(() => {
    // Check if user is admin
    const userRole = localStorage.getItem('userRole')
    if (userRole !== 'Admin') {
        router.push('/')
        return
    }

    fetchUsers()
})
</script>

<style scoped>
.users-container {
  min-height: 100vh;
  background: #f8f9fa;
  padding-top: 4rem;
}

.users-section {
    padding: 2rem 0;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 2rem;
}

.page-header {
    text-align: center;
    margin-bottom: 3rem;
}

.page-header h1 {
    font-size: 2.5rem;
    color: #2c3e50;
    margin-bottom: 0.5rem;
}

.page-header p {
    font-size: 1.1rem;
    color: #6c757d;
}

.loading-state,
.error-state,
.empty-state {
    text-align: center;
    padding: 3rem;
    background: white;
    border-radius: 12px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.loading-spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #007bff;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 1rem;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}

.error-icon,
.empty-icon {
    font-size: 3rem;
    margin-bottom: 1rem;
}

.users-table-container {
    background: white;
    border-radius: 12px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.table-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem 2rem;
    border-bottom: 1px solid #e9ecef;
}

.table-header h2 {
    margin: 0;
    color: #2c3e50;
}

.table-actions {
    display: flex;
    gap: 1rem;
    align-items: center;
}

.search-input {
    padding: 0.5rem 1rem;
    border: 1px solid #ced4da;
    border-radius: 6px;
    font-size: 0.875rem;
    width: 250px;
}

.table-wrapper {
    overflow-x: auto;
}

.users-table {
    width: 100%;
    border-collapse: collapse;
}

.users-table th {
    background: #f8f9fa;
    padding: 1rem;
    text-align: left;
    font-weight: 600;
    color: #495057;
    border-bottom: 1px solid #e9ecef;
}

.users-table td {
    padding: 1rem;
    border-bottom: 1px solid #f1f3f4;
}

.user-row:hover {
    background: #f8f9fa;
}

.user-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.user-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-weight: 600;
    font-size: 0.875rem;
}

.name {
    font-weight: 500;
    color: #2c3e50;
}

.role-badge {
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.75rem;
    font-weight: 500;
    text-transform: uppercase;
}

.role-admin {
    background: #dc3545;
    color: white;
}

.role-user {
    background: #28a745;
    color: white;
}

.role-moderator {
    background: #ffc107;
    color: #212529;
}

.status-active {
    color: #28a745;
    font-weight: 500;
}
</style>
