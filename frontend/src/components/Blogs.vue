<template>
  <div>
    <!-- Navigation -->
    <Navbar />

    <div class="blogs-container">
      <div class="header">
        <h1>📝 Travel Blogs</h1>
        <p class="subtitle">Share your travel experiences and read stories from fellow travelers</p>
      </div>

      <!-- Tab Navigation -->
      <div class="tab-navigation">
        <button 
          @click="activeTab = 'view'" 
          :class="['tab-btn', { active: activeTab === 'view' }]"
        >
          📖 Browse Blogs
        </button>
        <button 
          @click="activeTab = 'create'" 
          :class="['tab-btn', { active: activeTab === 'create' }]"
          v-if="isAuthenticated"
        >
          ✍️ Create Blog
        </button>
      </div>

    <!-- Authentication Required Message -->
    <div v-if="!isAuthenticated && activeTab === 'create'" class="auth-required">
      <div class="auth-icon">🔒</div>
      <h2>Login Required</h2>
      <p>To create a blog, you must be logged in to the platform.</p>
      <div class="auth-actions">
        <router-link to="/login" class="btn btn-primary">
          <i class="icon">🔑</i>
          Sign In
        </router-link>
        <router-link to="/register" class="btn btn-secondary">
          <i class="icon">👤</i>
          Register
        </router-link>
      </div>
    </div>

    <!-- Blog List View -->
    <div v-if="activeTab === 'view'" class="blogs-list-section">
      <div class="blogs-header">
        <h2>📚 Latest Blog Posts</h2>
        <div class="pagination-info" v-if="blogs.length > 0">
          Showing {{ blogs.length }} blogs
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loadingBlogs" class="loading-state">
        <div class="loading-spinner-large"></div>
        <p>Loading blogs...</p>
      </div>

      <!-- Error -->
      <div v-if="blogsError" class="error-state">
        <div class="error-icon">⚠️</div>
        <h3>Error Loading Blogs</h3>
        <p>{{ blogsError }}</p>
        <button @click="loadBlogs" class="btn btn-primary">
          🔄 Try Again
        </button>
      </div>

      <!-- Blogs Grid -->
      <div v-if="!loadingBlogs && !blogsError" class="blogs-grid">
        <div v-if="blogs.length === 0" class="no-blogs">
          <div class="no-blogs-icon">📝</div>
          <h3>No Blogs Yet</h3>
          <p>Be the first to share your travel experiences!</p>
          <button 
            v-if="isAuthenticated" 
            @click="activeTab = 'create'" 
            class="btn btn-primary"
          >
            ✍️ Create First Blog
          </button>
        </div>

        <article 
          v-for="blog in blogs" 
          :key="blog.id" 
          class="blog-card"
          @click="viewBlog(blog.id)"
        >
          <div v-if="blog.imageBase64" class="blog-image">
            <img :src="`data:image/jpeg;base64,${blog.imageBase64}`" :alt="blog.title" />
          </div>
          <div class="blog-content">
            <h3 class="blog-title">{{ blog.title }}</h3>
            <div class="blog-meta">
              <span class="blog-author">👤 Author ID: {{ blog.userId }}</span>
              <span class="blog-date">📅 {{ formatDate(blog.createdAt) }}</span>
            </div>
            <div class="blog-description">
              {{ getPlainTextPreview(blog.descriptionMarkdown) }}
            </div>
            <div class="blog-actions">
              <button @click.stop="viewBlog(blog.id)" class="btn btn-outline">
                📖 Read More
              </button>
              <div class="blog-actions-right">
                <div class="likes-info">
                  <span class="likes-count">{{ blog.likesCount || 0 }} likes</span>
                </div>
                <button 
                  @click.stop="toggleLike(blog)" 
                  :class="['btn', blog.isLikedByCurrentUser ? 'btn-liked' : 'btn-like']"
                  :disabled="likingBlogs.has(blog.id)"
                >
                  {{ blog.isLikedByCurrentUser ? '💖' : '❤️' }} 
                  {{ blog.isLikedByCurrentUser ? 'Unlike' : 'Like' }}
                </button>
              </div>
            </div>
          </div>
        </article>
      </div>

      <!-- Pagination -->
      <div v-if="blogs.length > 0" class="pagination">
        <button 
          @click="loadBlogs(currentPage - 1)" 
          :disabled="currentPage <= 1 || loadingBlogs"
          class="btn btn-secondary"
        >
          ← Previous
        </button>
        <span class="page-info">Page {{ currentPage }}</span>
        <button 
          @click="loadBlogs(currentPage + 1)" 
          :disabled="blogs.length < pageSize || loadingBlogs"
          class="btn btn-secondary"
        >
          Next →
        </button>
      </div>
    </div>

    <!-- Blog Creation Form - Only show if authenticated and create tab is active -->
    <template v-if="isAuthenticated && activeTab === 'create'">
      <div class="create-blog-section">
        <div class="user-info">
          <div class="user-welcome">
            <span class="welcome-text">Welcome, <strong>{{ userInfo?.userName || userInfo?.userEmail
                }}</strong>!</span>
            <span class="user-role">{{ getRoleDisplayName(userInfo?.userRole) }}</span>
          </div>
        </div>

        <form @submit.prevent="createBlog" class="blog-form">
          <!-- Title Input -->
          <div class="form-group">
            <label for="title" class="form-label">
              <i class="icon">📝</i>
              Blog Title *
            </label>
            <input type="text" id="title" v-model="blogForm.title" :class="['form-input', { 'error': errors.title }]"
              placeholder="Enter your blog title..." maxlength="200" @input="clearError('title')" />
            <div class="char-count">{{ blogForm.title.length }}/200</div>
            <div v-if="errors.title" class="error-message">{{ errors.title }}</div>
          </div>

          <!-- Description/Content Input -->
          <div class="form-group">
            <label for="description" class="form-label">
              <i class="icon">📄</i>
              Blog Content (Markdown supported) *
            </label>
            <div class="markdown-editor">
              <div class="editor-toolbar">
                <button type="button" @click="insertMarkdown('**', '**')" class="toolbar-btn" title="Bold">
                  <strong>B</strong>
                </button>
                <button type="button" @click="insertMarkdown('*', '*')" class="toolbar-btn" title="Italic">
                  <em>I</em>
                </button>
                <button type="button" @click="insertMarkdown('# ', '')" class="toolbar-btn" title="Heading">
                  H1
                </button>
                <button type="button" @click="insertMarkdown('- ', '')" class="toolbar-btn" title="List">
                  •
                </button>
                <button type="button" @click="insertMarkdown('[link text](url)', '')" class="toolbar-btn" title="Link">
                  🔗
                </button>
              </div>
              <textarea id="description" v-model="blogForm.descriptionMarkdown"
                :class="['form-textarea', { 'error': errors.descriptionMarkdown }]"
                placeholder="Write your blog content here..." rows="12" maxlength="10000"
                @input="clearError('descriptionMarkdown')"></textarea>
            </div>
            <div class="char-count">{{ blogForm.descriptionMarkdown.length }}/10000</div>
            <div v-if="errors.descriptionMarkdown" class="error-message">{{ errors.descriptionMarkdown }}</div>
          </div>

          <!-- Image Upload -->
          <div class="form-group">
            <label for="image" class="form-label">
              <i class="icon">🖼️</i>
              Blog Image (optional)
            </label>
            <div class="image-upload-area">
              <input type="file" id="image" ref="imageInput" @change="handleImageChange" accept="image/*"
                class="file-input" />
              <div v-if="!imagePreview" class="upload-placeholder" @click="$refs.imageInput.click()">
                <div class="upload-icon">📷</div>
                <p>Click to select an image</p>
                <p class="upload-hint">PNG, JPG, GIF up to 5MB</p>
              </div>
              <div v-if="imagePreview" class="image-preview">
                <img :src="imagePreview" alt="Blog image" class="preview-img" />
                <button type="button" @click="removeImage" class="remove-image-btn">
                  ✕ Remove image
                </button>
              </div>
            </div>
            <div v-if="errors.image" class="error-message">{{ errors.image }}</div>
          </div>

          <!-- Form Actions -->
          <div class="form-actions">
            <button type="button" @click="resetForm" class="btn btn-secondary" :disabled="isSubmitting">
              <i class="icon">🔄</i>
              Reset
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting || !isFormValid">
              <div v-if="isSubmitting" class="loading-spinner"></div>
              <i v-else class="icon">✨</i>
              {{ isSubmitting ? 'Creating...' : 'Create Blog' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Preview Section -->
      <div v-if="blogForm.title || blogForm.descriptionMarkdown" class="preview-section">
        <h2 class="preview-title">👀 Blog Preview</h2>
        <div class="blog-preview">
          <div class="preview-header">
            <h3 class="preview-blog-title">{{ blogForm.title || 'Blog Title' }}</h3>
            <div class="preview-meta">
              <span class="preview-author">By: {{ userInfo?.userName || userInfo?.userEmail }}</span>
              <span class="preview-date">{{ getCurrentDate() }}</span>
            </div>
          </div>
          <div v-if="imagePreview" class="preview-image">
            <img :src="imagePreview" alt="Blog image" />
          </div>
          <div class="preview-content" v-html="getMarkdownPreview()"></div>
        </div>
      </div>
    </template>
    </div>
  </div>
</template>
<script>
import { BlogsService } from '../services/blogs_service.js';
import { AuthService } from '../services/auth_service.js';
import Navbar from './Navbar.vue';

export default {
  name: 'Blogs',
  components: {
    Navbar
  },
  data() {
    return {
      activeTab: 'view', // 'view' or 'create'
      
      // Blog creation form data
      blogForm: {
        title: '',
        descriptionMarkdown: '',
        imageBase64: ''
      },
      imagePreview: null,
      selectedFile: null,
      errors: {},
      isSubmitting: false,
      
      // Authentication
      isAuthenticated: false,
      userInfo: null,
      
      // Blog listing data
      blogs: [],
      loadingBlogs: false,
      blogsError: null,
      currentPage: 1,
      pageSize: 10,
      likingBlogs: new Set()
    };
  },
  async mounted() {
    this.checkAuthentication();
    await this.loadBlogs();
  },
  computed: {
    isFormValid() {
      return this.blogForm.title.trim() !== '' &&
        this.blogForm.descriptionMarkdown.trim() !== '' &&
        Object.keys(this.errors).length === 0 &&
        this.isAuthenticated;
    }
  },
  methods: {
    checkAuthentication() {
      const token = localStorage.getItem('token');
      if (!token) {
        this.redirectToLogin();
        return;
      }

      try {
        this.userInfo = AuthService.decode(token);
        if (!this.userInfo) {
          this.redirectToLogin();
          return;
        }
        this.isAuthenticated = true;
      } catch (error) {
        console.error('Error checking authentication:', error);
        this.redirectToLogin();
      }
    },

    redirectToLogin() {
      this.$toast?.error?.('You must be logged in to create a blog');
      this.$router.push('/login');
    },

    async loadBlogs(page = 1) {
      this.loadingBlogs = true;
      this.blogsError = null;
      this.currentPage = page;

      try {
        const response = await BlogsService.getAllBlogs(page, this.pageSize);
        this.blogs = response || [];
        console.log('Loaded blogs:', this.blogs);
      } catch (error) {
        console.error('Error loading blogs:', error);
        this.blogsError = error.response?.data?.message || 'Failed to load blogs';
      } finally {
        this.loadingBlogs = false;
      }
    },

    async viewBlog(blogId) {
      // Navigate to blog detail page
      this.$router.push(`/blog/${blogId}`);
    },

    async toggleLike(blog) {
      if (!this.isAuthenticated) {
        this.$toast?.error?.('You must be logged in to like blogs');
        return;
      }

      this.likingBlogs.add(blog.id);

      try {
        if (blog.isLikedByCurrentUser) {
          // Dislike - uklanjanje lajka
          await BlogsService.dislikeBlog(blog.id);
          blog.isLikedByCurrentUser = false;
          blog.likesCount = Math.max(0, blog.likesCount - 1);
          this.$toast?.success?.('Like removed! 💔');
        } else {
          // Like
          await BlogsService.likeBlog(blog.id);
          blog.isLikedByCurrentUser = true;
          blog.likesCount = blog.likesCount + 1;
          this.$toast?.success?.('Blog liked! ❤️');
        }
      } catch (error) {
        console.error('Error toggling like:', error);
        this.$toast?.error?.('Failed to update like status');
      } finally {
        this.likingBlogs.delete(blog.id);
      }
    },

    formatDate(dateString) {
      try {
        const date = new Date(dateString);
        return date.toLocaleDateString('sr-RS', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      } catch (error) {
        return 'Unknown date';
      }
    },

    getPlainTextPreview(markdown) {
      if (!markdown) return 'No content available...';
      
      // Remove markdown formatting and limit to 150 characters
      const plainText = markdown
        .replace(/[#*_`]/g, '')
        .replace(/\n/g, ' ')
        .trim();
      
      return plainText.length > 150 
        ? plainText.substring(0, 150) + '...'
        : plainText;
    },

    redirectToLogin() {

    },

    getRoleDisplayName(role) {
      const roleMap = {
        'Admin': 'Administrator',
        'Author': 'Author',
        'Tourist': 'Tourist'
      };
      return roleMap[role] || role || 'User';
    },
    async createBlog() {
      if (!this.validateForm()) {
        return;
      }

      this.isSubmitting = true;

      try {
        // Convert image to base64 if selected
        if (this.selectedFile) {
          this.blogForm.imageBase64 = await BlogsService.fileToBase64(this.selectedFile);
        }

        const response = await BlogsService.createBlog(this.blogForm);

        this.$toast?.success?.('Blog created successfully! 🎉');
        this.resetForm();

        // Opciono: preusmeri na listu blogova ili početnu stranu
        // this.$router.push('/blogs-list');

      } catch (error) {
        console.error('Error creating blog:', error);
        let errorMessage = 'Error creating blog';

        if (error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error.response?.status === 401) {
          errorMessage = 'You must be logged in to create a blog';
        } else if (error.response?.status === 400) {
          errorMessage = 'Invalid blog data';
        }

        this.$toast?.error?.(errorMessage);
      } finally {
        this.isSubmitting = false;
      }
    },

    validateForm() {
      this.errors = {};

      const validationErrors = BlogsService.validateBlogData(this.blogForm);

      if (validationErrors.length > 0) {
        validationErrors.forEach(error => {
          if (error.includes('Title')) {
            this.errors.title = error;
          } else if (error.includes('Description')) {
            this.errors.descriptionMarkdown = error;
          }
        });
      }

      // Image validation
      if (this.selectedFile) {
        const maxSize = 5 * 1024 * 1024; // 5MB
        if (this.selectedFile.size > maxSize) {
          this.errors.image = 'Image cannot be larger than 5MB';
        }

        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(this.selectedFile.type)) {
          this.errors.image = 'Only JPEG, PNG, GIF or WebP format allowed';
        }
      }

      return Object.keys(this.errors).length === 0;
    },

    clearError(field) {
      if (this.errors[field]) {
        delete this.errors[field];
      }
    },

    async handleImageChange(event) {
      const file = event.target.files[0];
      if (!file) {
        this.removeImage();
        return;
      }

      this.selectedFile = file;

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);

      this.clearError('image');
    },

    removeImage() {
      this.selectedFile = null;
      this.imagePreview = null;
      this.blogForm.imageBase64 = '';
      this.$refs.imageInput.value = '';
      this.clearError('image');
    },

    resetForm() {
      this.blogForm = {
        title: '',
        descriptionMarkdown: '',
        imageBase64: ''
      };
      this.removeImage();
      this.errors = {};
    },

    insertMarkdown(before, after) {
      const textarea = document.getElementById('description');
      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      const selectedText = textarea.value.substring(start, end);

      const newText = before + selectedText + after;
      const beforeText = textarea.value.substring(0, start);
      const afterText = textarea.value.substring(end);

      this.blogForm.descriptionMarkdown = beforeText + newText + afterText;

      // Set cursor position
      this.$nextTick(() => {
        textarea.focus();
        textarea.setSelectionRange(start + before.length, start + before.length + selectedText.length);
      });
    },

    getCurrentDate() {
      return new Date().toLocaleDateString('sr-RS', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    getMarkdownPreview() {
      if (!this.blogForm.descriptionMarkdown) {
        return '<p class="empty-content">Blog content will be displayed here...</p>';
      }

      // Simple markdown to HTML conversion
      return this.blogForm.descriptionMarkdown
        .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.*?)\*/g, '<em>$1</em>')
        .replace(/^# (.*$)/gm, '<h1>$1</h1>')
        .replace(/^## (.*$)/gm, '<h2>$1</h2>')
        .replace(/^### (.*$)/gm, '<h3>$1</h3>')
        .replace(/^\- (.*$)/gm, '<li>$1</li>')
        .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank">$1</a>')
        .replace(/\n/g, '<br>');
    }
  }
};
</script>

<style scoped>
.blogs-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 80px 20px 20px 20px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
  min-height: 100vh;
}

.header {
  grid-column: 1 / -1;
  text-align: center;
  margin-bottom: 20px;
}

.header h1 {
  color: #333;
  margin-bottom: 10px;
  font-size: 32px;
  font-weight: 700;
}

.subtitle {
  color: #666;
  font-size: 18px;
  margin: 0;
}

/* Tab Navigation */
.tab-navigation {
  grid-column: 1 / -1;
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-bottom: 30px;
  padding: 0 20px;
}

.tab-btn {
  padding: 12px 24px;
  border: 2px solid #e0e0e0;
  background: white;
  border-radius: 25px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 600;
  color: #666;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tab-btn:hover {
  border-color: #007bff;
  background: #f8f9ff;
  color: #007bff;
}

.tab-btn.active {
  background: linear-gradient(135deg, #007bff, #0056b3);
  border-color: #007bff;
  color: white;
  box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
}

/* Blog List Section */
.blogs-list-section {
  grid-column: 1 / -1;
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.blogs-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  padding-bottom: 15px;
  border-bottom: 2px solid #f0f0f0;
}

.blogs-header h2 {
  color: #333;
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.pagination-info {
  color: #666;
  font-size: 14px;
}

/* Loading and Error States */
.loading-state {
  text-align: center;
  padding: 60px 20px;
  color: #666;
}

.loading-spinner-large {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

.error-state {
  text-align: center;
  padding: 60px 20px;
  color: #dc3545;
}

.error-icon {
  font-size: 48px;
  margin-bottom: 15px;
}

.error-state h3 {
  color: #dc3545;
  margin: 0 0 10px 0;
  font-size: 20px;
}

.error-state p {
  color: #666;
  margin-bottom: 20px;
}

/* Blogs Grid */
.blogs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
  margin-bottom: 30px;
}

.no-blogs {
  grid-column: 1 / -1;
  text-align: center;
  padding: 80px 20px;
  background: #f8f9fa;
  border-radius: 16px;
  border: 2px dashed #ddd;
}

.no-blogs-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.no-blogs h3 {
  color: #333;
  margin: 0 0 10px 0;
  font-size: 24px;
}

.no-blogs p {
  color: #666;
  margin-bottom: 25px;
  font-size: 16px;
}

/* Blog Cards */
.blog-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  cursor: pointer;
  border: 1px solid #f0f0f0;
}

.blog-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
  border-color: #007bff;
}

.blog-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
}

.blog-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.blog-card:hover .blog-image img {
  transform: scale(1.05);
}

.blog-content {
  padding: 20px;
}

.blog-title {
  color: #333;
  margin: 0 0 10px 0;
  font-size: 18px;
  font-weight: 600;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.blog-meta {
  display: flex;
  justify-content: space-between;
  margin-bottom: 15px;
  font-size: 12px;
  color: #666;
}

.blog-author,
.blog-date {
  display: flex;
  align-items: center;
  gap: 4px;
}

.blog-description {
  color: #555;
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 20px;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.blog-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 15px;
  border-top: 1px solid #f0f0f0;
}

.blog-actions-right {
  display: flex;
  gap: 10px;
  align-items: center;
}

.likes-info {
  display: flex;
  align-items: center;
}

.likes-count {
  font-size: 12px;
  color: #666;
  margin-right: 8px;
  font-weight: 500;
}

.btn-outline {
  background: transparent;
  color: #007bff;
  border: 1px solid #007bff;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  text-decoration: none;
  transition: all 0.2s ease;
}

.btn-outline:hover {
  background: #007bff;
  color: white;
}

.btn-like {
  background: transparent;
  color: #dc3545;
  border: 1px solid #dc3545;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-like:hover:not(:disabled) {
  background: #dc3545;
  color: white;
}

.btn-liked {
  background: #dc3545;
  color: white;
  border: 1px solid #dc3545;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-liked:hover:not(:disabled) {
  background: #c82333;
  border-color: #c82333;
}

.btn-like:disabled,
.btn-liked:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #f0f0f0;
}

.page-info {
  color: #666;
  font-weight: 500;
}

.auth-required {
  grid-column: 1 / -1;
  background: white;
  border-radius: 16px;
  padding: 60px 40px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  text-align: center;
  border: 2px solid #f8f9fa;
}

.auth-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.auth-required h2 {
  color: #333;
  margin-bottom: 15px;
  font-size: 28px;
}

.auth-required p {
  color: #666;
  font-size: 16px;
  margin-bottom: 30px;
  max-width: 400px;
  margin-left: auto;
  margin-right: auto;
}

.auth-actions {
  display: flex;
  gap: 15px;
  justify-content: center;
  flex-wrap: wrap;
}

.create-blog-section {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  height: fit-content;
}

.user-info {
  margin-bottom: 25px;
  padding: 15px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  color: white;
}

.user-welcome {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.welcome-text {
  font-size: 16px;
}

.user-role {
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
}

.blog-form {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #333;
  font-size: 16px;
}

.icon {
  font-size: 18px;
}

.form-input {
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  font-size: 16px;
  transition: all 0.2s ease;
  background: #fafafa;
}

.form-input:focus {
  outline: none;
  border-color: #007bff;
  background: white;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

.form-input.error {
  border-color: #dc3545;
  background: #fef5f5;
}

.markdown-editor {
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  overflow: hidden;
  background: white;
}

.editor-toolbar {
  display: flex;
  gap: 5px;
  padding: 10px;
  background: #f8f9fa;
  border-bottom: 1px solid #e0e0e0;
}

.toolbar-btn {
  padding: 6px 10px;
  border: 1px solid #ddd;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s ease;
}

.toolbar-btn:hover {
  background: #e9ecef;
  border-color: #007bff;
}

.form-textarea {
  width: 100%;
  padding: 16px;
  border: none;
  font-size: 16px;
  line-height: 1.6;
  resize: vertical;
  min-height: 200px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  box-sizing: border-box;
}

.form-textarea:focus {
  outline: none;
}

.form-textarea.error {
  background: #fef5f5;
}

.char-count {
  text-align: right;
  font-size: 12px;
  color: #888;
  margin-top: 5px;
}

.error-message {
  color: #dc3545;
  font-size: 14px;
  margin-top: 5px;
}

.image-upload-area {
  border: 2px dashed #ddd;
  border-radius: 10px;
  padding: 20px;
  text-align: center;
  transition: all 0.2s ease;
}

.image-upload-area:hover {
  border-color: #007bff;
  background: #f8f9ff;
}

.file-input {
  display: none;
}

.upload-placeholder {
  cursor: pointer;
  padding: 20px;
}

.upload-icon {
  font-size: 48px;
  margin-bottom: 15px;
}

.upload-placeholder p {
  margin: 10px 0;
  color: #333;
}

.upload-hint {
  font-size: 14px;
  color: #666;
}

.image-preview {
  position: relative;
}

.preview-img {
  max-width: 100%;
  max-height: 200px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.remove-image-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(255, 255, 255, 0.9);
  border: none;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  color: #dc3545;
  transition: all 0.2s ease;
}

.remove-image-btn:hover {
  background: #dc3545;
  color: white;
}

.form-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
  margin-top: 20px;
}

.btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: linear-gradient(135deg, #007bff, #0056b3);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 123, 255, 0.3);
}

.btn-secondary {
  background: #f8f9fa;
  color: #666;
  border: 1px solid #ddd;
}

.btn-secondary:hover:not(:disabled) {
  background: #e9ecef;
  border-color: #adb5bd;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.preview-section {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  height: fit-content;
  position: sticky;
  top: 20px;
}

.preview-title {
  color: #333;
  margin-bottom: 20px;
  font-size: 24px;
  font-weight: 600;
  border-bottom: 2px solid #f0f0f0;
  padding-bottom: 10px;
}

.blog-preview {
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  padding: 25px;
  background: #fafafa;
}

.preview-header {
  margin-bottom: 20px;
  border-bottom: 1px solid #e0e0e0;
  padding-bottom: 15px;
}

.preview-blog-title {
  color: #333;
  margin: 0 0 10px 0;
  font-size: 22px;
  font-weight: 700;
}

.preview-meta {
  display: flex;
  gap: 20px;
  font-size: 14px;
  color: #666;
}

.preview-image {
  margin-bottom: 20px;
}

.preview-image img {
  width: 100%;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.preview-content {
  line-height: 1.6;
  color: #555;
}

.preview-content h1,
.preview-content h2,
.preview-content h3 {
  color: #333;
  margin: 20px 0 10px 0;
}

.preview-content h1 {
  font-size: 24px;
}

.preview-content h2 {
  font-size: 20px;
}

.preview-content h3 {
  font-size: 18px;
}

.preview-content li {
  margin: 5px 0;
  list-style: disc;
  margin-left: 20px;
}

.preview-content a {
  color: #007bff;
  text-decoration: underline;
}

.empty-content {
  color: #999;
  font-style: italic;
  text-align: center;
  padding: 40px;
}

@media (max-width: 1024px) {
  .blogs-container {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .preview-section {
    position: static;
  }
}

@media (max-width: 768px) {
  .blogs-container {
    padding: 15px;
  }

  .create-blog-section,
  .preview-section {
    padding: 20px;
  }

  .header h1 {
    font-size: 28px;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn {
    justify-content: center;
  }
}
</style>