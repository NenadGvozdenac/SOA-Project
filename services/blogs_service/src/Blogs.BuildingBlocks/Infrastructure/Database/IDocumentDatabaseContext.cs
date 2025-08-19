using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using System.Linq.Expressions;

namespace blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;

public interface IDocumentDatabaseContext
{
    public Task<IQueryable<T>> GetCollection<T>(string collectionName) where T : BaseEntity;
    public Task<IQueryable<T>> GetCollection<T>(string collectionName, int pageNumber, int pageSize) where T : BaseEntity;
    public Task<IQueryable<T>> GetCollection<T>(string collectionName, int pageNumber, int pageSize, Expression<Func<T, bool>> filter) where T : BaseEntity;
    public Task<T> GetDocumentById<T>(string collectionName, string id) where T : BaseEntity;
    public Task AddDocument<T>(string collectionName, T document) where T : BaseEntity;
}