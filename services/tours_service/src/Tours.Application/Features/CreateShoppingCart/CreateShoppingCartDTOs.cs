namespace tours_service.src.Tours.Application.Features.CreateShoppingCart
{
    public class CreateShoppingCartDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
        public double TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public class OrderItemDTO
        {
            public long Id { get; set; }
            public long TourId { get; set; }
            public double Price { get; set; }
        }
    }
}
