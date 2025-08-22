using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class Tour : BaseEntity
{
    public long AuthorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TourDifficulty Difficulty { get; set; }
    public List<string> Tags { get; set; }
    public TourStatus Status { get; set; }
    public decimal Price { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime ArchivedAt { get; set; }
    public double LengthKm { get; set; }

    public Tour(long authorId, string name, string description,
                TourDifficulty difficulty, List<string> tags)
    {
        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        Price = 0;
        Status = TourStatus.Draft;
        LengthKm = 0;
    }

    public void UpdateLength(double lengthKm)
    {
        LengthKm = lengthKm;
    }

    public void UpdateTour(string name, string description, TourDifficulty difficulty, List<string> tags, decimal price, TourStatus status)
    {
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        Price = price;
        Status = status;
    }

        public bool CanBePublished(List<Checkpoint> checkpoints)
        {
            // 1. Osnovni podaci
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || Tags == null || Tags.Count == 0)
                return false;
            // 2. Bar dve ključne tačke
            if (checkpoints == null || checkpoints.Count < 2)
                return false;
            return true;
        }
}
public enum TourDifficulty
{
    Easy,
    Medium,
    Hard
}
public enum TourStatus
{
    Draft,
    Published,
    Archived
}
public enum TransportType
{
    Walking,
    Bicycle,
    Car
}