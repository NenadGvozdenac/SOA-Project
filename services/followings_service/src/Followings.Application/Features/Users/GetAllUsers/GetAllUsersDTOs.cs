using followings_service.src.Followings.BuildingBlocks.Infrastructure;

namespace followings_service.src.Followings.Application.Features.Users.GetAllUsers;

public class GetAllUsersResponse
{
    public List<UserDTO> Data { get; set; } = new();
}

public class UserDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string Role { get; set; } = string.Empty;
}
