namespace tours_service.src.Tours.BuildingBlocks.Core.Domain;

public abstract class BaseEntity
{
    public long Id { get; protected set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}