using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Followers.GetBlogsFromFollowedUsers;

public record GetBlogsFromFollowedUsersQuery(UserDTO UserDTO) : IRequest<Result<List<BlogFromFollowedUserDTO>>>;
