<template>
  <div>
    <Navbar />
    <div class="profile-container">
      <h1>My Profile</h1>
      
      <div v-if="loading" class="loading">
        Updating profile...
      </div>

      <div v-if="error" class="error">
        {{ error }}
      </div>

      <div v-if="success" class="success">
        {{ success }}
      </div>

      <form @submit.prevent="updateProfile" class="profile-form">
        <div class="form-section">
          <h2>Personal Information</h2>
          
          <div class="form-group profile-picture-section">
            <label>Profile Picture</label>
            <div class="profile-picture-container">
              <div class="profile-picture-preview">
                <img 
                  v-if="profilePictureUrl" 
                  :src="profilePictureUrl" 
                  alt="Profile Picture" 
                  class="profile-image"
                />
                <div v-else class="profile-placeholder">
                  <i class="fas fa-user"></i>
                </div>
              </div>
              <div class="profile-picture-actions">
                <input 
                  type="file" 
                  ref="fileInput" 
                  @change="handleFileSelect" 
                  accept="image/*" 
                  class="file-input"
                  id="profilePicture"
                />
                <label for="profilePicture" class="btn btn-secondary">
                  Choose Picture
                </label>
                <button 
                  v-if="profilePictureUrl" 
                  type="button" 
                  @click="removeProfilePicture" 
                  class="btn btn-danger"
                >
                  Remove
                </button>
              </div>
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label for="name">First Name</label>
              <input 
                type="text" 
                id="name" 
                v-model="profileData.name" 
                required 
                placeholder="Enter your first name"
              />
            </div>
            
            <div class="form-group">
              <label for="surname">Last Name</label>
              <input 
                type="text" 
                id="surname" 
                v-model="profileData.surname" 
                required 
                placeholder="Enter your last name"
              />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="email">Email</label>
              <input 
                type="email" 
                id="email" 
                v-model="profileData.email" 
                required 
                placeholder="Enter your email"
              />
            </div>
            
            <div class="form-group">
              <label for="username">Username</label>
              <input 
                type="text" 
                id="username" 
                v-model="profileData.username" 
                required 
                placeholder="Enter your username"
              />
            </div>
          </div>

          <div class="form-group">
            <label for="biography">Biography</label>
            <textarea 
              id="biography" 
              v-model="profileData.biography" 
              placeholder="Tell us about yourself..."
              rows="4"
            ></textarea>
          </div>

          <div class="form-group">
            <label for="motto">Motto</label>
            <input 
              type="text" 
              id="motto" 
              v-model="profileData.motto" 
              placeholder="Your personal motto or quote"
            />
          </div>
        </div>

        <div class="form-section">
          <h2>Change Password</h2>
          <p class="password-info">Leave password fields empty if you don't want to change your password</p>
          
          <div class="form-group">
            <label for="oldPassword">Current Password</label>
            <input 
              type="password" 
              id="oldPassword" 
              v-model="profileData.oldPassword" 
              placeholder="Enter your current password"
            />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="newPassword">New Password</label>
              <input 
                type="password" 
                id="newPassword" 
                v-model="profileData.newPassword" 
                placeholder="Enter new password"
              />
            </div>
            
            <div class="form-group">
              <label for="confirmPassword">Confirm New Password</label>
              <input 
                type="password" 
                id="confirmPassword" 
                v-model="profileData.confirmPassword" 
                placeholder="Confirm new password"
              />
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="submit" :disabled="loading" class="btn btn-primary">
            {{ loading ? 'Updating...' : 'Update Profile' }}
          </button>
          <button type="button" @click="cancelEdit" class="btn btn-secondary">
            Cancel
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import Navbar from './Navbar.vue';
import { StakeholdersService } from '../services/stakeholders_service.js';

