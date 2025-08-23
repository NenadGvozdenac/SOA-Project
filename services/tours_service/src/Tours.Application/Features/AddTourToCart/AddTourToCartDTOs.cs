namespace tours_service.src.Tours.Application.Features.AddTourToCart;

public class AddTourToCartDTO
{
    public long Id { get; set; }
    public long TourId { get; set; }
    public long ShoppingCartId { get; set; }
    public double Price { get; set; }
    public DateTime CreatedAt { get; set; }
}