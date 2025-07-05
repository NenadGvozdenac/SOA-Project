using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Followers.FollowUser;

public record FollowUserCommand(UserDTO UserDTO, string FollowerId) : IRequest<Result<FollowUserDTO>>;