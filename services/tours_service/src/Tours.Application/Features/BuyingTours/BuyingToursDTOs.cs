namespace tours_service.src.Tours.Application.Features.BuyingTours
{
    public class BuyingToursDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<PurchasedTourDTO> PurchasedTours { get; set; } = new();
    }

    public class PurchasedTourDTO
    {
        public long TourId { get; set; }
        public double Price { get; set; }
        public string PurchaseToken { get; set; } = string.Empty;
    }
}