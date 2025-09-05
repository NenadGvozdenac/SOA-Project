using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Application.Features.CreateTour;

public class CreateTourDTO
{
    public long Id { get; set; }
    public long AuthorId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TourDifficulty Difficulty { get; set; }
    public List<string> Tags { get; set; } = new();
    public TourStatus Status { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime ArchivedAt { get; set; }
    public double LengthKm { get; set; }
}
