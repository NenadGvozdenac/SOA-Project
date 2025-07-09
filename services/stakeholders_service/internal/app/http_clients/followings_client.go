package http_clients

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"soa-project/stakeholders-service/internal/app/utils/logger"
	"time"
)

type FollowingsServiceClient struct {
	baseURL string
	client  *http.Client
}

type CreateUserRequest struct {
	UserID string `json:"userId"`
}

func NewFollowingsServiceClient() *FollowingsServiceClient {
	baseURL := os.Getenv("FOLLOWINGS_SERVICE_URL")
	if baseURL == "" {
		baseURL = "http://localhost:9090" // Default localhost for easier testing
	}

	return &FollowingsServiceClient{
		baseURL: baseURL,
		client:  &http.Client{Timeout: 30 * time.Second},
	}
}

func (c *FollowingsServiceClient) CreateUser(userID string) error {
	logger.Info(fmt.Sprintf("Attempting to create user %s in followings service at %s", userID, c.baseURL))

	request := CreateUserRequest{
		UserID: userID,
	}

	jsonData, err := json.Marshal(request)
	if err != nil {
		return fmt.Errorf("failed to marshal request: %w", err)
	}

	maxRetries := 3
	for attempt := 1; attempt <= maxRetries; attempt++ {
		url := fmt.Sprintf("%s/api/users", c.baseURL)
		logger.Info(fmt.Sprintf("Attempt %d/%d: Sending POST request to %s", attempt, maxRetries, url))

		resp, err := c.client.Post(
			url,
			"application/json",
			bytes.NewBuffer(jsonData),
		)

		if err != nil {
			logger.Error(fmt.Sprintf("Attempt %d failed with error: %v", attempt, err))
			if attempt == maxRetries {
				return fmt.Errorf("failed to send request after %d attempts: %w", maxRetries, err)
			}
			// Wait before retrying
			logger.Info(fmt.Sprintf("Waiting %d seconds before retry...", attempt))
			time.Sleep(time.Duration(attempt) * time.Second)
			continue
		}

		resp.Body.Close()

		if resp.StatusCode == http.StatusOK || resp.StatusCode == http.StatusCreated {
			logger.Info(fmt.Sprintf("Successfully created user %s in followings service", userID))
			return nil
		}

		logger.Error(fmt.Sprintf("Attempt %d failed with status code: %d", attempt, resp.StatusCode))

		if attempt == maxRetries {
			return fmt.Errorf("failed to create user in followings service after %d attempts, status: %d", maxRetries, resp.StatusCode)
		}

		// Wait before retrying
		logger.Info(fmt.Sprintf("Waiting %d seconds before retry...", attempt))
		time.Sleep(time.Duration(attempt) * time.Second)
	}

	return fmt.Errorf("failed to create user in followings service after %d attempts", maxRetries)
}
