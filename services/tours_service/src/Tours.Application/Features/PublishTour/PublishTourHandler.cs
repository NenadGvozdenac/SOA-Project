using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.PublishTour;

public class PublishTourHandler(ICrudRepository<Tour> tourRepository, ICrudRepository<Checkpoint> checkpointRepository) : IRequestHandler<PublishTourCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PublishTourCommand request, CancellationToken cancellationToken)
    {
        var tour = tourRepository.Get(request.TourId);
        if (tour == null)
            return Result<bool>.Failure("Tour not found");

        var pagedCheckpoints = checkpointRepository.GetPaged(1, int.MaxValue);
        var checkpoints = pagedCheckpoints.Results.Where(c => c.TourId == request.TourId).ToList();
        if (!tour.CanBePublished(checkpoints))
            return Result<bool>.Failure("Tour does not meet the requirements for publishing: basic information and at least two checkpoints are required.");

        tour.Status = TourStatus.Published;
        tour.PublishedAt = DateTime.UtcNow;
        tour.Price = request.Price;
        tourRepository.Update(tour);
        return Result<bool>.Success(true);
    }
}
