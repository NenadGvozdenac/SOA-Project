using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.GetAllTours;

public class GetAllToursHandler(ICrudRepository<Tour> tourRepository) : IRequestHandler<GetAllToursQuery, Result<List<GetAllToursDTO>>>
{
    public async Task<Result<List<GetAllToursDTO>>> Handle(GetAllToursQuery request, CancellationToken cancellationToken)
    {
        // if (request.UserDTO.Role != "Guide") //HARDKODOVANO IZ TABELE roles u stakeholders_db (GUIDE GLEDAMO KAO AUTHOR)
        // {
        //   return Result<List<GetAllToursDTO>>.Failure("Samo autor može da dobavi ture.");
        // }
        var tours = tourRepository.GetPaged(1, int.MaxValue);

        List<GetAllToursDTO> filteredTours;
        
        if (request.UserDTO.Role == "Guide")
        {
          filteredTours = tours.Results
          .Where(t => t.AuthorId == Convert.ToInt64(request.UserDTO.Id))
          .Select(t => new GetAllToursDTO
          {
            Id = t.Id,
            AuthorId = t.AuthorId,
            Name = t.Name,
            Description = t.Description,
            Difficulty = t.Difficulty,
            Tags = t.Tags,
            Status = t.Status,
            Price = t.Price,
            CreatedAt = t.CreatedAt,
            PublishedAt = t.PublishedAt,
            ArchivedAt = t.ArchivedAt,
            LengthKm = t.LengthKm
          })
          .ToList();
        }
        else if (request.UserDTO.Role == "Tourist")
        {
          filteredTours = tours.Results
          .Where(t => t.Status == TourStatus.Published)
          .Select(t => new GetAllToursDTO
          {
            Id = t.Id,
            AuthorId = t.AuthorId,
            Name = t.Name,
            Description = t.Description,
            Difficulty = t.Difficulty,
            Tags = t.Tags,
            Status = t.Status,
            Price = t.Price,
            CreatedAt = t.CreatedAt,
            PublishedAt = t.PublishedAt,
            ArchivedAt = t.ArchivedAt,
            LengthKm = t.LengthKm
          })
          .ToList();
        }
        else
        {
            return Result<List<GetAllToursDTO>>.Failure("Invalid user role.");
        }

        return Result<List<GetAllToursDTO>>.Success(filteredTours);
    }
}