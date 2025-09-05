using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.BlogsService;

public class BlogsServiceClient : IBlogsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<BlogsServiceClient> _logger;

    public BlogsServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<BlogsServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Services:Blogs:Url"] 
            ?? throw new ArgumentNullException("Services:Blogs:Url", "Blogs service URL not configured");
        
        // Set timeout for HTTP client
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<BlogDetailsDTO>> GetBlogsByAuthorIdsAsync(List<string> authorIds)
    {
        try
        {
            _logger.LogInformation("Fetching blogs for {AuthorCount} authors from blogs service", authorIds.Count);
            
            var request = new BatchAuthorRequest(authorIds);
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/internal/blogs/by-authors", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to fetch blogs from blogs service. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorResponse);
                return new List<BlogDetailsDTO>();
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw response from blogs service: {Response}", responseContent);
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BlogDetailsDTO>>>(responseContent, jsonOptions);
            var blogs = apiResponse?.Data ?? new List<BlogDetailsDTO>();
            
            _logger.LogInformation("Successfully fetched {BlogCount} blogs from blogs service", blogs.Count);
            return blogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching blogs from blogs service");
            return new List<BlogDetailsDTO>();
        }
    }
}
