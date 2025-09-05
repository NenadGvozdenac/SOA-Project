using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using static tours_service.src.Tours.Application.Features.CreateShoppingCart.CreateShoppingCartDTO;

namespace tours_service.src.Tours.Application.Features.CreateShoppingCart
{
    public class CreateShoppingCartHandler(ICrudRepository<ShoppingCart> shoppingCartRepository) : IRequestHandler<CreateShoppingCartCommand, Result<CreateShoppingCartDTO>>
    {
        public async Task<Result<CreateShoppingCartDTO>> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
        {
            var cart = new ShoppingCart(request.UserId);
            
            shoppingCartRepository.Create(cart);

            var resultDto = new CreateShoppingCartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                TotalPrice = cart.TotalPrice,
                OrderItems = cart.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    TourId = oi.TourId,
                    Price = oi.Price
                }).ToList()
            };

            return Result<CreateShoppingCartDTO>.Success(resultDto);
        }
    }
}