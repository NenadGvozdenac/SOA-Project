using MediatR;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.GetAllCheckpoints
{
  public record GetAllCheckpointsQuery(UserDTO UserDTO, long tourId) : IRequest<Result<List<GetAllCheckpointsDTO>>>;
}