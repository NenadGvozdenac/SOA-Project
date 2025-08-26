package logger

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"time"
)

type LogLevel string

const (
	INFO  LogLevel = "Information"
	WARN  LogLevel = "Warning"
	ERROR LogLevel = "Error"
	FATAL LogLevel = "Fatal"
)

type LogEntry struct {
	Timestamp   string                 `json:"@timestamp"`
	Level       LogLevel               `json:"level"`
	Message     string                 `json:"message"`
	Service     string                 `json:"Service"`
	Environment string                 `json:"Environment"`
	Fields      map[string]interface{} `json:"fields,omitempty"`
}

var environment string = os.Getenv("GO_ENV")

func logMessage(level LogLevel, message string, fields ...map[string]interface{}) {
	if environment == "test" {
		return
	}

	entry := LogEntry{
		Timestamp:   time.Now().UTC().Format("2006-01-02T15:04:05.000Z"),
		Level:       level,
		Message:     message,
		Service:     "stakeholders-service",
		Environment: getEnvironment(),
	}

	if len(fields) > 0 {
		entry.Fields = fields[0]
	}

	// Log to console
	currentTime := time.Now().Format("2.1.2006 15:04:05")
	fmt.Printf("[%s] [%s] [stakeholders-service] %s\n", currentTime, level, message)

	// Send to Elasticsearch
	sendToElasticsearch(entry)
}

func getEnvironment() string {
	env := os.Getenv("ASPNETCORE_ENVIRONMENT")
	if env == "" {
		return "Production"
	}
	return env
}

func sendToElasticsearch(entry LogEntry) {
	jsonData, err := json.Marshal(entry)
	if err != nil {
		return // Silently fail if JSON marshaling fails
	}

	// Send to Elasticsearch
	go func() {
		client := &http.Client{Timeout: time.Second * 5}
		indexName := fmt.Sprintf("logstash-soa-project-%s", time.Now().Format("2006.01.02"))
		url := fmt.Sprintf("http://elasticsearch:9200/%s/_doc", indexName)

		req, err := http.NewRequest("POST", url, bytes.NewBuffer(jsonData))
		if err != nil {
			return
		}

		req.Header.Set("Content-Type", "application/json")
		client.Do(req) // Ignore response, fire and forget
	}()
}

func combineMessages(message []string) string {
	messageString := ""
	for _, m := range message {
		messageString += m + " "
	}
	return messageString
}

func Info(message ...string) {
	logMessage(INFO, combineMessages(message))
}

func Warn(message ...string) {
	logMessage(WARN, combineMessages(message))
}

func Error(message ...string) {
	logMessage(ERROR, combineMessages(message))
}

func Fatal(message ...string) {
	logMessage(FATAL, combineMessages(message))
}

func LogMessage(message string, code int) {
	if code >= 500 {
		Error(message)
	} else if code >= 400 {
		Warn(message)
	} else {
		Info(message)
	}
}

func InfoWithFields(message string, fields map[string]interface{}) {
	logMessage(INFO, message, fields)
}

func WarnWithFields(message string, fields map[string]interface{}) {
	logMessage(WARN, message, fields)
}

func ErrorWithFields(message string, fields map[string]interface{}) {
	logMessage(ERROR, message, fields)
}
