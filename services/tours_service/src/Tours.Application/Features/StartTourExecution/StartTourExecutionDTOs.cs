using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Application.Features.StartTourExecution
{
  public class StartTourExecutionDTO
  {
    public long Id { get; set; }
    public long TourId { get; set; }
    public long TouristId { get; set; }
    public DateTime StartTime { get; set; }
    public TourExecutionStatus Status { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public DateTime LastActivity { get; set; }
  }

  public class StartTourRequestDTO
  {
    public long TourId { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
  }
}
