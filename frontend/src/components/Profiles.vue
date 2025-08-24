<template>
  <div>
    <Navbar />
    <div class="profiles-container">
      <h1>Korisnici</h1>

      <!-- Tabs -->
      <div class="tabs">
        <button @click="activeTab = 'allUsers'" :class="{ active: activeTab === 'allUsers' }" class="tab-button">
          Svi korisnici
        </button>
        <button @click="activeTab = 'suggestions'" :class="{ active: activeTab === 'suggestions' }" class="tab-button">
          Predlozi za praćenje
        </button>
        <button @click="activeTab = 'following'" :class="{ active: activeTab === 'following' }" class="tab-button">
          Pratim ({{ followingCount }})
        </button>
        <button @click="activeTab = 'followers'" :class="{ active: activeTab === 'followers' }" class="tab-button">
          Prate me
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
      <div v-if="!loading && !error" class="users-grid">
        <!-- All Users Tab -->
        <div v-if="activeTab === 'allUsers'">
          <div v-if="allUsers.length === 0" class="no-data">
            Nema korisnika u sistemu.
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
                {{ followingInProgress.has(user.id) ? 'Pratim...' : 'Zaprati' }}
              </button>
              <button v-if="!isCurrentUser(user.id) && isUserFollowed(user.id)" @click="unfollowUser(user.id)"
                :disabled="followingInProgress.has(user.id)" class="unfollow-btn">
                {{ followingInProgress.has(user.id) ? 'Prekidam...' : 'Prekini praćenje' }}
              </button>
              <span v-if="isCurrentUser(user.id)" class="current-user-label">Vi ste</span>
            </div>
          </div>
        </div>

        <!-- Suggestions Tab -->
        <div v-if="activeTab === 'suggestions'">
          <div v-if="suggestions.length === 0" class="no-data">
            Nema predloga za praćenje.
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
                {{ followingInProgress.has(user.id) ? 'Pratim...' : 'Zaprati' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Following Tab -->
        <div v-if="activeTab === 'following'">
          <div v-if="following.length === 0" class="no-data">
            Ne pratite nikog.
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
                {{ followingInProgress.has(user.id) ? 'Prekidam...' : 'Prekini praćenje' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Followers Tab -->
        <div v-if="activeTab === 'followers'">
          <div v-if="followers.length === 0" class="no-data">
            Niko vas ne prati.
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
              <span class="follower-label">Prati vas</span>
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
        this.error = error.message || 'Greška pri učitavanju podataka';
        console.error('Error loading data:', error);
      } finally {
        this.loading = false;
      }
    },

    async loadAllUsers() {
      const response = await FollowingsService.getAllUsers();
      this.allUsers = response.data || [];
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

        this.$toast?.success?.('Uspešno ste zapratili korisnika!');
      } catch (error) {
        console.error('Error following user:', error);
        this.$toast?.error?.('Greška pri praćenju korisnika');
      } finally {
        this.followingInProgress.delete(userId);
      }
    },

    async unfollowUser(userId) {
      this.followingInProgress.add(userId);

      try {
        await FollowingsService.unfollowUser(userId);

        await this.loadFollowing();

        this.$toast?.success?.('Uspešno ste prekinuli praćenje!');
      } catch (error) {
        console.error('Error unfollowing user:', error);
        this.$toast?.error?.('Greška pri prekidanju praćenja');
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
        'Guide': 'Vodič/Autor',
        'Tourist': 'Turista'
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

.no-data {
  text-align: center;
  padding: 40px;
  color: #666;
  font-style: italic;
}

.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.user-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  display: flex;
  align-items: center;
  gap: 15px;
}

.user-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}

.user-avatar {
  flex-shrink: 0;
}

.avatar-img {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #e0e0e0;
}

.default-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 24px;
  font-weight: bold;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-info h3 {
  margin: 0 0 5px 0;
  color: #333;
  font-size: 18px;
}

.username {
  margin: 0 0 5px 0;
  color: #666;
  font-size: 14px;
}

.email {
  margin: 0 0 5px 0;
  color: #888;
  font-size: 13px;
  word-break: break-word;
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
}

.follow-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s ease;
}

.follow-btn:hover:not(:disabled) {
  background: #0056b3;
}

.follow-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.unfollow-btn {
  background: #dc3545;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s ease;
}

.unfollow-btn:hover:not(:disabled) {
  background: #c82333;
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
  }

  .user-card {
    flex-direction: column;
    text-align: center;
  }

  .user-info {
    text-align: center;
  }
}
</style>
