using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.GetToursFromCart;

public record GetToursFromCartQuery(UserDTO UserDTO) : IRequest<Result<GetToursFromCartDTO>>;