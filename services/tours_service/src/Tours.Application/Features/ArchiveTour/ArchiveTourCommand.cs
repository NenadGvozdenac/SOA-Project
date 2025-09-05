using MediatR;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.ArchiveTour;

public record ArchiveTourCommand(UserDTO UserDTO, long TourId) : IRequest<Result<bool>>;
