using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using MediatR;
using tours_service.src.Tours.API.DTOs;

namespace tours_service.src.Tours.Application.Features.CreateTour
{
  public record CreateTourCommand(UserDTO UserDTO, CreatedTourDTO CreatedTourDTO) : IRequest<Result<CreateTourDTO>>;
}
