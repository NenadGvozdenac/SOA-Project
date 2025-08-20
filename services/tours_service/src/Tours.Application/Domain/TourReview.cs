using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class TourReview : BaseEntity
{
    public long TourId { get; private set; }
    public long ReviewerId { get; private set; }
    public int Rating { get; private set; } 
    public string Comment { get; private set; }

    public DateTime CheckTourDate { get; private set; }
    public string? ImageBase64 { get; private set; }

    public TourReview(long tourId, long reviewerId, int rating, string comment, DateTime checkTourDate, string? imageBase64 = null)
    {
        TourId = tourId;
        ReviewerId = reviewerId;
        Rating = rating;
        Comment = comment;
        CheckTourDate = checkTourDate;
        ImageBase64 = imageBase64;
    }
}