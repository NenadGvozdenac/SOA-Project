using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CreateTour
{
  public class CreateTourHandler(ICrudRepository<Tour> tourRepository) : IRequestHandler<CreateTourCommand, Result<CreateTourDTO>>
  {
    public async Task<Result<CreateTourDTO>> Handle(CreateTourCommand request, CancellationToken cancellationToken)
    {
      if (request.UserDTO.Role != "Guide") //HARDKODOVANO IZ TABELE roles u stakeholders_db (GUIDE GLEDAMO KAO AUTHOR)
      {
        return Result<CreateTourDTO>.Failure("Samo autor može da kreira turu.");
      }

      var tour = new Tour(
        Convert.ToInt64(request.UserDTO.Id),
        request.CreatedTourDTO.Name,
        request.CreatedTourDTO.Description,
        Enum.Parse<TourDifficulty>(request.CreatedTourDTO.Difficulty, ignoreCase: true),
        request.CreatedTourDTO.Tags
      );

      tourRepository.Create(tour);

      var dto = new CreateTourDTO
      {
        Id = tour.Id,
        Name = tour.Name,
        Description = tour.Description,
        Difficulty = tour.Difficulty,
        Tags = tour.Tags,
        Status = tour.Status,
        Price = tour.Price,
        AuthorId = tour.AuthorId,
        CreatedAt = tour.CreatedAt,
        PublishedAt = tour.PublishedAt,
        ArchivedAt = tour.ArchivedAt,
        LengthKm = tour.LengthKm
      };

      return Result<CreateTourDTO>.Success(dto);
    }
  }
}
