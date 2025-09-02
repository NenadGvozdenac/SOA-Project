using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.StartTourExecution;

public class StartTourExecutionHandler(
    ICrudRepository<TourExecution> tourExecutionRepository,
    ICrudRepository<Tour> tourRepository,
    ICrudRepository<TourPurchaseToken> tokenRepository) 
    : IRequestHandler<StartTourExecutionCommand, Result<StartTourExecutionDTO>>
{
    public async Task<Result<StartTourExecutionDTO>> Handle(StartTourExecutionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Tourist")
        {
            return Result<StartTourExecutionDTO>.Failure("Only tourists can start tour execution.");
        }

        // Check if tour exists
        var tour = tourRepository.Get(request.StartTourRequestDTO.TourId);
        if (tour == null)
        {
            return Result<StartTourExecutionDTO>.Failure("Tour not found.");
        }

        // Check if tour is published or archived (can be started)
        if (tour.Status != TourStatus.Published && tour.Status != TourStatus.Archived)
        {
            return Result<StartTourExecutionDTO>.Failure("Tour must be published or archived to be started.");
        }

        // Check if tour is purchased (when purchase system is implemented)
        var touristId = Convert.ToInt64(request.UserDTO.Id);
        var hasPurchased = tokenRepository.GetPaged(1, int.MaxValue).Results
            .Any(t => t.TourId == request.StartTourRequestDTO.TourId && t.UserId == touristId);

        if (!hasPurchased && tour.Price > 0)
        {
            return Result<StartTourExecutionDTO>.Failure("Tour must be purchased before starting.");
        }

        // Check if there's already an active execution for this tourist and tour
        var existingExecution = tourExecutionRepository.GetPaged(1, int.MaxValue).Results
            .FirstOrDefault(te => te.TourId == request.StartTourRequestDTO.TourId && 
                                 te.TouristId == touristId && 
                                 te.Status == TourExecutionStatus.Active);

        if (existingExecution != null)
        {
            return Result<StartTourExecutionDTO>.Failure("There is already an active execution for this tour.");
        }

        // Create new tour execution
        var tourExecution = new TourExecution(
            request.StartTourRequestDTO.TourId,
            touristId,
            request.StartTourRequestDTO.CurrentLatitude,
            request.StartTourRequestDTO.CurrentLongitude
        );

        tourExecutionRepository.Create(tourExecution);

        var resultDto = new StartTourExecutionDTO
        {
            Id = tourExecution.Id,
            TourId = tourExecution.TourId,
            TouristId = tourExecution.TouristId,
            StartTime = tourExecution.StartTime,
            Status = tourExecution.Status,
            StartLatitude = tourExecution.StartLatitude,
            StartLongitude = tourExecution.StartLongitude,
            CurrentLatitude = tourExecution.CurrentLatitude,
            CurrentLongitude = tourExecution.CurrentLongitude,
            LastActivity = tourExecution.LastActivity
        };

        return Result<StartTourExecutionDTO>.Success(resultDto);
    }
}
