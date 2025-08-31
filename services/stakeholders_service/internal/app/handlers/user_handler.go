package handlers

import (
	"encoding/base64"
	"fmt"
	"net/http"
	"soa-project/stakeholders-service/internal/app/dtos"
	"soa-project/stakeholders-service/internal/app/repositories"
	"soa-project/stakeholders-service/internal/app/utils"
	"soa-project/stakeholders-service/internal/domain/models"
	"strconv"

	"github.com/gin-gonic/gin"
	"golang.org/x/crypto/bcrypt"
)

func GetAllUsers(c *gin.Context) {
	_, _, userRole, _ := utils.ExtractDataFromContext(c)

	if userRole != "Admin" {
		utils.CreateGinResponse(c, "Unauthorized access", http.StatusForbidden, nil)
		return
	}

	// Get all users from the database
	users, err := repositories.NewUserRepository().GetAll()

	if err != nil {
		utils.CreateGinResponse(c, "Failed to retrieve users", http.StatusInternalServerError, nil)
		return
	}

	utils.CreateGinResponse(c, "Users retrieved successfully", http.StatusOK, users)
}

func GetUserById(c *gin.Context) {
	idStr := c.Param("id")
	id, err := strconv.ParseUint(idStr, 10, 32)
	if err != nil {
		utils.CreateGinResponse(c, "Invalid user ID", http.StatusBadRequest, nil)
		return
	}

	// Get user from the database
	user, err := repositories.NewUserRepository().GetByID(uint(id))
	if err != nil || user == nil {
		utils.CreateGinResponse(c, "User not found", http.StatusNotFound, nil)
		return
	}

	// Convert profile picture to base64 if exists
	var profilePictureBase64 string
	if len(user.ProfilePicture) > 0 {
		profilePictureBase64 = base64.StdEncoding.EncodeToString(user.ProfilePicture)
	}

	// Convert to DTO without password
	userDTO := map[string]interface{}{
		"id":             strconv.FormatUint(uint64(user.ID), 10),
		"username":       user.Username,
		"name":           user.Name,
		"surname":        user.Surname,
		"email":          user.Email,
		"biography":      user.Biography,
		"moto":           user.Moto,
		"profilePicture": profilePictureBase64,
	}

	utils.CreateGinResponse(c, "User retrieved successfully", http.StatusOK, userDTO)
}

func GetAllUsersPublic(c *gin.Context) {
	// Get all users from the database
	users, err := repositories.NewUserRepository().GetAll()

	if err != nil {
		utils.CreateGinResponse(c, "Failed to retrieve users", http.StatusInternalServerError, nil)
		return
	}

	// Convert to public DTOs with limited information
	var publicUsers []dtos.UserDetailsDTO
	for _, user := range *users {
		var profilePictureBase64 *string
		if len(user.ProfilePicture) > 0 {
			encoded := base64.StdEncoding.EncodeToString(user.ProfilePicture)
			profilePictureBase64 = &encoded
		}

		publicUser := dtos.UserDetailsDTO{
			Id:             strconv.FormatUint(uint64(user.ID), 10),
			Username:       user.Username,
			Name:           user.Name + " " + user.Surname,
			Email:          user.Email,
			ProfilePicture: profilePictureBase64,
		}
		publicUsers = append(publicUsers, publicUser)
	}

	utils.CreateGinResponse(c, "Users retrieved successfully", http.StatusOK, publicUsers)
}

