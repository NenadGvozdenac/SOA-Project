package saga

import (
	"fmt"
	"soa-project/stakeholders-service/internal/app/utils/logger"
	"time"
)

// SagaStatus represents the current status of a saga
type SagaStatus string

const (
	SagaStatusPending     SagaStatus = "PENDING"
	SagaStatusInProgress  SagaStatus = "IN_PROGRESS"
	SagaStatusCompleted   SagaStatus = "COMPLETED"
	SagaStatusFailed      SagaStatus = "FAILED"
	SagaStatusCompensated SagaStatus = "COMPENSATED"
)

// SagaExecution represents a saga execution instance
type SagaExecution struct {
	ID          string
	Type        string
	Status      SagaStatus
	StartTime   time.Time
	EndTime     *time.Time
	Error       error
	CurrentStep int
	TotalSteps  int
}

// SagaManager manages saga executions and provides monitoring capabilities
type SagaManager struct {
	executions map[string]*SagaExecution
}

// NewSagaManager creates a new saga manager
func NewSagaManager() *SagaManager {
	return &SagaManager{
		executions: make(map[string]*SagaExecution),
	}
}

// StartSaga starts a new saga execution
func (sm *SagaManager) StartSaga(sagaID, sagaType string, totalSteps int) *SagaExecution {
	execution := &SagaExecution{
		ID:          sagaID,
		Type:        sagaType,
		Status:      SagaStatusInProgress,
		StartTime:   time.Now(),
		CurrentStep: 0,
		TotalSteps:  totalSteps,
	}

	sm.executions[sagaID] = execution
	logger.Info(fmt.Sprintf("Started saga execution: %s (type: %s)", sagaID, sagaType))

	return execution
}

// UpdateSagaProgress updates the progress of a saga execution
func (sm *SagaManager) UpdateSagaProgress(sagaID string, currentStep int) {
	if execution, exists := sm.executions[sagaID]; exists {
		execution.CurrentStep = currentStep
		logger.Info(fmt.Sprintf("Saga %s progress: step %d/%d", sagaID, currentStep, execution.TotalSteps))
	}
}

// CompleteSaga marks a saga as completed
func (sm *SagaManager) CompleteSaga(sagaID string) {
	if execution, exists := sm.executions[sagaID]; exists {
		execution.Status = SagaStatusCompleted
		now := time.Now()
		execution.EndTime = &now
		logger.Info(fmt.Sprintf("Saga %s completed successfully in %v", sagaID, now.Sub(execution.StartTime)))
	}
}

// FailSaga marks a saga as failed
func (sm *SagaManager) FailSaga(sagaID string, err error) {
	if execution, exists := sm.executions[sagaID]; exists {
		execution.Status = SagaStatusFailed
		execution.Error = err
		now := time.Now()
		execution.EndTime = &now
		logger.Error(fmt.Sprintf("Saga %s failed after %v: %v", sagaID, now.Sub(execution.StartTime), err))
	}
}

// CompensateSaga marks a saga as compensated
func (sm *SagaManager) CompensateSaga(sagaID string) {
	if execution, exists := sm.executions[sagaID]; exists {
		execution.Status = SagaStatusCompensated
		logger.Info(fmt.Sprintf("Saga %s compensated successfully", sagaID))
	}
}

// GetSagaStatus returns the status of a saga execution
func (sm *SagaManager) GetSagaStatus(sagaID string) (SagaStatus, bool) {
	if execution, exists := sm.executions[sagaID]; exists {
		return execution.Status, true
	}
	return SagaStatusPending, false
}

// GetAllSagas returns all saga executions
func (sm *SagaManager) GetAllSagas() map[string]*SagaExecution {
	return sm.executions
}

// CleanupCompletedSagas removes completed sagas older than the specified duration
func (sm *SagaManager) CleanupCompletedSagas(olderThan time.Duration) {
	cutoff := time.Now().Add(-olderThan)

	for id, execution := range sm.executions {
		if execution.EndTime != nil && execution.EndTime.Before(cutoff) {
			if execution.Status == SagaStatusCompleted || execution.Status == SagaStatusCompensated {
				delete(sm.executions, id)
				logger.Info(fmt.Sprintf("Cleaned up saga execution: %s", id))
			}
		}
	}
}

// Global saga manager instance
var DefaultSagaManager = NewSagaManager()
