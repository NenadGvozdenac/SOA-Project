using System;
using System.Collections.Generic;
using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Application.Features.GetBoughtTours;

public class GetBoughtToursDTO
{
    public long Id { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public List<CheckpointDTO> Checkpoints { get; set; } = new();
}

public class CheckpointDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageBase64 { get; set; }
}

