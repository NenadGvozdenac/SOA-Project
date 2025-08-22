namespace tours_service.src.Tours.API.DTOs;

public class UpdatedTourDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Difficulty { get; set; }
    public List<string> Tags { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
    public double LengthKm { get; set; }
}
