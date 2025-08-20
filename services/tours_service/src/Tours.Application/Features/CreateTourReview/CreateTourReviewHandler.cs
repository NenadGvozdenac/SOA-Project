using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CreateTourReview
{
  public class CreateTourReviewHandler(ICrudRepository<TourReview> tourReviewRepository) : IRequestHandler<CreateTourReviewCommand, Result<CreateTourReviewDTO>>
  {
    public async Task<Result<CreateTourReviewDTO>> Handle(CreateTourReviewCommand request, CancellationToken cancellationToken)
    {
      if (request.UserDTO.Role != "Tourist") 
      {
        return Result<CreateTourReviewDTO>.Failure("Only a tourist can leave a review for a tour.");
      }

        var tourReview = new TourReview
        (
            request.TourReviewDTO.TourId,
            Convert.ToInt64(request.UserDTO.Id),
            request.TourReviewDTO.Rating,
            request.TourReviewDTO.Comment,
            request.TourReviewDTO.CheckTourDate,
            request.TourReviewDTO.ImageBase64
        );

      tourReviewRepository.Create(tourReview);

      var dto = new CreateTourReviewDTO
      {
        TourId = tourReview.TourId,
        ReviewerId = tourReview.ReviewerId,
        Rating = tourReview.Rating,
        Comment = tourReview.Comment,
        CheckTourDate = tourReview.CheckTourDate,
        ImageBase64 = tourReview.ImageBase64,
        CreatedAt = DateTime.UtcNow
      };

      return Result<CreateTourReviewDTO>.Success(dto);
    }
  }
}
