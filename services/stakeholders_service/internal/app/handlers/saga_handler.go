package handlers

import (
	"net/http"
	"soa-project/stakeholders-service/internal/app/saga"
	"soa-project/stakeholders-service/internal/app/utils"

	"github.com/gin-gonic/gin"
)

// GetSagaStatus returns the status of a specific saga
func GetSagaStatus(c *gin.Context) {
	sagaID := c.Param("sagaId")
	if sagaID == "" {
		utils.CreateGinResponse(c, "Saga ID is required", http.StatusBadRequest, nil)
		return
	}

	status, exists := saga.DefaultSagaManager.GetSagaStatus(sagaID)
	if !exists {
		utils.CreateGinResponse(c, "Saga not found", http.StatusNotFound, nil)
		return
	}

	response := map[string]interface{}{
		"sagaId": sagaID,
		"status": status,
	}

	utils.CreateGinResponse(c, "Saga status retrieved successfully", http.StatusOK, response)
}

// GetAllSagas returns all saga executions
func GetAllSagas(c *gin.Context) {
	sagas := saga.DefaultSagaManager.GetAllSagas()

	response := make([]map[string]interface{}, 0, len(sagas))
	for id, execution := range sagas {
		sagaInfo := map[string]interface{}{
			"id":          id,
			"type":        execution.Type,
			"status":      execution.Status,
			"startTime":   execution.StartTime,
			"currentStep": execution.CurrentStep,
			"totalSteps":  execution.TotalSteps,
		}
		if execution.EndTime != nil {
			sagaInfo["endTime"] = execution.EndTime
		}
		if execution.Error != nil {
			sagaInfo["error"] = execution.Error.Error()
		}
		response = append(response, sagaInfo)
	}

	utils.CreateGinResponse(c, "All sagas retrieved successfully", http.StatusOK, response)
}
