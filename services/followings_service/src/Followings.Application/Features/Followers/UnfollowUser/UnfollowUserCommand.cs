using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Followers.UnfollowUser;

public record UnfollowUserCommand(UserDTO UserDTO, string FollowerId) : IRequest<Result<UnfollowUserDTO>>;