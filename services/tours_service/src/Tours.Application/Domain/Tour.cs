using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class Tour : BaseEntity
{
    public long AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public decimal Price { get; private set; }

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