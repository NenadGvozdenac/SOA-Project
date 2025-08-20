using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using MediatR;
using tours_service.src.Tours.API.DTOs;

namespace tours_service.src.Tours.Application.Features.CreateTourReview
{
  public record CreateTourReviewCommand(UserDTO UserDTO, TourReviewDTO TourReviewDTO) : IRequest<Result<CreateTourReviewDTO>>;
}