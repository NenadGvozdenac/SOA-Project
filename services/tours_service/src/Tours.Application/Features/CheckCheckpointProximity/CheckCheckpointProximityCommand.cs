using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.CheckCheckpointProximity;

public record CheckCheckpointProximityCommand(UserDTO UserDTO, CheckProximityRequestDTO CheckProximityRequestDTO) : IRequest<Result<CheckProximityResponseDTO>>;
