package httpclient

import (
	"bytes"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
)

// TracedHTTPClient wraps http.Client with tracing support
type TracedHTTPClient struct {
	client  *http.Client
	baseURL string
}

// NewTracedHTTPClient creates a new HTTP client with tracing
func NewTracedHTTPClient(baseURL string) *TracedHTTPClient {
	return &TracedHTTPClient{
		client: &http.Client{
			Timeout: time.Second * 30,
		},
		baseURL: baseURL,
	}
}

// Get makes a GET request with trace propagation
func (c *TracedHTTPClient) Get(ginCtx *gin.Context, path string) (*http.Response, error) {
	return c.makeRequest(ginCtx, "GET", path, nil)
}

// Post makes a POST request with trace propagation
func (c *TracedHTTPClient) Post(ginCtx *gin.Context, path string, body []byte) (*http.Response, error) {
	return c.makeRequest(ginCtx, "POST", path, body)
}

// Put makes a PUT request with trace propagation
func (c *TracedHTTPClient) Put(ginCtx *gin.Context, path string, body []byte) (*http.Response, error) {
	return c.makeRequest(ginCtx, "PUT", path, body)
}

// Delete makes a DELETE request with trace propagation
func (c *TracedHTTPClient) Delete(ginCtx *gin.Context, path string) (*http.Response, error) {
	return c.makeRequest(ginCtx, "DELETE", path, nil)
}

func (c *TracedHTTPClient) makeRequest(ginCtx *gin.Context, method, path string, body []byte) (*http.Response, error) {
	url := c.baseURL + path

	var bodyReader io.Reader
	if body != nil {
		bodyReader = bytes.NewReader(body)
	}

	req, err := http.NewRequest(method, url, bodyReader)
	if err != nil {
		return nil, err
	}

	// Propagate trace ID
	if traceID, exists := ginCtx.Get("traceID"); exists {
		req.Header.Set("X-Trace-ID", traceID.(string))
		fmt.Printf("[TRACE] %s - Outgoing %s request to %s\n", traceID, method, url)
	}

	// Set content type for POST/PUT requests
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}

	// Copy any authentication headers
	if auth := ginCtx.GetHeader("Authorization"); auth != "" {
		req.Header.Set("Authorization", auth)
	}

	resp, err := c.client.Do(req)
	if err != nil {
		if traceID, exists := ginCtx.Get("traceID"); exists {
			fmt.Printf("[TRACE] %s - Request to %s failed: %v\n", traceID, url, err)
		}
		return nil, err
	}

	if traceID, exists := ginCtx.Get("traceID"); exists {
		fmt.Printf("[TRACE] %s - Received response from %s: %d\n", traceID, url, resp.StatusCode)
	}

	return resp, nil
}
