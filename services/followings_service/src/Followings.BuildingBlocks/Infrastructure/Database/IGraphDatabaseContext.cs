using Neo4j.Driver;

namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;

public interface IGraphDatabaseContext : IDisposable
{
    public Task<IResultCursor> RunAsync(string query, object parameters);
    public Task<IResultCursor> RunAsync(string query);
}