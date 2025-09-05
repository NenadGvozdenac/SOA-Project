using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.UpdateTour;

public record UpdateTourCommand(UserDTO UserDTO, long TourId, UpdatedTourDTO UpdatedTourDTO) : IRequest<Result<UpdateTourDTO>>;