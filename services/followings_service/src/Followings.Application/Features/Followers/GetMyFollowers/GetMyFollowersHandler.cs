using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Followers.GetMyFollowers;

public class GetMyFollowersHandler(IGraphDatabaseContext context) : IRequestHandler<GetMyFollowersQuery, Result<List<FollowerDTO>>>
{
    public async Task<Result<List<FollowerDTO>>> Handle(GetMyFollowersQuery request, CancellationToken cancellationToken)
    {
        var query = @"
            MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
            RETURN f
        ";

        var parameters = new Dictionary<string, object>
        {
            { "userId", request.UserDTO.Id }
        };

        try
        {
            var resultCursor = await context.RunAsync(query, parameters);
            var result = await resultCursor.ToListAsync(cancellationToken);

            var followers = result.Select(r =>
            {
                var follower = r["f"].As<INode>();
                return new FollowerDTO(
                    follower["id"].As<string>()
                );
            }).ToList();

            return Result<List<FollowerDTO>>.Success(followers);
        }
        catch (Exception e)
        {
            return Result<List<FollowerDTO>>.Failure(e.Message);
        }
    }
}
