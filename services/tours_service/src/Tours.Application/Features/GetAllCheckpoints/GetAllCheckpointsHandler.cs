using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.GetAllCheckpoints;

public class GetAllCheckpointsHandler(ICrudRepository<Checkpoint> checkpointRepository) : IRequestHandler<GetAllCheckpointsQuery, Result<List<GetAllCheckpointsDTO>>>
{
    public async Task<Result<List<GetAllCheckpointsDTO>>> Handle(GetAllCheckpointsQuery request, CancellationToken cancellationToken)
    {
        var checkpoints = checkpointRepository.GetPaged(1, int.MaxValue);

        var filteredCheckpoints = checkpoints.Results
            .Where(c => c.TourId == request.tourId)
            .Select(c => new GetAllCheckpointsDTO
            {
                Id = c.Id,
                TourId = c.TourId,
                Name = c.Name,
                Description = c.Description,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                ImageBase64 = c.ImageBase64,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return Result<List<GetAllCheckpointsDTO>>.Success(filteredCheckpoints);
    }
}