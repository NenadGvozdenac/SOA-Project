package saga

import (
	"fmt"
	"soa-project/stakeholders-service/config"
	"soa-project/stakeholders-service/internal/app/dtos"
	"soa-project/stakeholders-service/internal/app/http_clients"
	"soa-project/stakeholders-service/internal/app/utils/logger"
	"soa-project/stakeholders-service/internal/domain/models"
	"strconv"
	"time"

	"golang.org/x/crypto/bcrypt"
)

// SagaStep represents a step in the saga with its compensation
type SagaStep struct {
	Name        string
	Execute     func() error
	Compensate  func() error
	Executed    bool
	Compensated bool
}

// UserRegistrationSaga handles the distributed transaction for user registration
type UserRegistrationSaga struct {
	sagaID           string
	steps            []SagaStep
	user             *models.User
	followingsClient *http_clients.FollowingsServiceClient
	registerDTO      dtos.RegisterDTO
	executedSteps    int
	manager          *SagaManager
}

// NewUserRegistrationSaga creates a new user registration saga
func NewUserRegistrationSaga(registerDTO dtos.RegisterDTO) *UserRegistrationSaga {
	sagaID := fmt.Sprintf("user_registration_%s_%d", registerDTO.Email, time.Now().UnixNano())

	saga := &UserRegistrationSaga{
		sagaID:           sagaID,
		registerDTO:      registerDTO,
		followingsClient: http_clients.NewFollowingsServiceClient(),
		user:             &models.User{},
		executedSteps:    0,
		manager:          DefaultSagaManager,
	}

	saga.initializeSteps()
	return saga
}

// initializeSteps sets up all the saga steps with their compensation logic
func (s *UserRegistrationSaga) initializeSteps() {
	s.steps = []SagaStep{
		{
			Name:    "ValidateUserEmail",
			Execute: s.validateUserEmail,
			Compensate: func() error {
				// No compensation needed for validation
				return nil
			},
		},
		{
			Name:       "CreateUserInDatabase",
			Execute:    s.createUserInDatabase,
			Compensate: s.deleteUserFromDatabase,
		},
		{
			Name:       "CreateUserInFollowingsService",
			Execute:    s.createUserInFollowingsService,
			Compensate: s.deleteUserFromFollowingsService,
		},
	}
}

// Execute runs the saga, executing all steps and handling compensation on failure
func (s *UserRegistrationSaga) Execute() (*models.User, error) {
	// Start saga tracking
	s.manager.StartSaga(s.sagaID, "UserRegistration", len(s.steps))

	logger.Info(fmt.Sprintf("Starting user registration saga with ID: %s", s.sagaID))

	for i, step := range s.steps {
		logger.Info(fmt.Sprintf("Executing step %d: %s", i+1, step.Name))
		s.manager.UpdateSagaProgress(s.sagaID, i+1)

		if err := step.Execute(); err != nil {
			logger.Error(fmt.Sprintf("Step %s failed: %v", step.Name, err))

			// Mark this step as executed (even though it failed) for compensation tracking
			s.steps[i].Executed = true
			s.executedSteps = i + 1

			// Mark saga as failed
			s.manager.FailSaga(s.sagaID, err)

			// Compensate all executed steps
			if compensationErr := s.compensate(); compensationErr != nil {
				logger.Error(fmt.Sprintf("Compensation failed: %v", compensationErr))
				return nil, fmt.Errorf("saga execution failed: %v, compensation also failed: %v", err, compensationErr)
			}

			// Mark saga as compensated
			s.manager.CompensateSaga(s.sagaID)
			return nil, fmt.Errorf("saga execution failed at step %s: %v", step.Name, err)
		}

		s.steps[i].Executed = true
		s.executedSteps = i + 1
		logger.Info(fmt.Sprintf("Step %s completed successfully", step.Name))
	}

	// Mark saga as completed
	s.manager.CompleteSaga(s.sagaID)
	logger.Info(fmt.Sprintf("User registration saga %s completed successfully", s.sagaID))
	return s.user, nil
}

// compensate runs compensation for all executed steps in reverse order
func (s *UserRegistrationSaga) compensate() error {
	logger.Info("Starting saga compensation")

	for i := s.executedSteps - 1; i >= 0; i-- {
		step := &s.steps[i]
		if step.Executed && !step.Compensated {
			logger.Info(fmt.Sprintf("Compensating step: %s", step.Name))

			if err := step.Compensate(); err != nil {
				logger.Error(fmt.Sprintf("Compensation failed for step %s: %v", step.Name, err))
				return err
			}

			step.Compensated = true
			logger.Info(fmt.Sprintf("Step %s compensated successfully", step.Name))
		}
	}

	logger.Info("Saga compensation completed")
	return nil
}

// Step implementations

func (s *UserRegistrationSaga) validateUserEmail() error {
	var existingUser models.User
	if err := config.DB.Where("email = ?", s.registerDTO.Email).First(&existingUser).Error; err == nil {
		return fmt.Errorf("email already in use")
	}
	return nil
}

func (s *UserRegistrationSaga) createUserInDatabase() error {
	// Hash the password
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(s.registerDTO.Password), bcrypt.DefaultCost)
	if err != nil {
		return fmt.Errorf("failed to hash password: %v", err)
	}

	*s.user = models.User{
		Name:      s.registerDTO.Name,
		Surname:   s.registerDTO.Surname,
		Email:     s.registerDTO.Email,
		Password:  string(hashedPassword),
		Username:  s.registerDTO.Username,
		Biography: "",
		Moto:      "",
		RoleId:    s.registerDTO.RoleId,
		Blocked:   false,
	}

	if err := config.DB.Create(s.user).Error; err != nil {
		return fmt.Errorf("failed to create user in database: %v", err)
	}

	logger.Info(fmt.Sprintf("User created in database with ID: %d", s.user.ID))
	return nil
}

func (s *UserRegistrationSaga) deleteUserFromDatabase() error {
	if s.user.ID == 0 {
		// User was never created
		return nil
	}

	if err := config.DB.Delete(s.user).Error; err != nil {
		return fmt.Errorf("failed to delete user from database: %v", err)
	}

	logger.Info(fmt.Sprintf("User %d deleted from database during compensation", s.user.ID))
	return nil
}

func (s *UserRegistrationSaga) createUserInFollowingsService() error {
	if s.user.ID == 0 {
		return fmt.Errorf("user ID is not available")
	}

	userIdStr := strconv.FormatUint(uint64(s.user.ID), 10)
	logger.Info(fmt.Sprintf("Creating user %s in followings service", userIdStr))

	if err := s.followingsClient.CreateUser(userIdStr); err != nil {
		return fmt.Errorf("failed to create user in followings service: %v", err)
	}

	logger.Info(fmt.Sprintf("Successfully created user %s in followings service", userIdStr))
	return nil
}

func (s *UserRegistrationSaga) deleteUserFromFollowingsService() error {
	if s.user.ID == 0 {
		// User was never created
		return nil
	}

	userIdStr := strconv.FormatUint(uint64(s.user.ID), 10)
	logger.Info(fmt.Sprintf("Deleting user %s from followings service during compensation", userIdStr))

	// Note: You'll need to implement DeleteUser method in FollowingsServiceClient
	if err := s.followingsClient.DeleteUser(userIdStr); err != nil {
		// Log error but don't fail compensation - the followings service might not have the delete endpoint
		logger.Error(fmt.Sprintf("Failed to delete user from followings service (this might be expected): %v", err))
		// Return nil to continue with other compensations
		return nil
	}

	logger.Info(fmt.Sprintf("Successfully deleted user %s from followings service", userIdStr))
	return nil
}
