using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.API.DTOs;

public class CreatedTourDTO
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Difficulty { get; set; } // Tour difficulty ENUM!!!
    public List<string> Tags { get; set; } = new();
}
