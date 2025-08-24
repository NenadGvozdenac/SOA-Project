<template>
  <div>
    <Navbar />
  </div>
  <div>
  <div v-if="loading" class="loading">
    Loading...
  </div>

    <!-- Error -->
    <div v-if="error" class="error">
      {{ error }}
    </div>
    <div class="profiles-container">
      <h1>Users</h1>

      <!-- Tabs -->
      <div class="tabs">
        <button @click="activeTab = 'allUsers'" :class="{ active: activeTab === 'allUsers' }" class="tab-button">
          All Users
        </button>
        <button @click="activeTab = 'suggestions'" :class="{ active: activeTab === 'suggestions' }" class="tab-button">
          Follow Suggestions
        </button>
        <button @click="activeTab = 'following'" :class="{ active: activeTab === 'following' }" class="tab-button">
          Following ({{ followingCount }})
        </button>
        <button @click="activeTab = 'followers'" :class="{ active: activeTab === 'followers' }" class="tab-button">
          Followers
        </button>
      </div> <!-- Loading -->
      <div v-if="loading" class="loading">
        Učitava...
      </div>

      <!-- Error -->
      <div v-if="error" class="error">
        {{ error }}
      </div>

      <!-- Content based on active tab -->
      <div v-if="!loading && !error">
        <!-- All Users Tab -->
        <div v-if="activeTab === 'allUsers'" class="users-grid">
          <div v-if="allUsers.length === 0" class="no-data">
            No users in the system.
          </div>
          <div v-for="user in allUsers" :key="user.id" class="user-card">
            <div class="user-avatar">
              <img v-if="user.profilePicture" :src="`data:image/png;base64,${user.profilePicture}`" :alt="user.name"
                class="avatar-img" />
              <div v-else class="default-avatar">
                {{ user.name.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div class="user-info">
              <h3>{{ user.name }}</h3>
              <p class="username">@{{ user.username }}</p>
              <p class="email">{{ user.email }}</p>
            </div>
            <div class="user-actions">
              <button v-if="!isCurrentUser(user.id) && !isUserFollowed(user.id)" @click="followUser(user.id)"
                :disabled="followingInProgress.has(user.id)" class="follow-btn">
                {{ followingInProgress.has(user.id) ? 'Following...' : 'Follow' }}
              </button>
              <button v-if="!isCurrentUser(user.id) && isUserFollowed(user.id)" @click="unfollowUser(user.id)"
                :disabled="followingInProgress.has(user.id)" class="unfollow-btn">
                {{ followingInProgress.has(user.id) ? 'Unfollowing...' : 'Unfollow' }}
              </button>
              <span v-if="isCurrentUser(user.id)" class="current-user-label">This is you</span>
            </div>
          </div>
        </div>

        <!-- Suggestions Tab -->
        <div v-if="activeTab === 'suggestions'" class="users-grid">
          <div v-if="suggestions.length === 0" class="no-data">
            No follow suggestions.
          </div>
          <div v-for="user in suggestions" :key="user.id" class="user-card">
            <div class="user-avatar">
              <img v-if="user.profilePicture" :src="`data:image/png;base64,${user.profilePicture}`" :alt="user.name"
                class="avatar-img" />
              <div v-else class="default-avatar">
                {{ user.name.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div class="user-info">
              <h3>{{ user.name }}</h3>
              <p class="username">@{{ user.username }}</p>
              <p class="email">{{ user.email }}</p>
            </div>
            <div class="user-actions">
              <button @click="followUser(user.id)" :disabled="followingInProgress.has(user.id)" class="follow-btn">
                {{ followingInProgress.has(user.id) ? 'Following...' : 'Follow' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Following Tab -->
        <div v-if="activeTab === 'following'" class="users-grid">
          <div v-if="following.length === 0" class="no-data">
            You're not following anyone.
          </div>
          <div v-for="user in following" :key="user.id" class="user-card">
            <div class="user-avatar">
              <img v-if="user.profilePicture" :src="`data:image/png;base64,${user.profilePicture}`" :alt="user.name"
                class="avatar-img" />
              <div v-else class="default-avatar">
                {{ user.name.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div class="user-info">
              <h3>{{ user.name }}</h3>
              <p class="username">@{{ user.username }}</p>
              <p class="email">{{ user.email }}</p>
            </div>
            <div class="user-actions">
              <button @click="unfollowUser(user.id)" :disabled="followingInProgress.has(user.id)" class="unfollow-btn">
                {{ followingInProgress.has(user.id) ? 'Unfollowing...' : 'Unfollow' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Followers Tab -->
        <div v-if="activeTab === 'followers'" class="users-grid">
          <div v-if="followers.length === 0" class="no-data">
            No one is following you.
          </div>
          <div v-for="user in followers" :key="user.id" class="user-card">
            <div class="user-avatar">
              <img v-if="user.profilePicture" :src="`data:image/png;base64,${user.profilePicture}`" :alt="user.name"
                class="avatar-img" />
              <div v-else class="default-avatar">
                {{ user.name.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div class="user-info">
              <h3>{{ user.name }}</h3>
              <p class="username">@{{ user.username }}</p>
              <p class="email">{{ user.email }}</p>
            </div>
            <div class="user-actions">
              <span class="follower-label">Follows you</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { FollowingsService } from '../services/followings_service.js';
import Navbar from './Navbar.vue';

export default {
  name: 'Profiles',
  components: {
    Navbar
  },
  data() {
    return {
      activeTab: 'allUsers',
      loading: false,
      error: null,
      allUsers: [],
      suggestions: [],
      following: [],
      followers: [],
      followingInProgress: new Set()
    };
  },
  computed: {
    followingCount() {
      return this.following.length;
    },
    followersCount() {
      return this.followers.length;
    }
  },
  async mounted() {
    // Load following list first to know which users are followed
    await this.loadFollowing();
    await this.loadData();
  },
  watch: {
    async activeTab() {
      await this.loadData();
    }
  },
  methods: {
    async loadData() {
      this.loading = true;
      this.error = null;

      try {
        if (this.activeTab === 'allUsers') {
          await this.loadAllUsers();
        } else if (this.activeTab === 'suggestions') {
          await this.loadSuggestions();
        } else if (this.activeTab === 'following') {
          await this.loadFollowing();
        } else if (this.activeTab === 'followers') {
          await this.loadFollowers();
        }
      } catch (error) {
        this.error = error.message || 'Error loading data';
        console.error('Error loading data:', error);
      } finally {
        this.loading = false;
      }
    },

    async loadAllUsers() {
      const response = await FollowingsService.getAllUsers();
      console.log(response)
      this.allUsers = response.data.filter(user => user.id != 1) || [];
    },

    async loadSuggestions() {
      const response = await FollowingsService.getFollowSuggestions();
      this.suggestions = response.value || response.data || [];
    },

    async loadFollowing() {
      const response = await FollowingsService.getMyFollowings();
      this.following = response.value || response.data || [];
    },

    async loadFollowers() {
      const response = await FollowingsService.getMyFollowers();
      this.followers = response.value || response.data || [];
    },

    async followUser(userId) {
      this.followingInProgress.add(userId);

      try {
        await FollowingsService.followUser(userId);

        // Reload following list to get updated data
        await this.loadFollowing();

        // Remove from suggestions if present
        const userIndex = this.suggestions.findIndex(u => u.id === userId);
        if (userIndex !== -1) {
          this.suggestions.splice(userIndex, 1);
        }

        this.$toast?.success?.('Successfully followed user!');
      } catch (error) {
        console.error('Error following user:', error);
        this.$toast?.error?.('Error following user');
      } finally {
        this.followingInProgress.delete(userId);
      }
    },

    async unfollowUser(userId) {
      this.followingInProgress.add(userId);

      try {
        await FollowingsService.unfollowUser(userId);

        await this.loadFollowing();

        this.$toast?.success?.('Successfully unfollowed user!');
      } catch (error) {
        console.error('Error unfollowing user:', error);
        this.$toast?.error?.('Error unfollowing user');
      } finally {
        this.followingInProgress.delete(userId);
      }
    },

    isCurrentUser(userId) {
      const currentUserId = localStorage.getItem('userID');
      return currentUserId && currentUserId == userId;
    },

    isUserFollowed(userId) {
      return this.following.some(user => user.id == userId);
    },

    getRoleDisplayName(role) {
      const roleMap = {
        'Admin': 'Administrator',
        'Guide': 'Guide/Author',
        'Tourist': 'Tourist'
      };
      return roleMap[role] || role;
    }
  }
};
</script>

<style scoped>
.profiles-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 80px 20px 20px 20px;
}

h1 {
  text-align: center;
  color: #333;
  margin-bottom: 30px;
}

.tabs {
  display: flex;
  justify-content: center;
  margin-bottom: 30px;
  border-bottom: 2px solid #e0e0e0;
}

.tab-button {
  padding: 10px 20px;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 16px;
  color: #666;
  border-bottom: 3px solid transparent;
  transition: all 0.3s ease;
}

.tab-button:hover {
  color: #333;
}

.tab-button.active {
  color: #007bff;
  border-bottom-color: #007bff;
  font-weight: 600;
}

.loading {
  text-align: center;
  padding: 40px;
  font-size: 18px;
  color: #666;
}

.error {
  text-align: center;
  padding: 20px;
  color: #dc3545;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 5px;
  margin-bottom: 20px;
}

.users-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-top: 24px;
  width: 100%;
}

.no-data {
  grid-column: 1 / -1;
  text-align: center;
  color: #666;
  font-style: italic;
  padding: 40px;
  background: #f8f9fa;
  border-radius: 8px;
}

.user-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 20px;
  border: 1px solid #f0f0f0;
  min-height: 120px;
}

.user-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.12);
  border-color: #e0e7ff;
}

.user-avatar {
  flex-shrink: 0;
}

.avatar-img {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #e0e7ff;
}

.default-avatar {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 28px;
  font-weight: bold;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-info h3 {
  margin: 0 0 8px 0;
  color: #1f2937;
  font-size: 20px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.username {
  margin: 0 0 4px 0;
  color: #6b7280;
  font-size: 14px;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.email {
  margin: 0 0 5px 0;
  color: #9ca3af;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.role {
  margin: 0;
  color: #666;
  font-size: 12px;
  font-weight: 500;
  background: #f8f9fa;
  padding: 2px 6px;
  border-radius: 3px;
  display: inline-block;
}

.user-actions {
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: flex-end;
  padding: 4px;
}

.follow-btn {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
  min-width: 120px;
}

.follow-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 3px 8px rgba(59, 130, 246, 0.4);
}

.follow-btn:disabled {
  background: #9ca3af;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.unfollow-btn {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
  min-width: 120px;
}

.unfollow-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 3px 8px rgba(239, 68, 68, 0.4);
  background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
}

.unfollow-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.follower-label {
  color: #28a745;
  font-size: 14px;
  font-weight: 500;
  padding: 8px 16px;
  background: #d4edda;
  border-radius: 6px;
}

.current-user-label {
  color: #6f42c1;
  font-size: 14px;
  font-weight: 500;
  padding: 8px 16px;
  background: #e2d9f3;
  border-radius: 6px;
}

@media (max-width: 768px) {
  .users-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }
}

@media (max-width: 480px) {
  .users-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }

  .user-card {
    flex-direction: column;
    text-align: center;
    min-height: auto;
    padding: 20px;
  }

  .user-info {
    text-align: center;
  }

  .avatar-img,
  .default-avatar {
    width: 60px;
    height: 60px;
  }

  .default-avatar {
    font-size: 24px;
  }
}
</style>
