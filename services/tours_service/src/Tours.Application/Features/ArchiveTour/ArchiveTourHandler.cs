using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.ArchiveTour;

public class ArchiveTourHandler(ICrudRepository<Tour> tourRepository) : IRequestHandler<ArchiveTourCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ArchiveTourCommand request, CancellationToken cancellationToken)
    {
        var tour = tourRepository.Get(request.TourId);
        if (tour == null)
            return Result<bool>.Failure("Tour not found");
        tour.Status = TourStatus.Archived;
        tour.ArchivedAt = DateTime.UtcNow;
        tourRepository.Update(tour);
        return Result<bool>.Success(true);
    }
}
