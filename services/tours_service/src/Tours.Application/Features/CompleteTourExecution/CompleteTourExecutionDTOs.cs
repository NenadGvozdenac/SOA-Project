using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Application.Features.CompleteTourExecution
{
  public class CompleteTourExecutionDTO
  {
    public long Id { get; set; }
    public long TourId { get; set; }
    public long TouristId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TourExecutionStatus Status { get; set; }
    public int CompletedCheckpoints { get; set; }
    public int TotalCheckpoints { get; set; }
  }

  public class CompleteTourRequestDTO
  {
    public long TourExecutionId { get; set; }
    public bool IsAbandoned { get; set; } // true for abandon, false for complete
  }
}
