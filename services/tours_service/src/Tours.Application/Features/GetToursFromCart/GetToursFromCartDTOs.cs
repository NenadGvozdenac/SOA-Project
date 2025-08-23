namespace tours_service.src.Tours.Application.Features.GetToursFromCart
{
    public class GetToursFromCartDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public double TotalPrice { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }

    public class OrderItemDTO
    {
        public long Id { get; set; }
        public long TourId { get; set; }
        public double Price { get; set; }
    }
}