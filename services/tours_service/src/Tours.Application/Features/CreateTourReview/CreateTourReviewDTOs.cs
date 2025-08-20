using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Application.Features.CreateTourReview;

public class CreateTourReviewDTO
{
    public long TourId { get; set; }
    public long ReviewerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CheckTourDate { get; set; }
    public string? ImageBase64 { get; set; }
    public DateTime CreatedAt { get; set; } 
}
