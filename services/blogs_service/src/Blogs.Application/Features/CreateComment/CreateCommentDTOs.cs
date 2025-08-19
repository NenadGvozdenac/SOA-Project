namespace blogs_service.src.Blogs.Application.Features.CreateComment
{
  public class CreateCommentDTO
  {
    public string Id { get; set; }
    public string? Content { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CreateCommentDTO(string id, string? content, string userId, DateTime createdAt, DateTime updatedAt)
    {
      Id = id;
      Content = content;
      UserId = userId;
      CreatedAt = createdAt;
      UpdatedAt = updatedAt;
    }
  }
}
