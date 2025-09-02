using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.CompleteTourExecution;

public record CompleteTourExecutionCommand(UserDTO UserDTO, CompleteTourRequestDTO CompleteTourRequestDTO) : IRequest<Result<CompleteTourExecutionDTO>>;
