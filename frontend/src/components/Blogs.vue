<template>
  <div>
    <!-- Navigation -->
    <Navbar />

    <div class="blogs-container">
      <div class="header">
        <h1>📝 Kreiranje Bloga</h1>
        <p class="subtitle">Podelite svoje putničke doživljaje sa svetom</p>
      </div>

    <!-- Authentication Required Message -->
    <div v-if="!isAuthenticated" class="auth-required">
      <div class="auth-icon">🔒</div>
      <h2>Potrebna je prijava</h2>
      <p>Da biste kreirali blog, morate biti ulogovani na platformu.</p>
      <div class="auth-actions">
        <router-link to="/login" class="btn btn-primary">
          <i class="icon">🔑</i>
          Prijavite se
        </router-link>
        <router-link to="/register" class="btn btn-secondary">
          <i class="icon">👤</i>
          Registrujte se
        </router-link>
      </div>
    </div>

    <!-- Blog Creation Form - Only show if authenticated -->
    <template v-if="isAuthenticated">
      <div class="create-blog-section">
        <div class="user-info">
          <div class="user-welcome">
            <span class="welcome-text">Dobrodošli, <strong>{{ userInfo?.userName || userInfo?.userEmail
                }}</strong>!</span>
            <span class="user-role">{{ getRoleDisplayName(userInfo?.userRole) }}</span>
          </div>
        </div>

        <form @submit.prevent="createBlog" class="blog-form">
          <!-- Title Input -->
          <div class="form-group">
            <label for="title" class="form-label">
              <i class="icon">📝</i>
              Naslov bloga *
            </label>
            <input type="text" id="title" v-model="blogForm.title" :class="['form-input', { 'error': errors.title }]"
              placeholder="Unesite naslov vašeg bloga..." maxlength="200" @input="clearError('title')" />
            <div class="char-count">{{ blogForm.title.length }}/200</div>
            <div v-if="errors.title" class="error-message">{{ errors.title }}</div>
          </div>

          <!-- Description/Content Input -->
          <div class="form-group">
            <label for="description" class="form-label">
              <i class="icon">📄</i>
              Sadržaj bloga (Markdown podržan) *
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
                placeholder="Napišite sadržaj vašeg bloga ovde..." rows="12" maxlength="10000"
                @input="clearError('descriptionMarkdown')"></textarea>
            </div>
            <div class="char-count">{{ blogForm.descriptionMarkdown.length }}/10000</div>
            <div v-if="errors.descriptionMarkdown" class="error-message">{{ errors.descriptionMarkdown }}</div>
          </div>

          <!-- Image Upload -->
          <div class="form-group">
            <label for="image" class="form-label">
              <i class="icon">🖼️</i>
              Slika bloga (opciono)
            </label>
            <div class="image-upload-area">
              <input type="file" id="image" ref="imageInput" @change="handleImageChange" accept="image/*"
                class="file-input" />
              <div v-if="!imagePreview" class="upload-placeholder" @click="$refs.imageInput.click()">
                <div class="upload-icon">📷</div>
                <p>Kliknite da izaberete sliku</p>
                <p class="upload-hint">PNG, JPG, GIF do 5MB</p>
              </div>
              <div v-if="imagePreview" class="image-preview">
                <img :src="imagePreview" alt="Blog slika" class="preview-img" />
                <button type="button" @click="removeImage" class="remove-image-btn">
                  ✕ Ukloni sliku
                </button>
              </div>
            </div>
            <div v-if="errors.image" class="error-message">{{ errors.image }}</div>
          </div>

          <!-- Form Actions -->
          <div class="form-actions">
            <button type="button" @click="resetForm" class="btn btn-secondary" :disabled="isSubmitting">
              <i class="icon">🔄</i>
              Resetuj
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting || !isFormValid">
              <div v-if="isSubmitting" class="loading-spinner"></div>
              <i v-else class="icon">✨</i>
              {{ isSubmitting ? 'Kreiram...' : 'Kreiraj Blog' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Preview Section -->
      <div v-if="blogForm.title || blogForm.descriptionMarkdown" class="preview-section">
        <h2 class="preview-title">👀 Pregled bloga</h2>
        <div class="blog-preview">
          <div class="preview-header">
            <h3 class="preview-blog-title">{{ blogForm.title || 'Naslov bloga' }}</h3>
            <div class="preview-meta">
              <span class="preview-author">By: {{ userInfo?.userName || userInfo?.userEmail }}</span>
              <span class="preview-date">{{ getCurrentDate() }}</span>
            </div>
          </div>
          <div v-if="imagePreview" class="preview-image">
            <img :src="imagePreview" alt="Blog slika" />
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
      blogForm: {
        title: '',
        descriptionMarkdown: '',
        imageBase64: ''
      },
      imagePreview: null,
      selectedFile: null,
      errors: {},
      isSubmitting: false,
      isAuthenticated: false,
      userInfo: null
    };
  },
  async mounted() {
    this.checkAuthentication();
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
      this.$toast?.error?.('Morate biti ulogovani da biste kreirali blog');
      this.$router.push('/login');
    },

    getRoleDisplayName(role) {
      const roleMap = {
        'Admin': 'Administrator',
        'Author': 'Autor',
        'Tourist': 'Turista'
      };
      return roleMap[role] || role || 'Korisnik';
    },
    async createBlog() {
      if (!this.validateForm()) {
        return;
      }

      this.isSubmitting = true;

      try {
        // Konvertuj sliku u base64 ako je izabrana
        if (this.selectedFile) {
          this.blogForm.imageBase64 = await BlogsService.fileToBase64(this.selectedFile);
        }

        const response = await BlogsService.createBlog(this.blogForm);

        this.$toast?.success?.('Blog je uspešno kreiran! 🎉');
        this.resetForm();

        // Opciono: preusmeri na listu blogova ili početnu stranu
        // this.$router.push('/blogs-list');

      } catch (error) {
        console.error('Error creating blog:', error);
        let errorMessage = 'Greška pri kreiranju bloga';

        if (error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error.response?.status === 401) {
          errorMessage = 'Morate biti ulogovani da biste kreirali blog';
        } else if (error.response?.status === 400) {
          errorMessage = 'Neispravni podaci za blog';
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
          if (error.includes('Naslov')) {
            this.errors.title = error;
          } else if (error.includes('Opis')) {
            this.errors.descriptionMarkdown = error;
          }
        });
      }

      // Validacija slike
      if (this.selectedFile) {
        const maxSize = 5 * 1024 * 1024; // 5MB
        if (this.selectedFile.size > maxSize) {
          this.errors.image = 'Slika ne može biti veća od 5MB';
        }

        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(this.selectedFile.type)) {
          this.errors.image = 'Dozvoljen je samo JPEG, PNG, GIF ili WebP format';
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
        return '<p class="empty-content">Sadržaj bloga će se prikazati ovde...</p>';
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