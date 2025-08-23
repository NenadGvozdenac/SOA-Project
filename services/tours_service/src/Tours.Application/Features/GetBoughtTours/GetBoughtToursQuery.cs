using MediatR;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Features.GetBoughtTours;

public record GetBoughtToursQuery(UserDTO UserDTO) : IRequest<Result<List<GetBoughtToursDTO>>>;