export default {
  name: 'Profile',
  components: {
    Navbar
  },
  data() {
    return {
      loading: false,
      error: null,
      success: null,
      profilePictureUrl: null,
      profilePictureFile: null,
      profileData: {
        name: '',
        surname: '',
        email: '',
        username: '',
        biography: '',
        motto: '',
        profilePicture: '',
        oldPassword: '',
        newPassword: '',
        confirmPassword: ''
      }
    };
  },
  async mounted() {
    await this.loadCurrentUserData();
  },
  methods: {
    async loadCurrentUserData() {
      try {
        const userId = localStorage.getItem('userID');
        if (!userId) {
          this.error = 'User not logged in';
          return;
        }

        // Try to get current user data from API
        try {
          const response = await StakeholdersService.getUserById(userId);
          const userData = response.data;
          
          if (userData) {
            this.profileData.name = userData.name || '';
            this.profileData.surname = userData.surname || '';
            this.profileData.email = userData.email || '';
            this.profileData.username = userData.username || userData.email || '';
            this.profileData.biography = userData.biography || '';
            this.profileData.motto = userData.moto || '';  // Note: backend returns 'moto' not 'motto'
            
            // Handle profile picture
            if (userData.profilePicture) {
              this.profilePictureUrl = `data:image/jpeg;base64,${userData.profilePicture}`;
            }
          }
        } catch (apiError) {
          console.warn('Could not fetch user data from API, using localStorage:', apiError);
          
          // Fallback to localStorage if API fails
          const userName = localStorage.getItem('userName');
          const userEmail = localStorage.getItem('userEmail');
          
          if (userName) {
            const nameParts = userName.split(' ');
            this.profileData.name = nameParts[0] || '';
            this.profileData.surname = nameParts.slice(1).join(' ') || '';
          }
          
          if (userEmail) {
            this.profileData.email = userEmail;
            this.profileData.username = userEmail;
          }
        }
        
      } catch (error) {
        console.error('Error loading user data:', error);
        this.error = 'Failed to load user data';
      }
    },

    async updateProfile() {
      this.loading = true;
      this.error = null;
      this.success = null;

      try {
        // Validation
        if (this.profileData.newPassword && !this.profileData.oldPassword) {
          this.error = 'Current password is required to set new password';
          return;
        }

        if (this.profileData.newPassword && this.profileData.newPassword !== this.profileData.confirmPassword) {
          this.error = 'New passwords do not match';
          return;
        }

        const userId = localStorage.getItem('userID');
        if (!userId) {
          this.error = 'User not logged in';
          return;
        }

        // Prepare request data - only include password fields if changing password
        const requestData = {
          name: this.profileData.name,
          surname: this.profileData.surname,
          email: this.profileData.email,
          username: this.profileData.username,
          biography: this.profileData.biography,
          motto: this.profileData.motto,  // Keep as motto for consistency
          profilePicture: ''
        };

        // Handle profile picture if a new file was selected
        if (this.profilePictureFile) {
          try {
            const base64Data = await this.convertFileToBase64(this.profilePictureFile);
            requestData.profilePicture = base64Data;
            console.log('Profile picture data length:', base64Data.length);
            console.log('Profile picture preview:', base64Data.substring(0, 50) + '...');
            console.log('Sending profile picture with request');
          } catch (error) {
            console.error('Error converting file to base64:', error);
            this.error = 'Failed to process image file';
            return;
          }
        } else {
          console.log('No new profile picture file selected, not sending profile picture data');
        }

        console.log('Sending request data:', {
          ...requestData,
          profilePicture: requestData.profilePicture ? `${requestData.profilePicture.substring(0, 50)}... (${requestData.profilePicture.length} chars)` : 'empty'
        });

        // Only add password fields if user wants to change password
        if (this.profileData.newPassword) {
          requestData.oldPassword = this.profileData.oldPassword;
          requestData.newPassword = this.profileData.newPassword;
          requestData.confirmPassword = this.profileData.confirmPassword;
        } else {
          // Send empty strings for password fields when not changing password
          requestData.oldPassword = '';
          requestData.newPassword = '';
          requestData.confirmPassword = '';
        }

        await StakeholdersService.updateUser(userId, requestData);
        
        this.success = 'Profile updated successfully!';
        
        // Update localStorage with new user data
        localStorage.setItem('userName', `${this.profileData.name} ${this.profileData.surname}`);
        localStorage.setItem('userEmail', this.profileData.email);
        
        // Clear password fields
        this.profileData.oldPassword = '';
        this.profileData.newPassword = '';
        this.profileData.confirmPassword = '';
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.success = null;
        }, 3000);

      } catch (error) {
        console.error('Error updating profile:', error);
        this.error = error.response?.data?.message || error.message || 'Failed to update profile';
      } finally {
        this.loading = false;
      }
    },

    cancelEdit() {
      this.$router.push('/');
    },

    handleFileSelect(event) {
      const file = event.target.files[0];
      if (!file) return;

      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.error = 'Please select a valid image file';
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.error = 'Image file size must be less than 5MB';
        return;
      }

      this.profilePictureFile = file;

      // Create preview URL
      const reader = new FileReader();
      reader.onload = (e) => {
        this.profilePictureUrl = e.target.result;
      };
      reader.readAsDataURL(file);

      // Clear any previous errors
      this.error = null;
    },

    removeProfilePicture() {
      this.profilePictureUrl = null;
      this.profilePictureFile = null;
      this.profileData.profilePicture = '';
      
      // Clear the file input
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = '';
      }
    },

    async convertFileToBase64(file) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
          // Remove the data URL prefix (data:image/jpeg;base64,)
          const base64 = reader.result.split(',')[1];
          resolve(base64);
        };
        reader.onerror = reject;
        reader.readAsDataURL(file);
      });
    }
  }
};
</script>

