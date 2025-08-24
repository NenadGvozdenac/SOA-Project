<template>
  <div>
    <!-- Navigation -->
    <Navbar />

    <div class="blog-detail-container">
      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="loading-spinner-large"></div>
        <p>Loading blog...</p>
      </div>

      <!-- Error State -->
      <div v-if="error" class="error-state">
        <div class="error-icon">⚠️</div>
        <h3>Error Loading Blog</h3>
        <p>{{ error }}</p>
        <button @click="loadBlog" class="btn btn-primary">
          🔄 Try Again
        </button>
      </div>

      <!-- Blog Detail -->
      <div v-if="!loading && !error && blog" class="blog-detail">
        <!-- Blog Header -->
        <div class="blog-header">
          <button @click="$router.go(-1)" class="back-btn">
            ← Back to Blogs
          </button>
          <h1 class="blog-title">{{ blog.title }}</h1>
          <div class="blog-meta">
            <div class="meta-item">
              <span class="meta-icon">👤</span>
              <span>Author: {{ blog.userId }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-icon">📅</span>
              <span>{{ formatDate(blog.createdAt) }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-icon">❤️</span>
              <span>{{ blog.likesCount || 0 }} likes</span>
            </div>
          </div>
        </div>

        <!-- Blog Image -->
        <div v-if="blog.imageBase64" class="blog-image">
          <img :src="`data:image/jpeg;base64,${blog.imageBase64}`" :alt="blog.title" />
        </div>

        <!-- Blog Content -->
        <div class="blog-content">
          <div class="markdown-content" v-html="renderMarkdown(blog.descriptionMarkdown)"></div>
        </div>

        <!-- Like Section -->
        <div class="blog-actions">
          <button 
            @click="toggleLike" 
            :class="['btn', blog.isLikedByCurrentUser ? 'btn-liked' : 'btn-like']"
            :disabled="likingBlog"
            v-if="isAuthenticated"
          >
            {{ blog.isLikedByCurrentUser ? '💖' : '❤️' }} 
            {{ blog.isLikedByCurrentUser ? 'Unlike' : 'Like' }}
            ({{ blog.likesCount || 0 }})
          </button>
        </div>

        <!-- Comments Section -->
        <div class="comments-section">
          <h2 class="comments-title">
            💬 Comments ({{ comments.length }})
          </h2>

          <!-- Add Comment Form -->
          <div v-if="isAuthenticated" class="add-comment-form">
            <h3>Add Your Comment</h3>
            <form @submit.prevent="submitComment" class="comment-form">
              <div class="form-group">
                <textarea 
                  v-model="newComment" 
                  :class="['comment-textarea', { 'error': commentError }]"
                  placeholder="Write your comment here..." 
                  rows="4"
                  maxlength="1000"
                  @input="clearCommentError"
                ></textarea>
                <div class="char-count">{{ newComment.length }}/1000</div>
                <div v-if="commentError" class="error-message">{{ commentError }}</div>
              </div>
              <div class="form-actions">
                <button 
                  type="submit" 
                  class="btn btn-primary"
                  :disabled="isSubmittingComment || !newComment.trim()"
                >
                  <div v-if="isSubmittingComment" class="loading-spinner"></div>
                  <span v-else>💬 Post Comment</span>
                </button>
              </div>
            </form>
          </div>

          <!-- Login Required Message -->
          <div v-if="!isAuthenticated" class="auth-required-comment">
            <p>Please <router-link to="/login">login</router-link> to leave a comment.</p>
          </div>

          <!-- Comments List -->
          <div class="comments-list">
            <div v-if="loadingComments" class="loading-comments">
              <div class="loading-spinner"></div>
              <span>Loading comments...</span>
            </div>

            <div v-if="!loadingComments && comments.length === 0" class="no-comments">
              <div class="no-comments-icon">💭</div>
              <p>No comments yet. Be the first to comment!</p>
            </div>

            <div v-for="comment in comments" :key="comment.id" class="comment-item">
              <div class="comment-header">
                <div class="comment-author">
                  <span class="author-icon">👤</span>
                  <strong>User {{ comment.userId }}</strong>
                </div>
                <div class="comment-meta">
                  <div class="comment-dates">
                    <span class="comment-date">{{ formatDate(comment.createdAt) }}</span>
                    <span v-if="comment.updatedAt && comment.updatedAt !== comment.createdAt" class="edited-date">
                      • edited {{ formatDate(comment.updatedAt) }}
                    </span>
                  </div>
                  <div v-if="canEditComment(comment)" class="comment-actions">
                    <button @click="startEditComment(comment)" class="edit-btn" v-if="!isEditingComment(comment.id)">
                      ✏️ Edit
                    </button>
                    <button @click="cancelEditComment" class="cancel-btn" v-if="isEditingComment(comment.id)">
                      ❌ Cancel
                    </button>
                  </div>
                </div>
              </div>
              
              <!-- Edit Mode -->
              <div v-if="isEditingComment(comment.id)" class="edit-comment-form">
                <textarea 
                  v-model="editingCommentText" 
                  :class="['comment-textarea', { 'error': editCommentError }]"
                  rows="3"
                  maxlength="1000"
                  @input="clearEditCommentError"
                ></textarea>
                <div class="char-count">{{ editingCommentText.length }}/1000</div>
                <div v-if="editCommentError" class="error-message">{{ editCommentError }}</div>
                <div class="edit-actions">
                  <button 
                    @click="saveEditComment" 
                    class="btn btn-primary btn-sm"
                    :disabled="isUpdatingComment || !editingCommentText.trim()"
                  >
                    <div v-if="isUpdatingComment" class="loading-spinner"></div>
                    <span v-else>💾 Save</span>
                  </button>
                </div>
              </div>
              
              <!-- View Mode -->
              <div v-else class="comment-content">
                {{ comment.content }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { BlogsService } from '../services/blogs_service.js';
import { AuthService } from '../services/auth_service.js';
import Navbar from './Navbar.vue';

export default {
  name: 'BlogDetail',
  components: {
    Navbar
  },
  data() {
    return {
      blog: null,
      comments: [],
      loading: false,
      loadingComments: false,
      error: null,
      likingBlog: false,
      
      // Authentication
      isAuthenticated: false,
      userInfo: null,
      
      // Comment form
      newComment: '',
      isSubmittingComment: false,
      commentError: null,
      
      // Edit comment
      editingCommentId: null,
      editingCommentText: '',
      isUpdatingComment: false,
      editCommentError: null
    };
  },
  async mounted() {
    this.checkAuthentication();
    await this.loadBlog();
    await this.loadComments();
  },
  methods: {
    checkAuthentication() {
      const token = localStorage.getItem('token');
      if (token) {
        try {
          this.userInfo = AuthService.decode(token);
          this.isAuthenticated = !!this.userInfo;
        } catch (error) {
          console.error('Error checking authentication:', error);
          this.isAuthenticated = false;
        }
      }
    },

    async loadBlog() {
      const blogId = this.$route.params.id;
      if (!blogId) {
        this.error = 'Blog ID not found';
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        this.blog = await BlogsService.getBlogById(blogId);
      } catch (error) {
        console.error('Error loading blog:', error);
        this.error = error.response?.data?.message || 'Failed to load blog';
      } finally {
        this.loading = false;
      }
    },

    async loadComments() {
      const blogId = this.$route.params.id;
      if (!blogId) return;

      this.loadingComments = true;

      try {
        this.comments = await BlogsService.getComments(blogId);
        console.log('Loaded comments in component:', this.comments);
      } catch (error) {
        console.error('Error loading comments:', error);
        // Don't show error for comments, just log it
      } finally {
        this.loadingComments = false;
      }
    },

    async toggleLike() {
      if (!this.isAuthenticated || !this.blog) return;

      this.likingBlog = true;

      try {
        if (this.blog.isLikedByCurrentUser) {
          await BlogsService.dislikeBlog(this.blog.id);
          this.blog.isLikedByCurrentUser = false;
          this.blog.likesCount = Math.max(0, this.blog.likesCount - 1);
          this.$toast?.success?.('Like removed! 💔');
        } else {
          await BlogsService.likeBlog(this.blog.id);
          this.blog.isLikedByCurrentUser = true;
          this.blog.likesCount = this.blog.likesCount + 1;
          this.$toast?.success?.('Blog liked! ❤️');
        }
      } catch (error) {
        console.error('Error toggling like:', error);
        this.$toast?.error?.('Failed to update like status');
      } finally {
        this.likingBlog = false;
      }
    },

    async submitComment() {
      if (!this.newComment.trim()) {
        this.commentError = 'Comment cannot be empty';
        return;
      }

      this.isSubmittingComment = true;
      this.commentError = null;

      try {
        await BlogsService.createComment(this.$route.params.id, this.newComment.trim());
        this.newComment = '';
        this.$toast?.success?.('Comment posted successfully! 💬');
        
        // Reload comments
        await this.loadComments();
      } catch (error) {
        console.error('Error submitting comment:', error);
        this.commentError = error.response?.data?.message || 'Failed to post comment';
      } finally {
        this.isSubmittingComment = false;
      }
    },

    clearCommentError() {
      this.commentError = null;
    },

    // Comment editing methods
    canEditComment(comment) {
      // console.log(this.userInfo.userID)
      // console.log(comment.userId)
      // console.log(this.userInfo.userID == comment.userId)
      return this.userInfo && this.userInfo.userID == comment.userId;
    },
    
    isEditingComment(commentId) {
      return this.editingCommentId == commentId;
    },
    
    startEditComment(comment) {
      this.editingCommentId = comment.id;
      this.editingCommentText = comment.content;
      this.editCommentError = null;
    },
    
    cancelEditComment() {
      this.editingCommentId = null;
      this.editingCommentText = '';
      this.editCommentError = null;
    },
    
    async saveEditComment() {
      if (!this.editingCommentText.trim()) {
        this.editCommentError = 'Comment cannot be empty';
        return;
      }
      
      this.isUpdatingComment = true;
      this.editCommentError = null;
      
      try {
        await BlogsService.updateComment(this.editingCommentId, this.editingCommentText.trim());
        
        // Update comment in local state
        const commentIndex = this.comments.findIndex(c => c.id === this.editingCommentId);
        if (commentIndex !== -1) {
          this.comments[commentIndex].content = this.editingCommentText.trim();
          this.comments[commentIndex].updatedAt = new Date().toISOString();
        }
        
        this.cancelEditComment();
        this.$toast?.success?.('Comment updated successfully! ✏️');
      } catch (error) {
        console.error('Error updating comment:', error);
        this.editCommentError = error.response?.data?.message || 'Failed to update comment';
      } finally {
        this.isUpdatingComment = false;
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

    renderMarkdown(markdown) {
      if (!markdown) return '';
      
      // Simple markdown to HTML conversion
      return markdown
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
.blog-detail-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 80px 20px 20px 20px;
  min-height: 100vh;
}

/* Loading and Error States */
.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
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

.error-icon {
  font-size: 48px;
  margin-bottom: 15px;
}

.error-state h3 {
  color: #dc3545;
  margin: 0 0 10px 0;
}

/* Blog Detail */
.blog-detail {
  background: white;
  border-radius: 16px;
  padding: 40px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.blog-header {
  margin-bottom: 30px;
}

.back-btn {
  background: transparent;
  border: 1px solid #ddd;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 20px;
  color: #666;
  transition: all 0.2s ease;
}

.back-btn:hover {
  background: #f8f9fa;
  border-color: #007bff;
  color: #007bff;
}

.blog-title {
  color: #333;
  margin: 0 0 20px 0;
  font-size: 32px;
  font-weight: 700;
  line-height: 1.3;
}

.blog-meta {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #666;
  font-size: 14px;
}

.meta-icon {
  font-size: 16px;
}

.blog-image {
  margin-bottom: 30px;
  text-align: center;
}

.blog-image img {
  max-width: 100%;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.blog-content {
  margin-bottom: 30px;
  line-height: 1.8;
  font-size: 16px;
}

.markdown-content {
  color: #333;
}

.markdown-content h1,
.markdown-content h2,
.markdown-content h3 {
  margin: 30px 0 15px 0;
  color: #333;
}

.markdown-content h1 {
  font-size: 28px;
  border-bottom: 2px solid #f0f0f0;
  padding-bottom: 10px;
}

.markdown-content h2 {
  font-size: 24px;
}

.markdown-content h3 {
  font-size: 20px;
}

.markdown-content li {
  margin: 8px 0;
  list-style: disc;
  margin-left: 20px;
}

.markdown-content a {
  color: #007bff;
  text-decoration: underline;
}

.blog-actions {
  text-align: center;
  padding: 20px 0;
  border-top: 1px solid #f0f0f0;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 40px;
}

/* Comments Section */
.comments-section {
  margin-top: 40px;
}

.comments-title {
  color: #333;
  margin-bottom: 30px;
  font-size: 24px;
  font-weight: 600;
}

.add-comment-form {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 25px;
  margin-bottom: 30px;
}

.add-comment-form h3 {
  color: #333;
  margin: 0 0 20px 0;
  font-size: 18px;
}

.comment-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.comment-textarea {
  width: 100%;
  padding: 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  resize: vertical;
  transition: border-color 0.2s ease;
  box-sizing: border-box;
}

.comment-textarea:focus {
  outline: none;
  border-color: #007bff;
}

.comment-textarea.error {
  border-color: #dc3545;
}

.char-count {
  text-align: right;
  font-size: 12px;
  color: #888;
}

.error-message {
  color: #dc3545;
  font-size: 14px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
}

.auth-required-comment {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 20px;
  text-align: center;
  margin-bottom: 30px;
  color: #666;
}

.auth-required-comment a {
  color: #007bff;
  text-decoration: none;
  font-weight: 500;
}

.auth-required-comment a:hover {
  text-decoration: underline;
}

/* Comments List */
.loading-comments {
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: center;
  padding: 20px;
  color: #666;
}

.no-comments {
  text-align: center;
  padding: 40px;
  color: #666;
}

.no-comments-icon {
  font-size: 48px;
  margin-bottom: 15px;
}

.comment-item {
  background: #fafafa;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 15px;
  border-left: 4px solid #007bff;
}

.comment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  flex-wrap: wrap;
  gap: 10px;
}

.comment-author {
  display: flex;
  align-items: center;
  gap: 6px;
}

.author-icon {
  font-size: 14px;
}

.comment-dates {
  color: #666;
  font-size: 12px;
}

.comment-date {
  font-weight: 500;
}

.edited-date {
  font-style: italic;
}

.comment-content {
  color: #333;
  line-height: 1.6;
  font-size: 14px;
}

/* Button Styles */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
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

.btn-like {
  background: transparent;
  color: #dc3545;
  border: 1px solid #dc3545;
}

.btn-like:hover:not(:disabled) {
  background: #dc3545;
  color: white;
}

.btn-liked {
  background: #dc3545;
  color: white;
  border: 1px solid #dc3545;
}

.btn-liked:hover:not(:disabled) {
  background: #c82333;
  border-color: #c82333;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* Comment Editing Styles */
.edit-comment-form {
  margin: 15px 0;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.edit-comment-textarea {
  width: 100%;
  min-height: 80px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-family: inherit;
  font-size: 14px;
  resize: vertical;
  margin-bottom: 10px;
}

.edit-comment-textarea:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.1);
}

.edit-comment-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
}

.edit-btn,
.cancel-btn,
.save-btn {
  background: transparent;
  border: 1px solid #ddd;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s ease;
}

.edit-btn:hover {
  background: #f8f9fa;
  border-color: #007bff;
  color: #007bff;
}

.cancel-btn:hover {
  background: #f8f9fa;
  border-color: #6c757d;
  color: #6c757d;
}

.save-btn {
  background: #007bff;
  color: white;
  border-color: #007bff;
}

.save-btn:hover:not(:disabled) {
  background: #0056b3;
  border-color: #0056b3;
}

.save-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.edit-comment-error {
  color: #dc3545;
  font-size: 12px;
  margin-top: 5px;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Responsive Design */
@media (max-width: 768px) {
  .blog-detail-container {
    padding: 15px;
  }

  .blog-detail {
    padding: 20px;
  }

  .blog-title {
    font-size: 24px;
  }

  .blog-meta {
    flex-direction: column;
    gap: 10px;
  }

  .comment-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
