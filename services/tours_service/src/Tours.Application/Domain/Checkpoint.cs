using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class Checkpoint : BaseEntity
{
    public long TourId { get;  set; }
    public string Name { get;  set; }
    public string Description { get;  set; }
    public double Latitude { get;  set; }
    public double Longitude { get;  set; }
    public string? ImageBase64 { get;  set; }

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