// UpdateUser ažurira podatke korisnika po ID-u
func UpdateUser(c *gin.Context) {
	idStr := c.Param("id")
	id, err := strconv.ParseUint(idStr, 10, 32)
	if err != nil {
		utils.CreateGinResponse(c, "Invalid user ID", http.StatusBadRequest, nil)
		return
	}

	var userUpdate dtos.UserUpdateDTO
	if err := c.ShouldBindJSON(&userUpdate); err != nil {
		utils.CreateGinResponse(c, "Invalid request body", http.StatusBadRequest, nil)
		return
	}

	// Debug: Log profile picture data
	if userUpdate.ProfilePicture != "" {
		fmt.Printf("Received profile picture data length: %d\n", len(userUpdate.ProfilePicture))
		previewLen := 50
		if len(userUpdate.ProfilePicture) < previewLen {
			previewLen = len(userUpdate.ProfilePicture)
		}
		fmt.Printf("Profile picture preview: %s...\n", userUpdate.ProfilePicture[:previewLen])
	} else {
		fmt.Println("No profile picture data received")
	}

	// Dohvati korisnika iz baze
	existingUser, err := repositories.NewUserRepository().GetByID(uint(id))
	if err != nil || existingUser == nil {
		utils.CreateGinResponse(c, "User not found", http.StatusNotFound, nil)
		return
	}

	// Ako korisnik menja lozinku, proveri da li je stara lozinka tačna
	if userUpdate.NewPassword != "" {
		if err := bcrypt.CompareHashAndPassword([]byte(existingUser.Password), []byte(userUpdate.OldPassword)); err != nil {
			utils.CreateGinResponse(c, "Old password is incorrect", http.StatusBadRequest, nil)
			return
		}
		if userUpdate.NewPassword != userUpdate.ConfirmPassword {
			utils.CreateGinResponse(c, "New passwords do not match", http.StatusBadRequest, nil)
			return
		}
	}

	user := models.User{
		ID:        uint(id), // Add the ID for GORM update
		Name:      userUpdate.Name,
		Surname:   userUpdate.Surname,
		Email:     userUpdate.Email,
		Username:  userUpdate.Username,
		Biography: userUpdate.Biography,
		Moto:      userUpdate.Moto,
	}

	// Handle profile picture if provided
	if userUpdate.ProfilePicture != "" {
		// Decode base64 image
		profilePictureBytes, err := base64.StdEncoding.DecodeString(userUpdate.ProfilePicture)
		if err != nil {
			utils.CreateGinResponse(c, "Invalid profile picture format", http.StatusBadRequest, nil)
			return
		}
		user.ProfilePicture = profilePictureBytes
		fmt.Printf("DEBUG Handler: Successfully decoded profile picture, length: %d bytes\n", len(profilePictureBytes))
	} else {
		// Keep existing profile picture
		user.ProfilePicture = existingUser.ProfilePicture
		fmt.Printf("DEBUG Handler: Keeping existing profile picture, length: %d bytes\n", len(existingUser.ProfilePicture))
	}

	// Samo ako korisnik menja lozinku, ažuriraj password polje
	if userUpdate.NewPassword != "" {
		hashedPassword, err := bcrypt.GenerateFromPassword([]byte(userUpdate.NewPassword), bcrypt.DefaultCost)
		if err != nil {
			utils.CreateGinResponse(c, "Failed to hash password", http.StatusInternalServerError, nil)
			return
		}
		user.Password = string(hashedPassword)
	} else {
		// Zadrži postojeći password
		user.Password = existingUser.Password
	}

	err = repositories.NewUserRepository().UpdateByID(uint(id), &user)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to update user", http.StatusInternalServerError, nil)
		return
	}

	utils.CreateGinResponse(c, "User updated successfully", http.StatusOK, nil)
}

func GetUsersByIds(c *gin.Context) {
	var request dtos.BatchUserRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		utils.CreateGinResponse(c, "Invalid request body", http.StatusBadRequest, nil)
		return
	}

	// Convert string IDs to uint
	var userIds []uint
	for _, idStr := range request.UserIds {
		id, err := strconv.ParseUint(idStr, 10, 32)
		if err != nil {
			utils.CreateGinResponse(c, "Invalid user ID format", http.StatusBadRequest, nil)
			return
		}
		userIds = append(userIds, uint(id))
	}

	// Get users from the database
	users, err := repositories.NewUserRepository().GetByIDs(userIds)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to retrieve users", http.StatusInternalServerError, nil)
		return
	}

	// Convert to DTOs
	var userDetails []dtos.UserDetailsDTO
	for _, user := range *users {
		userDetail := dtos.UserDetailsDTO{
			Id:             strconv.FormatUint(uint64(user.ID), 10),
			Username:       user.Email, // Assuming email is used as username
			Name:           user.Name + " " + user.Surname,
			Email:          user.Email,
			ProfilePicture: nil, // Assuming no profile picture field in current model
		}
		userDetails = append(userDetails, userDetail)
	}

	utils.CreateGinResponse(c, "Users retrieved successfully", http.StatusOK, userDetails)
}

// BlockUser blocks or unblocks a user (admin only)
func BlockUser(c *gin.Context) {
	_, _, userRole, _ := utils.ExtractDataFromContext(c)

	if userRole != "Admin" {
		utils.CreateGinResponse(c, "Unauthorized access", http.StatusForbidden, nil)
		return
	}

	idStr := c.Param("id")
	id, err := strconv.ParseUint(idStr, 10, 32)
	if err != nil {
		utils.CreateGinResponse(c, "Invalid user ID", http.StatusBadRequest, nil)
		return
	}

	var blockRequest struct {
		Blocked bool `json:"blocked"`
	}

	if err := c.ShouldBindJSON(&blockRequest); err != nil {
		utils.CreateGinResponse(c, "Invalid request body", http.StatusBadRequest, nil)
		return
	}

	// Get user from the database
	user, err := repositories.NewUserRepository().GetByID(uint(id))
	if err != nil || user == nil {
		utils.CreateGinResponse(c, "User not found", http.StatusNotFound, nil)
		return
	}

	// Don't allow blocking admin users
	if user.RoleId == 0 { // Assuming 0 is admin role
		utils.CreateGinResponse(c, "Cannot block admin users", http.StatusBadRequest, nil)
		return
	}

	// Update the blocked status
	err = repositories.NewUserRepository().UpdateBlockedStatus(uint(id), blockRequest.Blocked)
	if err != nil {
		utils.CreateGinResponse(c, "Failed to update user block status", http.StatusInternalServerError, nil)
		return
	}

	action := "blocked"
	if !blockRequest.Blocked {
		action = "unblocked"
	}

	utils.CreateGinResponse(c, "User "+action+" successfully", http.StatusOK, nil)
}
