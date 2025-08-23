using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.AddTourToCart;

public class AddTourToCartHandler(
    ICrudRepository<ShoppingCart> shoppingCartRepository,
    ICrudRepository<OrderItem> orderItemRepository
) : IRequestHandler<AddTourToCartCommand, Result<AddTourToCartDTO>>
{
    public async Task<Result<AddTourToCartDTO>> Handle(AddTourToCartCommand request, CancellationToken cancellationToken)
    {
        var allCarts = shoppingCartRepository.GetPaged(1, int.MaxValue);
        var cart = allCarts.Results.FirstOrDefault(c => c.UserId == Convert.ToInt64(request.UserDTO.Id));
        if (cart == null)
        {
            return Result<AddTourToCartDTO>.Failure("Shopping cart not found.");
        }

        var orderItem = new OrderItem(cart.Id, request.CreatedOrderItemDTO.TourId, request.CreatedOrderItemDTO.Price);

        cart.AddOrderItem(orderItem);

        orderItemRepository.Create(orderItem);

        shoppingCartRepository.Update(cart);

        var dto = new AddTourToCartDTO
        {
            Id = orderItem.Id,
            TourId = orderItem.TourId,
            ShoppingCartId = cart.Id,
            Price = orderItem.Price,
            CreatedAt = DateTime.UtcNow
        };

        return Result<AddTourToCartDTO>.Success(dto);
    }
}