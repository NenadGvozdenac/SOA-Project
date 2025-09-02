using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CompleteTourExecution;

public class CompleteTourExecutionHandler(
    ICrudRepository<TourExecution> tourExecutionRepository,
    ICrudRepository<Checkpoint> checkpointRepository) 
    : IRequestHandler<CompleteTourExecutionCommand, Result<CompleteTourExecutionDTO>>
{
    public async Task<Result<CompleteTourExecutionDTO>> Handle(CompleteTourExecutionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Tourist")
        {
            return Result<CompleteTourExecutionDTO>.Failure("Only tourists can complete/abandon tour execution.");
        }

        // Get tour execution
        var tourExecution = tourExecutionRepository.Get(request.CompleteTourRequestDTO.TourExecutionId);
        if (tourExecution == null)
        {
            return Result<CompleteTourExecutionDTO>.Failure("Tour execution not found.");
        }

        // Verify that the tourist owns this execution
        var touristId = Convert.ToInt64(request.UserDTO.Id);
        if (tourExecution.TouristId != touristId)
        {
            return Result<CompleteTourExecutionDTO>.Failure("You can only complete/abandon your own tour executions.");
        }

        // Check if execution is already completed/abandoned
        if (tourExecution.Status != TourExecutionStatus.Active)
        {
            return Result<CompleteTourExecutionDTO>.Failure("Tour execution is already completed or abandoned.");
        }

        // Complete or abandon the tour
        if (request.CompleteTourRequestDTO.IsAbandoned)
        {
            tourExecution.AbandonTour();
        }
        else
        {
            tourExecution.CompleteTour();
        }

        tourExecutionRepository.Update(tourExecution);

        // Get total checkpoints count for statistics
        var totalCheckpoints = checkpointRepository.GetPaged(1, int.MaxValue).Results
            .Count(c => c.TourId == tourExecution.TourId);

        var resultDto = new CompleteTourExecutionDTO
        {
            Id = tourExecution.Id,
            TourId = tourExecution.TourId,
            TouristId = tourExecution.TouristId,
            StartTime = tourExecution.StartTime,
            EndTime = tourExecution.EndTime ?? DateTime.UtcNow,
            Status = tourExecution.Status,
            CompletedCheckpoints = tourExecution.CheckpointProgresses.Count,
            TotalCheckpoints = totalCheckpoints
        };

        return Result<CompleteTourExecutionDTO>.Success(resultDto);
    }
}
