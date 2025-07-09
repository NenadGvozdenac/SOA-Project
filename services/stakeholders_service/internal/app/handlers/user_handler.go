package handlers

import (
	"net/http"
	"soa-project/stakeholders-service/internal/app/dtos"
	"soa-project/stakeholders-service/internal/app/repositories"
	"soa-project/stakeholders-service/internal/app/utils"
	"strconv"

	"github.com/gin-gonic/gin"
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
