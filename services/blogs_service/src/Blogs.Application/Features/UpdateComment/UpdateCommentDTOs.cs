namespace blogs_service.src.Blogs.Application.Features.UpdateComment
{
  public class UpdateCommentDTO
  {
    public string Id { get; set; }
    public string? Content { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UpdateCommentDTO(string id, string? content, string userId, DateTime createdAt, DateTime updatedAt)
    {
      Id = id;
      Content = content;
      UserId = userId;
      CreatedAt = createdAt;
      UpdatedAt = updatedAt;
    }
  }
}