using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CheckCheckpointProximity;

public class CheckCheckpointProximityHandler(
    ICrudRepository<TourExecution> tourExecutionRepository,
    ICrudRepository<Checkpoint> checkpointRepository) 
    : IRequestHandler<CheckCheckpointProximityCommand, Result<CheckProximityResponseDTO>>
{
    private const double PROXIMITY_THRESHOLD_METERS = 50.0; // 50 meters proximity threshold

    public async Task<Result<CheckProximityResponseDTO>> Handle(CheckCheckpointProximityCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Tourist")
        {
            return Result<CheckProximityResponseDTO>.Failure("Only tourists can check checkpoint proximity.");
        }

        // Get tour execution
        var tourExecution = tourExecutionRepository.Get(request.CheckProximityRequestDTO.TourExecutionId);
        if (tourExecution == null)
        {
            return Result<CheckProximityResponseDTO>.Failure("Tour execution not found.");
        }

        // Verify that the tourist owns this execution
        var touristId = Convert.ToInt64(request.UserDTO.Id);
        if (tourExecution.TouristId != touristId)
        {
            return Result<CheckProximityResponseDTO>.Failure("You can only check proximity for your own tour executions.");
        }

        // Update tourist position
        tourExecution.UpdatePosition(
            request.CheckProximityRequestDTO.CurrentLatitude,
            request.CheckProximityRequestDTO.CurrentLongitude
        );
        tourExecutionRepository.Update(tourExecution);

        // Get all checkpoints for the tour
        var checkpoints = checkpointRepository.GetPaged(1, int.MaxValue).Results
            .Where(c => c.TourId == tourExecution.TourId)
            .ToList();

        // Check proximity to each checkpoint
        foreach (var checkpoint in checkpoints)
        {
            var distance = CalculateDistanceInMeters(
                request.CheckProximityRequestDTO.CurrentLatitude,
                request.CheckProximityRequestDTO.CurrentLongitude,
                checkpoint.Latitude,
                checkpoint.Longitude
            );

            if (distance <= PROXIMITY_THRESHOLD_METERS)
            {
                // Check if this checkpoint is already completed
                var isCompleted = tourExecution.CheckpointProgresses
                    .Any(cp => cp.CheckpointId == checkpoint.Id);

                // If not completed, mark as completed
                if (!isCompleted)
                {
                    tourExecution.CompleteCheckpoint(checkpoint.Id, DateTime.UtcNow);
                    tourExecutionRepository.Update(tourExecution);
                }

                return Result<CheckProximityResponseDTO>.Success(new CheckProximityResponseDTO
                {
                    IsNearCheckpoint = true,
                    CheckpointId = checkpoint.Id,
                    CheckpointName = checkpoint.Name,
                    DistanceMeters = distance,
                    CheckpointCompleted = !isCompleted, // true if just completed, false if was already completed
                    LastActivity = tourExecution.LastActivity
                });
            }
        }

        // No checkpoint nearby
        return Result<CheckProximityResponseDTO>.Success(new CheckProximityResponseDTO
        {
            IsNearCheckpoint = false,
            CheckpointId = null,
            CheckpointName = null,
            DistanceMeters = null,
            CheckpointCompleted = null,
            LastActivity = tourExecution.LastActivity
        });
    }

    private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth's radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
