using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain
{
    public class TourPurchaseToken : BaseEntity
    {
        public long UserId { get; set; }
        public long TourId { get; set; }
        public string TokenValue { get; set; } = string.Empty;

        public TourPurchaseToken(long userId, long tourId, string tokenValue)
        {
            UserId = userId;
            TourId = tourId;
            TokenValue = tokenValue;
        }
    }
}
