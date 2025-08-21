namespace tours_service.src.Tours.API.DTOs;

public class CreatedCheckpointDTO
{
    public long TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageBase64 { get; set; }
}