<style scoped>
.profile-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 80px 20px 20px 20px;
}

h1 {
  text-align: center;
  color: #333;
  margin-bottom: 30px;
  font-size: 2.5rem;
  font-weight: 700;
}

.loading {
  text-align: center;
  padding: 20px;
  font-size: 18px;
  color: #666;
  background-color: #f8f9fa;
  border-radius: 8px;
  margin-bottom: 20px;
}

.error {
  text-align: center;
  padding: 15px;
  color: #dc3545;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 8px;
  margin-bottom: 20px;
}

.success {
  text-align: center;
  padding: 15px;
  color: #155724;
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  border-radius: 8px;
  margin-bottom: 20px;
}

.profile-form {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.form-section {
  padding: 30px;
  border-bottom: 1px solid #e9ecef;
}

.form-section:last-child {
  border-bottom: none;
}

.form-section h2 {
  color: #2c3e50;
  margin-bottom: 20px;
  font-size: 1.5rem;
  font-weight: 600;
  border-bottom: 2px solid #3498db;
  padding-bottom: 10px;
}

.password-info {
  color: #6c757d;
  font-size: 0.9rem;
  margin-bottom: 20px;
  font-style: italic;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #374151;
  font-weight: 600;
  font-size: 0.95rem;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 16px;
  transition: all 0.3s ease;
  background-color: #f9fafb;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #3498db;
  background-color: white;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.form-group textarea {
  resize: vertical;
  min-height: 100px;
}

/* Profile Picture Styles */
.profile-picture-section {
  margin-bottom: 30px;
}

.profile-picture-container {
  display: flex;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}

.profile-picture-preview {
  position: relative;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  overflow: hidden;
  border: 3px solid #e5e7eb;
  background-color: #f9fafb;
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-placeholder {
  color: #9ca3af;
  font-size: 3rem;
}

.profile-picture-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
  flex: 1;
  min-width: 200px;
}

.file-input {
  display: none;
}

.btn-danger {
  background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
  color: white;
  box-shadow: 0 2px 4px rgba(220, 53, 69, 0.3);
}

.btn-danger:hover {
  background: linear-gradient(135deg, #c82333 0%, #bd2130 100%);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.4);
}

.form-actions {
  padding: 30px;
  display: flex;
  gap: 15px;
  justify-content: center;
  background-color: #f8f9fa;
}

.btn {
  padding: 12px 30px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  display: inline-block;
  text-align: center;
  min-width: 140px;
}

.btn-primary {
  background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
  color: white;
  box-shadow: 0 2px 4px rgba(52, 152, 219, 0.3);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(52, 152, 219, 0.4);
}

.btn-primary:disabled {
  background: #95a5a6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.btn-secondary {
  background: #6c757d;
  color: white;
  box-shadow: 0 2px 4px rgba(108, 117, 125, 0.3);
}

.btn-secondary:hover {
  background: #5a6268;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(108, 117, 125, 0.4);
}

@media (max-width: 768px) {
  .profile-container {
    padding: 60px 15px 15px 15px;
  }
  
  .form-row {
    grid-template-columns: 1fr;
    gap: 15px;
  }
  
  .form-section {
    padding: 20px;
  }
  
  .form-actions {
    padding: 20px;
    flex-direction: column;
  }
  
  .btn {
    width: 100%;
  }
  
  h1 {
    font-size: 2rem;
  }
}
</style>
