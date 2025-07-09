using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Users.CreateUser;

public record CreateUserCommand(string UserId) : IRequest<Result<string>>;
