namespace followings_service.src.Followings.Application.Features.Followers.FollowUser;

public record FollowUserDTO(string UserId, string FollowerId, bool Followed);