using MediatR;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.PublishTour;

public record PublishTourCommand(UserDTO UserDTO, long TourId, decimal Price) : IRequest<Result<bool>>;
