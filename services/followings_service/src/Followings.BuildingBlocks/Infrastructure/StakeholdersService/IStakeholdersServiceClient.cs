using System.Text.Json.Serialization;

namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;

public interface IStakeholdersServiceClient
{
    Task<List<UserDetailsDTO>> GetUsersByIdsAsync(List<string> userIds);
    Task<List<UserDetailsDTO>> GetAllUsersAsync();
}

public record UserDetailsDTO(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("profilePicture")] string? ProfilePicture
);

public record BatchUserRequest(
    [property: JsonPropertyName("userIds")] List<string> UserIds
);
