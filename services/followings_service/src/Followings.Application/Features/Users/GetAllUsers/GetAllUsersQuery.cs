using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Users.GetAllUsers;

public class GetAllUsersQuery : IRequest<Result<GetAllUsersResponse>>
{
}
