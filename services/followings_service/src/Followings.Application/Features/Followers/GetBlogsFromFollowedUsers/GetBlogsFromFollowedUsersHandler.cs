using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.BlogsService;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Followers.GetBlogsFromFollowedUsers;

public class GetBlogsFromFollowedUsersHandler(
    IGraphDatabaseContext context,
    IStakeholdersServiceClient stakeholdersServiceClient,
    IBlogsServiceClient blogsServiceClient
) : IRequestHandler<GetBlogsFromFollowedUsersQuery, Result<List<BlogFromFollowedUserDTO>>>
{
    public async Task<Result<List<BlogFromFollowedUserDTO>>> Handle(GetBlogsFromFollowedUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the list of users that the current user follows
            var followingQuery = @"
                MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
                RETURN f.id AS id
            ";

            var followingParameters = new Dictionary<string, object>
            {
                { "userId", request.UserDTO.Id }
            };

            var followingResultCursor = await context.RunAsync(followingQuery, followingParameters);
            var followingResult = await followingResultCursor.ToListAsync(cancellationToken);

            // Extract user IDs from Neo4j results
            var followedUserIds = followingResult.Select(r => r["id"].As<string>()).ToList();

            if (!followedUserIds.Any())
            {
                return Result<List<BlogFromFollowedUserDTO>>.Success(new List<BlogFromFollowedUserDTO>());
            }

            // Get blogs from those users
            var blogs = await blogsServiceClient.GetBlogsByAuthorIdsAsync(followedUserIds);

            if (!blogs.Any())
            {
                return Result<List<BlogFromFollowedUserDTO>>.Success(new List<BlogFromFollowedUserDTO>());
            }

            // Get detailed user information for all authors
            var authorIds = blogs.Select(b => b.AuthorId).Distinct().ToList();
            var authorDetails = await stakeholdersServiceClient.GetUsersByIdsAsync(authorIds);

            // Create a dictionary for quick lookup of author details
            var authorDetailsDict = authorDetails.ToDictionary(a => a.Id, a => a);

            // Combine blog data with author details
            var blogsFromFollowedUsers = blogs.Select(blog =>
            {
                var author = authorDetailsDict.GetValueOrDefault(blog.AuthorId);
                return new BlogFromFollowedUserDTO(
                    blog.Id,
                    blog.Title,
                    blog.Description,
                    blog.CreationTime,
                    blog.AuthorId,
                    author?.Username ?? "Unknown",
                    author?.Name ?? "Unknown",
                    author?.ProfilePicture
                );
            }).OrderByDescending(b => b.CreationTime).ToList();

            return Result<List<BlogFromFollowedUserDTO>>.Success(blogsFromFollowedUsers);
        }
        catch (Exception e)
        {
            return Result<List<BlogFromFollowedUserDTO>>.Failure(e.Message);
        }
    }
}
