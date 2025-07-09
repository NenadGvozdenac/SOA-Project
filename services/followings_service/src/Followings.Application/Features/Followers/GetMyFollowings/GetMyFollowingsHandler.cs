using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Followers.GetMyFollowings;

public class GetMyFollowingsHandler(
    IGraphDatabaseContext context,
    IStakeholdersServiceClient stakeholdersServiceClient
) : IRequestHandler<GetMyFollowingsQuery, Result<List<FollowingDTO>>>
{
    public async Task<Result<List<FollowingDTO>>> Handle(GetMyFollowingsQuery request, CancellationToken cancellationToken)
    {
        var query = @"
            MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
            RETURN f.id AS id
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
                return Result<List<FollowingDTO>>.Success(new List<FollowingDTO>());
            }

            // Get detailed user information from stakeholders service
            var userDetails = await stakeholdersServiceClient.GetUsersByIdsAsync(userIds);

            // Convert to FollowingDTO with detailed information
            var followings = userDetails.Select(user => new FollowingDTO(
                user.Id,
                user.Username,
                user.Name,
                user.Email,
                user.ProfilePicture
            )).ToList();

            return Result<List<FollowingDTO>>.Success(followings);
        }
        catch (Exception e)
        {
            return Result<List<FollowingDTO>>.Failure(e.Message);
        }
    }
}
