namespace blogs_service.src.Blogs.Application.Features.LikeBlog
{
  public class LikeBlogDTO
  {
    public string Id { get; set; }
    public string? BlogId { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public LikeBlogDTO(string id, string? blogId, string userId, DateTime createdAt)
    {
      Id = id;
      BlogId = blogId;
      UserId = userId;
      CreatedAt = createdAt;
    }
  }
}