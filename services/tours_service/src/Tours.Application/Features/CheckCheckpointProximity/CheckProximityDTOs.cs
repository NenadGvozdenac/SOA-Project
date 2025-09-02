namespace tours_service.src.Tours.Application.Features.CheckCheckpointProximity
{
  public class CheckProximityRequestDTO
  {
    public long TourExecutionId { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
  }

  public class CheckProximityResponseDTO
  {
    public bool IsNearCheckpoint { get; set; }
    public long? CheckpointId { get; set; }
    public string? CheckpointName { get; set; }
    public double? DistanceMeters { get; set; }
    public bool? CheckpointCompleted { get; set; }
    public DateTime LastActivity { get; set; }
  }
}
