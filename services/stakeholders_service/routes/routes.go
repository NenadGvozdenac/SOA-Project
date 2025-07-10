package routes

import (
	"soa-project/stakeholders-service/internal/app/handlers"
	"soa-project/stakeholders-service/internal/app/middleware"

	"github.com/gin-gonic/gin"
)

func SetupRoutes(router *gin.Engine) {
	// Define the main API group with the prefix "/api"
	api := router.Group("/api")

	// Setup public routes
	setupPublicRoutes(api)

	// Setup protected routes
	setupProtectedRoutes(api)
}

func setupPublicRoutes(api *gin.RouterGroup) {
	api.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "healthy"})
	})

	api.POST("/register", handlers.Register)
	api.POST("/login", handlers.Login)

	// Batch endpoint to get user details by IDs, used by other services
	api.POST("/users/batch", handlers.GetUsersByIds)
}

func setupProtectedRoutes(api *gin.RouterGroup) {
	protected := api.Group("/")
	protected.Use(middleware.AuthMiddleware())

	protected.GET("/users", handlers.GetAllUsers)
	// protected.GET("/users/:id", handlers.GetUserById)
	// protected.GET("/user", handlers.GetActiveUser)

	// Saga monitoring routes
	protected.GET("/sagas", handlers.GetAllSagas)
	protected.GET("/sagas/:sagaId", handlers.GetSagaStatus)
}
