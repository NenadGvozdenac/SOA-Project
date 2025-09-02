using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.Application.Domain;

public class TourExecution : BaseEntity
{
    public long TourId { get; set; }
    public long TouristId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TourExecutionStatus Status { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public DateTime LastActivity { get; set; }
    public List<CheckpointProgress> CheckpointProgresses { get; set; }

    public TourExecution(long tourId, long touristId, double startLatitude, double startLongitude)
    {
        TourId = tourId;
        TouristId = touristId;
        StartTime = DateTime.UtcNow;
        Status = TourExecutionStatus.Active;
        StartLatitude = startLatitude;
        StartLongitude = startLongitude;
        CurrentLatitude = startLatitude;
        CurrentLongitude = startLongitude;
        LastActivity = DateTime.UtcNow;
        CheckpointProgresses = new List<CheckpointProgress>();
    }

    public void UpdatePosition(double latitude, double longitude)
    {
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        LastActivity = DateTime.UtcNow;
    }

    public void CompleteTour()
    {
        Status = TourExecutionStatus.Completed;
        EndTime = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
    }

    public void AbandonTour()
    {
        Status = TourExecutionStatus.Abandoned;
        EndTime = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
    }

    public void CompleteCheckpoint(long checkpointId, DateTime completedAt)
    {
        var existingProgress = CheckpointProgresses.FirstOrDefault(cp => cp.CheckpointId == checkpointId);
        if (existingProgress == null)
        {
            CheckpointProgresses.Add(new CheckpointProgress(checkpointId, completedAt));
        }
        LastActivity = DateTime.UtcNow;
    }
}

public class CheckpointProgress : BaseEntity
{
    public long CheckpointId { get; set; }
    public DateTime CompletedAt { get; set; }

    public CheckpointProgress(long checkpointId, DateTime completedAt)
    {
        CheckpointId = checkpointId;
        CompletedAt = completedAt;
    }
}

public enum TourExecutionStatus
{
    Active,
    Completed,
    Abandoned
}
