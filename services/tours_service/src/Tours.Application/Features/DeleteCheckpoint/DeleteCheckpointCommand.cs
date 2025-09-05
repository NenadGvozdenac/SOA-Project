using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.DeleteCheckpoint;

public record DeleteCheckpointCommand(UserDTO UserDTO, long CheckpointId) : IRequest<Result<bool>>;