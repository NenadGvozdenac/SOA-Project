using System.Text.Json.Serialization;

namespace tours_service.src.Tours.BuildingBlocks.Infrastructure.StakeholdersService;

public interface IStakeholdersServiceClient
{
    Task<List<UserDetailsDTO>> GetUsersByIdsAsync(List<string> userIds);
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
