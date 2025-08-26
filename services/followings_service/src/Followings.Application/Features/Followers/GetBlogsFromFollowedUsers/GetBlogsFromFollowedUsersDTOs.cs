namespace followings_service.src.Followings.Application.Features.Followers.GetBlogsFromFollowedUsers;

public record BlogFromFollowedUserDTO(
    string Id,
    string Title,
    string Description,
    DateTime CreationTime,
    string AuthorId,
    string AuthorUsername,
    string AuthorName,
    string? AuthorProfilePicture
);
