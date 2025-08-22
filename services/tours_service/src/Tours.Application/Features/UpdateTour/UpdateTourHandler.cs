using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.UpdateTour;

public class UpdateTourHandler(ICrudRepository<Tour> tourRepository) : IRequestHandler<UpdateTourCommand, Result<UpdateTourDTO>>
{
    public async Task<Result<UpdateTourDTO>> Handle(UpdateTourCommand request, CancellationToken cancellationToken)
    {
        var tour = tourRepository.Get(request.TourId);
        if (tour == null)
            return Result<UpdateTourDTO>.Failure("Tour not found");

        tour.Name = request.UpdatedTourDTO.Name;
        tour.Description = request.UpdatedTourDTO.Description;
        tour.Difficulty = Enum.Parse<TourDifficulty>(request.UpdatedTourDTO.Difficulty);
        tour.Tags = request.UpdatedTourDTO.Tags;
        tour.Price = request.UpdatedTourDTO.Price;
        tour.Status = Enum.Parse<TourStatus>(request.UpdatedTourDTO.Status);
        tour.LengthKm = request.UpdatedTourDTO.LengthKm;

        tourRepository.Update(tour);

        var updatedTourDTO = new UpdateTourDTO
        {
          Id = tour.Id,
          AuthorId = tour.AuthorId,
          Name = tour.Name,
          Description = tour.Description,
          Difficulty = tour.Difficulty,
          Tags = tour.Tags,
          Price = tour.Price,
          Status = tour.Status,
          LengthKm = tour.LengthKm,
          CreatedAt = tour.CreatedAt,
          PublishedAt = tour.PublishedAt,
          ArchivedAt = tour.ArchivedAt
        };
        return Result<UpdateTourDTO>.Success(updatedTourDTO);
    }
}
