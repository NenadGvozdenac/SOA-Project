using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class OrderItem : BaseEntity
{
    public long ShoppingCartId { get; set; }
    public long TourId { get; set; }
    public double Price { get; set; }
    public OrderItem(long shoppingCartId, long tourId, double price)
    {
        ShoppingCartId = shoppingCartId;
        TourId = tourId;
        Price = price;
    }
}