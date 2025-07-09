namespace followings_service.src.Followings.Application.Features.Followers.GetFollowSuggestions;

public record FollowerDTO(
    string Id,
    string Username,
    string Name,
    string Email,
    string? ProfilePicture
);