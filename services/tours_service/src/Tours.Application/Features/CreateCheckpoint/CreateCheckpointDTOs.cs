namespace tours_service.src.Tours.Application.Features.CreateCheckpoint
{
  public class CreateCheckpointDTO
  {
    public long Id { get; set; }
    public long TourId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageBase64 { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
