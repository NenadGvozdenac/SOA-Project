<template>
  <div>
    <Navbar />
  </div>
  <div class="followed-blogs-container">
    <h1>Blogs from Followed Users</h1>
    <p class="subtitle">Here you can see blogs only from users you follow</p>

    <!-- Loading -->
    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      Loading blogs...
    </div>

    <!-- Error -->
    <div v-if="error" class="error">
      <i class="error-icon">⚠️</i>
      {{ error }}
      <button @click="loadBlogs" class="retry-btn">Try again</button>
    </div>

    <!-- No blogs -->
    <div v-if="!loading && !error && blogs.length === 0" class="no-blogs">
      <div class="no-blogs-icon">📝</div>
      <h3>No blogs</h3>
      <p>
        Users you follow haven't published blogs yet or you're not following anyone.
      </p>
      <router-link to="/profiles" class="find-users-btn">
        Find users to follow
      </router-link>
    </div>

    <!-- Blogs list -->
    <div v-if="!loading && !error && blogs.length > 0" class="blogs-list">
      <div v-for="blog in blogs" :key="blog.id" class="blog-card">
        <div class="blog-header">
          <div class="author-info">
            <div class="author-avatar">
              <img v-if="blog.authorProfilePicture" :src="`data:image/png;base64,${blog.authorProfilePicture}`"
                :alt="blog.authorName" class="avatar-img" />
              <div v-else class="default-avatar">
                {{ blog.authorName.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div class="author-details">
              <h4 class="author-name">{{ blog.authorName }}</h4>
              <p class="author-username">@{{ blog.authorUsername }}</p>
            </div>
          </div>
          <div class="blog-meta">
            <span class="blog-date">{{ formatDate(blog.creationTime) }}</span>
          </div>
        </div>

        <div class="blog-content">
          <h2 class="blog-title">{{ blog.title }}</h2>
          <div class="blog-description" v-html="formatMarkdown(blog.description)"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { FollowingsService } from '../services/followings_service.js';
import Navbar from './Navbar.vue';

export default {
  name: 'FollowedBlogs',
  components: {
    Navbar
  },
  data() {
    return {
      blogs: [],
      loading: false,
      error: null
    };
  },
  async mounted() {
    await this.loadBlogs();
  },
  methods: {
    async loadBlogs() {
      this.loading = true;
      this.error = null;

      try {
        const response = await FollowingsService.getBlogsFromFollowedUsers();
        console.log('Response from getBlogsFromFollowedUsers:', response);
        console.log('Response data:', response.data);

        // Backend vraća Result<List<BlogFromFollowedUserDTO>> koji ima Value property
        let blogs = [];
        if (response.data && response.data.value) {
          blogs = response.data.value;
        } else if (response.data && response.data.data) {
          blogs = response.data.data;
        } else if (response.data && Array.isArray(response.data)) {
          blogs = response.data;
        }

        console.log('Parsed blogs:', blogs);
        this.blogs = blogs || [];
      } catch (error) {
        this.error = error.message || 'Error loading blogs';
        console.error('Error loading blogs:', error);
      } finally {
        this.loading = false;
      }
    },

    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleDateString('sr-RS', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    formatMarkdown(text) {
      if (!text) return '';

      // Simple markdown to HTML conversion
      return text
        .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.*?)\*/g, '<em>$1</em>')
        .replace(/\n/g, '<br>')
        .substring(0, 300) + (text.length > 300 ? '...' : '');
    }
  }
};
</script>

<style scoped>
.followed-blogs-container {
  max-width: 800px;
  margin: 80px auto;
  padding: 20px;
}

h1 {
  text-align: center;
  color: #333;
  margin-bottom: 10px;
  font-size: 28px;
}

.subtitle {
  text-align: center;
  color: #666;
  margin-bottom: 30px;
  font-size: 16px;
}

.loading {
  text-align: center;
  padding: 60px 20px;
  color: #666;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.error {
  text-align: center;
  padding: 30px;
  color: #dc3545;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 10px;
  margin-bottom: 20px;
}

.error-icon {
  font-size: 24px;
  margin-bottom: 10px;
}

.retry-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  margin-top: 15px;
  transition: background-color 0.2s ease;
}

.retry-btn:hover {
  background: #0056b3;
}

.no-blogs {
  text-align: center;
  padding: 60px 20px;
  color: #666;
}

.no-blogs-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.no-blogs h3 {
  color: #333;
  margin-bottom: 15px;
}

.find-users-btn {
  display: inline-block;
  background: #007bff;
  color: white;
  text-decoration: none;
  padding: 12px 24px;
  border-radius: 8px;
  margin-top: 20px;
  transition: background-color 0.2s ease;
}

.find-users-btn:hover {
  background: #0056b3;
}

.blogs-list {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.blog-card {
  background: white;
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid #e0e0e0;
}

.blog-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
}

.blog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #f0f0f0;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.author-avatar {
  flex-shrink: 0;
}

.avatar-img {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e0e0e0;
}

.default-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  font-weight: bold;
}

.author-details {
  flex: 1;
}

.author-name {
  margin: 0 0 5px 0;
  color: #333;
  font-size: 16px;
  font-weight: 600;
}

.author-username {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.blog-meta {
  text-align: right;
}

.blog-date {
  display: block;
  color: #888;
  font-size: 13px;
  margin-bottom: 5px;
}

.blog-content {
  margin-bottom: 20px;
}

.blog-title {
  color: #333;
  margin: 0 0 15px 0;
  font-size: 24px;
  font-weight: 700;
  line-height: 1.3;
}

.blog-description {
  color: #555;
  line-height: 1.6;
  font-size: 16px;
}

.blog-description strong {
  color: #333;
}

.blog-description em {
  color: #666;
}

@media (max-width: 768px) {
  .followed-blogs-container {
    padding: 15px;
  }

  .blog-card {
    padding: 20px;
  }

  .blog-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }

  .blog-meta {
    text-align: left;
    width: 100%;
  }

  .blog-title {
    font-size: 20px;
  }
}
</style>
