package handlers

import (
	"elektrohelper/backend/internal/app/repositories"
	"elektrohelper/backend/internal/app/utils"
	"net/http"

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
