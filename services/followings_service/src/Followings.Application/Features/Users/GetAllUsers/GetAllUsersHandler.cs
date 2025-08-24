using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;
using MediatR;

namespace followings_service.src.Followings.Application.Features.Users.GetAllUsers;

public class GetAllUsersHandler(
    IStakeholdersServiceClient stakeholdersServiceClient
) : IRequestHandler<GetAllUsersQuery, Result<GetAllUsersResponse>>
{
    public async Task<Result<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all users from stakeholders service
            var userDetails = await stakeholdersServiceClient.GetAllUsersAsync();

            // Convert to our DTOs
            var users = userDetails.Select(user => new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Role = "User" // Default role, could be enhanced later
            }).ToList();

            var response = new GetAllUsersResponse
            {
                Data = users
            };

            return Result<GetAllUsersResponse>.Success(response);
        }
        catch (Exception e)
        {
            return Result<GetAllUsersResponse>.Failure(e.Message);
        }
    }
}
