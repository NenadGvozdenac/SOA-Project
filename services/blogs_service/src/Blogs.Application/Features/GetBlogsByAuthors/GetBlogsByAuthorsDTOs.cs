using System.Text.Json.Serialization;

namespace blogs_service.src.Blogs.Application.Features.GetBlogsByAuthors;

public record BlogsByAuthorsDTO(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("creationTime")] DateTime CreationTime,
    [property: JsonPropertyName("authorId")] string AuthorId
);

public record BatchAuthorRequest(
    [property: JsonPropertyName("authorIds")] List<string> AuthorIds
);
