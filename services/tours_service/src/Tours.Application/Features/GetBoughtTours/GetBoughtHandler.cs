using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using System.Linq;

namespace tours_service.src.Tours.Application.Features.GetBoughtTours;

public class GetBoughtToursHandler(
	ICrudRepository<TourPurchaseToken> tokenRepository,
	ICrudRepository<Tour> tourRepository,
	ICrudRepository<Checkpoint> checkpointRepository
) : IRequestHandler<GetBoughtToursQuery, Result<List<GetBoughtToursDTO>>>
{
	public async Task<Result<List<GetBoughtToursDTO>>> Handle(GetBoughtToursQuery request, CancellationToken cancellationToken)
	{
		var results = await Task.Run(() => {
			var tokens = tokenRepository.GetPaged(1, int.MaxValue).Results
				.Where(t => t.UserId.ToString() == request.UserDTO.Id)
				.ToList();

			var tours = tourRepository.GetPaged(1, int.MaxValue).Results.ToList();
			var checkpoints = checkpointRepository.GetPaged(1, int.MaxValue).Results.ToList();

			return tokens
				.Select(token => {
					var tour = tours.FirstOrDefault(t => t.Id == token.TourId);
					if (tour == null) return null;
					return new GetBoughtToursDTO {
						Id = tour.Id,
						AuthorId = tour.AuthorId,
						Name = tour.Name,
						Description = tour.Description,
						Difficulty = tour.Difficulty,
						Tags = tour.Tags,
						Status = tour.Status,
						Price = tour.Price,
						PublishedAt = tour.PublishedAt,
						ArchivedAt = tour.ArchivedAt,
						LengthKm = tour.LengthKm,
						CreatedAt = tour.CreatedAt,
						Checkpoints = checkpoints
							.Where(c => c.TourId == tour.Id)
							.Select(c => new CheckpointDTO {
								Id = c.Id,
								Name = c.Name,
								Description = c.Description,
								Latitude = c.Latitude,
								Longitude = c.Longitude,
								ImageBase64 = c.ImageBase64
							})
							.ToList()
					};
				})
				.OfType<GetBoughtToursDTO>()
				.ToList();
		});

		return Result<List<GetBoughtToursDTO>>.Success(results);
	}
}
