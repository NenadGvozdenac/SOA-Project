using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Followers.GetMyFollowings;

public record GetMyFollowingsQuery(UserDTO UserDTO) : IRequest<Result<List<FollowingDTO>>>;
