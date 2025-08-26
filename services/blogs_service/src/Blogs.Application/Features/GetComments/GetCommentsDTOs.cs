using System.Text.Json.Serialization;

namespace blogs_service.src.Blogs.Application.Features.GetComments;

public class CommentDTO
{
    public string Id { get; set; } = string.Empty;
    public string BlogId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CommentDTO(string id, string blogId, string userId, string content, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        BlogId = blogId;
        UserId = userId;
        Content = content;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
