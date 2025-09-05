using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.BuildingBlocks.Core.UseCases;

public interface ICrudRepository<TEntity> where TEntity : BaseEntity
{
    PagedResult<TEntity> GetPaged(int page, int pageSize);
    TEntity Get(long id);
    TEntity Create(TEntity entity);
    TEntity Update(TEntity entity);
    void Delete(long id);
}