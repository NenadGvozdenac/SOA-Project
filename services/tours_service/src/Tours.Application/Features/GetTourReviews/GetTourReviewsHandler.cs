using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using System.Linq;
using System.Threading.Tasks;

namespace tours_service.src.Tours.Application.Features.GetTourReviews;

public class GetTourReviewsHandler(ICrudRepository<TourReview> tourReviewRepository) : IRequestHandler<GetTourReviewsQuery, Result<List<GetTourReviewDTO>>>
{
    public async Task<Result<List<GetTourReviewDTO>>> Handle(GetTourReviewsQuery request, CancellationToken cancellationToken)
    {
        var allReviews = tourReviewRepository.GetPaged(1, int.MaxValue);
        var reviewsForTour = allReviews.Results.Where(r => r.TourId == request.TourId).ToList();
        
        var reviewDTOs = reviewsForTour.Select(r => new GetTourReviewDTO
        {
            Id = r.Id,
            TourId = r.TourId,
            ReviewerId = r.ReviewerId,
            Rating = r.Rating,
            Comment = r.Comment,
            CheckTourDate = r.CheckTourDate,
            ImageBase64 = r.ImageBase64
        }).ToList();

       return Result<List<GetTourReviewDTO>>.Success(reviewDTOs);
    }
}
