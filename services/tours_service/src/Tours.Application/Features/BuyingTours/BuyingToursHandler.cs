using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.BuyingTours;

public class BuyingToursHandler(
    ICrudRepository<TourPurchaseToken> tokenRepository,
    ICrudRepository<OrderItem> orderItemRepository,
    ICrudRepository<ShoppingCart> shoppingCartRepository
) : IRequestHandler<BuyToursCommand, Result<BuyingToursDTO>>
{
    public async Task<Result<BuyingToursDTO>> Handle(BuyToursCommand request, CancellationToken cancellationToken)
    {
        var purchasedTours = new List<PurchasedTourDTO>();

        // Pronađi shopping cart korisnika
        var allCarts = shoppingCartRepository.GetPaged(1, int.MaxValue);
        var cart = allCarts.Results.FirstOrDefault(c => c.UserId == Convert.ToInt64(request.UserDTO.Id));
        if (cart == null)
        {
            return Result<BuyingToursDTO>.Failure("Shopping cart not found.");
        }

        foreach (var item in request.OrderItemFromCartDTOs)
        {
            var tokenValue = Guid.NewGuid().ToString();
            var purchaseToken = new TourPurchaseToken(Convert.ToInt64(request.UserDTO.Id), item.TourId, tokenValue);
            tokenRepository.Create(purchaseToken);

            var orderItem = cart.OrderItems.FirstOrDefault(oi => oi.Id == item.Id);
            if (orderItem != null)
            {
                cart.OrderItems.Remove(orderItem);
                orderItemRepository.Delete(orderItem.Id);
            }

            purchasedTours.Add(new PurchasedTourDTO
            {
                TourId = item.TourId,
                Price = item.Price,
                PurchaseToken = tokenValue
            });
        }

        cart.TotalPrice = 0;
        shoppingCartRepository.Update(cart);

        var resultDto = new BuyingToursDTO
        {
            Id = cart.Id,
            UserId = Convert.ToInt64(request.UserDTO.Id),
            PurchasedTours = purchasedTours
        };

        return Result<BuyingToursDTO>.Success(resultDto);
    }
}
