using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using System.Linq;

namespace tours_service.src.Tours.Application.Features.RemoveTourFromCart;

public class RemoveTourFromCartHandler(
    ICrudRepository<ShoppingCart> shoppingCartRepository,
    ICrudRepository<OrderItem> orderItemRepository
) : IRequestHandler<RemoveTourFromCartCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveTourFromCartCommand request, CancellationToken cancellationToken)
    {
        var allCarts = shoppingCartRepository.GetPaged(1, int.MaxValue);
        var cart = allCarts.Results.FirstOrDefault(c => c.UserId == Convert.ToInt64(request.UserDTO.Id));
        if (cart == null)
        {
            return Result<bool>.Failure("Shopping cart not found.");
        }

        var orderItem = cart.OrderItems.FirstOrDefault(oi => oi.Id == request.OrderItemId);
        if (orderItem == null)
        {
            return Result<bool>.Failure("Order item not found in cart.");
        }

        cart.RemoveOrderItem(orderItem);
        orderItemRepository.Delete(orderItem.Id);
        shoppingCartRepository.Update(cart);

        return Result<bool>.Success(true);
    }
}