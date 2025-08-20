
namespace tours_service.src.Tours.API.DTOs;

public class TourReviewDTO
{
    public long TourId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CheckTourDate { get; set; }
    public string? ImageBase64 { get; set; }
}