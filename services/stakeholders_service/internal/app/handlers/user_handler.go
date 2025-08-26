package handlers

import (
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

	// Convert to DTO without password
	userDTO := map[string]interface{}{
		"id":        strconv.FormatUint(uint64(user.ID), 10),
		"username":  user.Username,
		"name":      user.Name,
		"surname":   user.Surname,
		"email":     user.Email,
		"biography": user.Biography,
		"moto":      user.Moto,
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
		publicUser := dtos.UserDetailsDTO{
			Id:             strconv.FormatUint(uint64(user.ID), 10),
			Username:       user.Username,
			Name:           user.Name + " " + user.Surname,
			Email:          user.Email,
			ProfilePicture: nil, // Assuming no profile picture field in current model
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
		Name:      userUpdate.Name,
		Surname:   userUpdate.Surname,
		Email:     userUpdate.Email,
		Username:  userUpdate.Username,
		Biography: userUpdate.Biography,
		Moto:      userUpdate.Moto,
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
