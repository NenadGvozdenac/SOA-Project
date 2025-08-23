namespace tours_service.src.Tours.Application.Features.GetTourReviews
{
    public class GetTourReviewDTO
    {
        public long Id { get; set; }
        public long TourId { get; set; }
        public long ReviewerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CheckTourDate { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
