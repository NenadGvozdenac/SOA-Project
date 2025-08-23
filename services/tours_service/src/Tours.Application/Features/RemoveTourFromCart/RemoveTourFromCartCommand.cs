using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.RemoveTourFromCart;

public record RemoveTourFromCartCommand(UserDTO UserDTO, long OrderItemId) : IRequest<Result<bool>>;