using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Followers.GetFollowSuggestions;

public class GetFollowSuggestionsHandler(IGraphDatabaseContext context) : IRequestHandler<GetFollowSuggestionsQuery, Result<List<FollowerDTO>>>
{
    public async Task<Result<List<FollowerDTO>>> Handle(GetFollowSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var query = @"
            MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
            MATCH (f)-[:FOLLOWS]->(suggested:User)
            WHERE NOT (u)-[:FOLLOWS]->(suggested) AND u <> suggested
            RETURN suggested.id AS id, suggested.name AS name, suggested.username AS username
            LIMIT 10
        ";

        var parameters = new Dictionary<string, object>
        {
            { "userId", request.UserDTO.Id }
        };

        try
        {
            var resultCursor = await context.RunAsync(query, parameters);
            var result = await resultCursor.ToListAsync(cancellationToken);

            var suggestions = result.Select(r =>
            {
                var suggested = r["suggested"].As<INode>();
                return new FollowerDTO(
                    suggested["id"].As<string>()
                );
            }).ToList();

            return Result<List<FollowerDTO>>.Success(suggestions);
        }
        catch (Exception e)
        {
            return Result<List<FollowerDTO>>.Failure(e.Message);
        }
    }
}