using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using tours_service.src.Tours.API.DTOs;
using System.Linq;
using tours_service.src.Tours.Application.Features.GetToursFromCart;

namespace tours_service.src.Tours.Application.Features.GetToursFromCart;

public class GetToursFromCartHandler(ICrudRepository<ShoppingCart> shoppingCartRepository) : IRequestHandler<GetToursFromCartQuery, Result<GetToursFromCartDTO>>
{
	public async Task<Result<GetToursFromCartDTO>> Handle(GetToursFromCartQuery request, CancellationToken cancellationToken)
	{
		var allCarts = shoppingCartRepository.GetPaged(1, int.MaxValue);
		var cart = allCarts.Results.FirstOrDefault(c => c.UserId.ToString() == request.UserDTO.Id);
		if (cart == null)
		{
			return Result<GetToursFromCartDTO>.Failure("Shopping cart not found.");
		}

		var dto = new GetToursFromCartDTO
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

		return Result<GetToursFromCartDTO>.Success(dto);
	}
}
