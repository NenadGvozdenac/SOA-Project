using MediatR;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.GetAllTours
{
  public record GetAllToursQuery(UserDTO UserDTO) : IRequest<Result<List<GetAllToursDTO>>>;
}