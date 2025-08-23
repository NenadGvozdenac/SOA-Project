namespace tours_service.src.Tours.API.DTOs;

public class OrderItemFromCartDTO
{
    public long Id { get; set; }
    public long ShoppingCartId { get; set; }
    public long TourId { get; set; }
    public double Price { get; set; }
}
