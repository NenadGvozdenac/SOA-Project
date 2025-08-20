using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace tours_service.src.Tours.BuildingBlocks.Infrastructure.StakeholdersService;

public class StakeholdersServiceClient : IStakeholdersServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<StakeholdersServiceClient> _logger;

    public StakeholdersServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<StakeholdersServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Services:Stakeholders:Url"] 
            ?? throw new ArgumentNullException("Services:Stakeholders:Url", "Stakeholders service URL not configured");
        
        // Set timeout for HTTP client
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<UserDetailsDTO>> GetUsersByIdsAsync(List<string> userIds)
    {
        try
        {
            _logger.LogInformation("Fetching user details for {UserCount} users from stakeholders service", userIds.Count);
            
            var request = new BatchUserRequest(userIds);
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/users/batch", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to fetch users from stakeholders service. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorResponse);
                return new List<UserDetailsDTO>();
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw response from stakeholders service: {Response}", responseContent);
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserDetailsDTO>>>(responseContent, jsonOptions);
            var users = apiResponse?.Data ?? new List<UserDetailsDTO>();
            
            _logger.LogInformation("Successfully fetched {UserCount} user details from stakeholders service", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users from stakeholders service");
            return new List<UserDetailsDTO>();
        }
    }
}
