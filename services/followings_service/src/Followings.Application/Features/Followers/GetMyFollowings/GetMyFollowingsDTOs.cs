namespace followings_service.src.Followings.Application.Features.Followers.GetMyFollowings;

public record FollowingDTO(
    string Id,
    string Username,
    string Name,
    string Email,
    string? ProfilePicture
);
