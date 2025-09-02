using tours_service.src.Tours.Application.Features.StartTourExecution;
using tours_service.src.Tours.Application.Features.CheckCheckpointProximity;
using tours_service.src.Tours.Application.Features.CompleteTourExecution;

namespace tours_service.src.Tours.API.DTOs
{
    // Re-export DTOs from feature modules for easier access in controllers
    public class StartTourRequestApiDTO : StartTourRequestDTO { }
    public class StartTourExecutionApiDTO : StartTourExecutionDTO { }
    
    public class CheckProximityRequestApiDTO : CheckProximityRequestDTO { }
    public class CheckProximityResponseApiDTO : CheckProximityResponseDTO { }
    
    public class CompleteTourRequestApiDTO : CompleteTourRequestDTO { }
    public class CompleteTourExecutionApiDTO : CompleteTourExecutionDTO { }
}
