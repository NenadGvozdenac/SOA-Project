package handlers

import (
	"net/http"
	"soa-project/stakeholders-service/config"
	"soa-project/stakeholders-service/internal/app/dtos"
	"soa-project/stakeholders-service/internal/app/http_clients"
	"soa-project/stakeholders-service/internal/app/utils"
	"soa-project/stakeholders-service/internal/app/utils/logger"
	"soa-project/stakeholders-service/internal/domain/models"
	"strconv"

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

	// Check if the email is already in use
	var existingUser models.User
	if err := config.DB.Where("email = ?", user.Email).First(&existingUser).Error; err == nil {
		utils.CreateGinResponse(c, "Email already in use", http.StatusBadRequest, nil)
		return
	}

	// Hash the password
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(user.Password), bcrypt.DefaultCost)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to hash password", http.StatusInternalServerError, nil)
		return
	}

	newUser := models.User{
		Name:      user.Name,
		Surname:   user.Surname,
		Email:     user.Email,
		Password:  string(hashedPassword),
		Username:  user.Username,
		Biography: "",
		Moto:      "",
		RoleId:    user.RoleId,
		Blocked:   false,
	}

	if err := config.DB.Create(&newUser).Error; err != nil {
		utils.CreateGinResponse(c, "Failed to create user", http.StatusInternalServerError, nil)
		return
	}

	// Create user in followings service
	followingsClient := http_clients.NewFollowingsServiceClient()
	userIdStr := strconv.FormatUint(uint64(newUser.ID), 10)
	logger.Info("Creating user " + userIdStr + " in followings service")
	if err := followingsClient.CreateUser(userIdStr); err != nil {
		logger.Error("Failed to create user in followings service: " + err.Error())
		// Don't fail the registration, just log the error
	} else {
		logger.Info("Successfully created user " + userIdStr + " in followings service")
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
