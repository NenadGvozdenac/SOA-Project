namespace followings_service.src.Followings.Application.Features.Followers.UnfollowUser;

public record UnfollowUserDTO(string UserId, string FollowerId, bool Followed);