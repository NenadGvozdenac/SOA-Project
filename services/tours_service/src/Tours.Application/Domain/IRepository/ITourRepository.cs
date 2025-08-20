using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Domain.IRepository;

public interface ITourRepository : ICrudRepository<Tour>
{
  // No additional members needed
}
