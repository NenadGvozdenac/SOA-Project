using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Followers.GetFollowSuggestions;

public class GetFollowSuggestionsHandler(
    IGraphDatabaseContext context,
    IStakeholdersServiceClient stakeholdersServiceClient
) : IRequestHandler<GetFollowSuggestionsQuery, Result<List<FollowerDTO>>>
{
    public async Task<Result<List<FollowerDTO>>> Handle(GetFollowSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var query = @"
            MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
            MATCH (f)-[:FOLLOWS]->(suggested:User)
            WHERE NOT (u)-[:FOLLOWS]->(suggested) AND u <> suggested
            RETURN suggested.id AS id
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

            // Extract user IDs from Neo4j results
            var userIds = result.Select(r => r["id"].As<string>()).ToList();

            if (!userIds.Any())
            {
                return Result<List<FollowerDTO>>.Success(new List<FollowerDTO>());
            }

            // Get detailed user information from stakeholders service
            var userDetails = await stakeholdersServiceClient.GetUsersByIdsAsync(userIds);

            // Convert to FollowerDTO with detailed information
            var suggestions = userDetails.Select(user => new FollowerDTO(
                user.Id,
                user.Username,
                user.Name,
                user.Email,
                user.ProfilePicture
            )).ToList();

            return Result<List<FollowerDTO>>.Success(suggestions);
        }
        catch (Exception e)
        {
            return Result<List<FollowerDTO>>.Failure(e.Message);
        }
    }
}