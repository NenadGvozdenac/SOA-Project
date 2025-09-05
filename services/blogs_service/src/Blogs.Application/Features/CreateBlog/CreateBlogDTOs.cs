namespace blogs_service.src.Blogs.Application.Features.CreateBlog
{
  public class CreateBlogDTO
  {
    public string Id { get; set; }
    public string? Title { get; set; }
    public string UserId { get; set; }
    public string? DescriptionMarkdown { get; set; }
    public string? ImageBase64 { get; set; }
    public DateTime CreatedAt { get; set; }

    public CreateBlogDTO(string id, string? title, string userId, string? descriptionMarkdown, string? imageBase64, DateTime createdAt)
    {
      Id = id;
      Title = title;
      UserId = userId;
      DescriptionMarkdown = descriptionMarkdown;
      ImageBase64 = imageBase64;
      CreatedAt = createdAt;
    }
  }
}
