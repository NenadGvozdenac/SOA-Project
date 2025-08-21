using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class Checkpoint : BaseEntity
{
    public long TourId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? ImageBase64 { get; private set; }

    public Checkpoint(long tourId, string name, string description, double latitude, double longitude, string? imageBase64)
    {
        TourId = tourId;
        Name = name;
        Description = description;
        Latitude = latitude;
        Longitude = longitude;
        ImageBase64 = imageBase64;
    }
}
