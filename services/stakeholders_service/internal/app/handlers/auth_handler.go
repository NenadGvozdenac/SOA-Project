package handlers

import (
	"net/http"
	"soa-project/stakeholders-service/config"
	"soa-project/stakeholders-service/internal/app/dtos"
	"soa-project/stakeholders-service/internal/app/saga"
	"soa-project/stakeholders-service/internal/app/utils"
	"soa-project/stakeholders-service/internal/app/utils/logger"
	"soa-project/stakeholders-service/internal/domain/models"

	"github.com/gin-gonic/gin"
	"golang.org/x/crypto/bcrypt"
)

// Register a new user and return a JWT
func Register(c *gin.Context) {
	var user dtos.RegisterDTO
	if err := c.ShouldBindJSON(&user); err != nil {
		utils.CreateGinResponse(c, "Invalid input", http.StatusBadRequest, nil)
		return
	}

	// Create and execute the user registration saga
	userSaga := saga.NewUserRegistrationSaga(user)
	newUser, err := userSaga.Execute()
	if err != nil {
		logger.Error("User registration saga failed: " + err.Error())
		utils.CreateGinResponse(c, "Failed to register user: "+err.Error(), http.StatusInternalServerError, nil)
		return
	}

	role := models.Role{}

	// Find the role by ID
	if err := config.DB.First(&role, user.RoleId).Error; err != nil {
		utils.CreateGinResponse(c, "Role not found", http.StatusBadRequest, nil)
		return
	}

	// Generate access and refresh tokens
	token, err := utils.GenerateToken(newUser.ID, newUser.Email, role.Name, newUser.Name)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to generate token", http.StatusInternalServerError, nil)
		return
	}

	tokenDTO := dtos.TokenDTO{
		Token: token,
	}

	utils.CreateGinResponse(c, "User registered successfully", http.StatusCreated, tokenDTO)
}

// Login a user and return a JWT
func Login(c *gin.Context) {
	var input dtos.LoginDTO

	if err := c.ShouldBindJSON(&input); err != nil {
		utils.CreateGinResponse(c, "Invalid input", http.StatusBadRequest, nil)
		return
	}

	// Find the user
	var user models.User
	if err := config.DB.Where("email = ?", input.Email).First(&user).Error; err != nil {
		utils.CreateGinResponse(c, "Invalid email or password", http.StatusUnauthorized, nil)
		return
	}

	// Compare passwords
	if err := bcrypt.CompareHashAndPassword([]byte(user.Password), []byte(input.Password)); err != nil {
		utils.CreateGinResponse(c, "Invalid email or password", http.StatusUnauthorized, nil)
		return
	}

	role := models.Role{}
	// Find the role by ID
	if err := config.DB.First(&role, user.RoleId).Error; err != nil {
		utils.CreateGinResponse(c, "Role not found", http.StatusBadRequest, nil)
		return
	}

	// Generate access and refresh tokens
	token, err := utils.GenerateToken(user.ID, user.Email, role.Name, user.Name)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to generate token", http.StatusInternalServerError, nil)
		return
	}

	tokenDTO := dtos.TokenDTO{
		Token: token,
	}

	utils.CreateGinResponse(c, "User logged in successfully", http.StatusOK, tokenDTO)
}
