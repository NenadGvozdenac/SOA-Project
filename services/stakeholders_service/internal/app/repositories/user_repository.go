package repositories

import (
	"log"
	"soa-project/stakeholders-service/config"
	"soa-project/stakeholders-service/internal/domain/interfaces"
	"soa-project/stakeholders-service/internal/domain/models"
)

// UserRepository is the implementation of UserRepositoryInterface
type UserRepository struct{}

// NewUserRepository creates and returns a new UserRepository
func NewUserRepository() interfaces.UserRepositoryInterface {
	return &UserRepository{}
}

// Create creates a new user in the database
func (repo *UserRepository) Create(user *models.User) error {
	if err := config.DB.Create(user).Error; err != nil {
		return err
	}
	return nil
}

// GetByEmail retrieves a user by email
func (repo *UserRepository) GetByEmail(email string) (*models.User, error) {
	var user models.User
	if err := config.DB.Where("email = ?", email).First(&user).Error; err != nil {
		return nil, err
	}
	return &user, nil
}

// GetByID retrieves a user by ID
func (repo *UserRepository) GetByID(id uint) (*models.User, error) {
	var user models.User
	if err := config.DB.Where("id = ?", id).First(&user).Error; err != nil {
		return nil, err
	}
	return &user, nil
}

// GetAll retrieves all users from the database
func (repo *UserRepository) GetAll() (*[]models.User, error) {
	var users []models.User
	if err := config.DB.Find(&users).Error; err != nil {
		log.Printf("Error retrieving users: %v", err)
		return nil, err
	}
	return &users, nil
}

// DeleteByID deletes a user by ID
func (repo *UserRepository) DeleteByID(id uint) error {
	if err := config.DB.Delete(&models.User{}, id).Error; err != nil {
		return err
	}
	return nil
}

// GetByIDs retrieves multiple users by their IDs
func (repo *UserRepository) GetByIDs(ids []uint) (*[]models.User, error) {
	var users []models.User
	if err := config.DB.Where("id IN ?", ids).Find(&users).Error; err != nil {
		log.Printf("Error retrieving users by IDs: %v", err)
		return nil, err
	}
	return &users, nil
}

// UpdateByID updates a user's data by ID
func (repo *UserRepository) UpdateByID(id uint, user *models.User) error {
	if err := config.DB.Model(&models.User{}).Where("id = ?", id).Updates(user).Error; err != nil {
		return err
	}
	return nil
}

// UpdateBlockedStatus updates only the blocked status of a user
func (repo *UserRepository) UpdateBlockedStatus(id uint, blocked bool) error {
	result := config.DB.Model(&models.User{}).Where("id = ?", id).Update("blocked", blocked)
	if result.Error != nil {
		return result.Error
	}
	return nil
}
