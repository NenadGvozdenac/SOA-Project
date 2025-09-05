using System.Text.Json.Serialization;

namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.BlogsService;

public interface IBlogsServiceClient
{
    Task<List<BlogDetailsDTO>> GetBlogsByAuthorIdsAsync(List<string> authorIds);
}

public record BlogDetailsDTO(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("creationTime")] DateTime CreationTime,
    [property: JsonPropertyName("authorId")] string AuthorId
);

public record BatchAuthorRequest(
    [property: JsonPropertyName("authorIds")] List<string> AuthorIds
);
