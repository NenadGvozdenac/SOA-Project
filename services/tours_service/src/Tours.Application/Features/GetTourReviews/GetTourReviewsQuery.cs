using MediatR;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;

namespace tours_service.src.Tours.Application.Features.GetTourReviews;

public record GetTourReviewsQuery(UserDTO UserDTO, long TourId) : IRequest<Result<List<GetTourReviewDTO>>>;