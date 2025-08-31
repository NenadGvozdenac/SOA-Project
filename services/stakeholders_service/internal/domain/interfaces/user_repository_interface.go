package interfaces

import "soa-project/stakeholders-service/internal/domain/models"

type UserRepositoryInterface interface {
	Create(user *models.User) error
	GetAll() (*[]models.User, error)
	GetByID(id uint) (*models.User, error)
	GetByEmail(email string) (*models.User, error)
	GetByIDs(ids []uint) (*[]models.User, error)
	DeleteByID(id uint) error
	UpdateByID(id uint, user *models.User) error
	UpdateBlockedStatus(id uint, blocked bool) error
}
