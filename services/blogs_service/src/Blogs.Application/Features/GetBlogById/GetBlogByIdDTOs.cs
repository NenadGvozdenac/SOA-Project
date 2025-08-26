using System.Text.Json.Serialization;

namespace blogs_service.src.Blogs.Application.Features.GetBlogById;

public class BlogDetailDTO
{
  public string Id { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string DescriptionMarkdown { get; set; } = string.Empty;
  public string? ImageBase64 { get; set; }
  public DateTime CreatedAt { get; set; }
  public string UserId { get; set; } = string.Empty;
  public int LikesCount { get; set; } = 0;
  public bool IsLikedByCurrentUser { get; set; } = false;

  public BlogDetailDTO(string id, string title, string descriptionMarkdown, string? imageBase64, DateTime createdAt, string userId, int likesCount = 0, bool isLikedByCurrentUser = false)
  {
    Id = id;
    Title = title;
    DescriptionMarkdown = descriptionMarkdown;
    ImageBase64 = imageBase64;
    CreatedAt = createdAt;
    UserId = userId;
    LikesCount = likesCount;
    IsLikedByCurrentUser = isLikedByCurrentUser;
  }
}
