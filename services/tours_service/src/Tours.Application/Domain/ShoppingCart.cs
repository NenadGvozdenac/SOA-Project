using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class ShoppingCart : BaseEntity
{
    public long UserId { get; set; }
    public double TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();

    public ShoppingCart(long userId)
    {
        UserId = userId;
    }

    public void AddOrderItem(OrderItem item)
    {
        OrderItems.Add(item);
        RecalculateTotalPrice();
    }

    public void RemoveOrderItem(OrderItem item)
    {
        OrderItems.Remove(item);
        RecalculateTotalPrice();
    }

    public void RecalculateTotalPrice()
    {
        TotalPrice = OrderItems.Sum(item => item.Price);
    }
}
