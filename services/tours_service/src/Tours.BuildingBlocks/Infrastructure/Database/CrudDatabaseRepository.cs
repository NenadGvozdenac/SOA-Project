
using Microsoft.EntityFrameworkCore;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;

namespace tours_service.src.Tours.BuildingBlocks.Infrastructure.Database;
public class CrudDatabaseRepository<TEntity, TDbContext> : ICrudRepository<TEntity>
    where TEntity : BaseEntity
    where TDbContext : DbContext
{
  protected readonly TDbContext DbContext;
  private readonly DbSet<TEntity> _dbSet;

  public CrudDatabaseRepository(TDbContext dbContext)
  {
    DbContext = dbContext;
    _dbSet = DbContext.Set<TEntity>();
  }

  public PagedResult<TEntity> GetPaged(int page, int pageSize)
  {
    var task = _dbSet.IncludeRelatedEntities().GetPagedById(page, pageSize);
    task.Wait();
    return task.Result;
  }

  public TEntity Get(long id)
  {
    var entity = _dbSet.IncludeRelatedEntities().FirstOrDefault(t => t.Id == id);
    if (entity == null) throw new KeyNotFoundException("404: Not found: " + id);
    return entity;
  }

  public TEntity Create(TEntity entity)
  {
    _dbSet.Add(entity);
    DbContext.SaveChanges();
    return entity;
  }

  public TEntity Update(TEntity entity)
  {
    try
    {
      DbContext.Update(entity);
      DbContext.SaveChanges();
    }
    catch (DbUpdateException e)
    {
      throw new KeyNotFoundException(e.Message);
    }
    return entity;
  }

  public void Delete(long id)
  {
    var entity = Get(id);
    _dbSet.Remove(entity);
    DbContext.SaveChanges();
  }
}