using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.Application.Domain;

namespace blogs_service.src.Blogs.Application.Features.CreateBlog
{
  public class CreateBlogHandler(IDocumentDatabaseContext documentDatabaseContext) : IRequestHandler<CreateBlogCommand, Result<CreateBlogDTO>>
  {
    public async Task<Result<CreateBlogDTO>> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
      var blog = new Blog
      {
        Title = request.Title,
        UserId = request.UserDTO.Id,
        DescriptionMarkdown = request.DescriptionMarkdown,
        ImageBase64 = request.ImageBase64
      };

      await documentDatabaseContext.AddDocument("blogs_db", blog);

      return Result<CreateBlogDTO>.Success(new CreateBlogDTO(
        blog.Id,
        blog.Title,
        blog.UserId,
        blog.DescriptionMarkdown,
        blog.ImageBase64,
        DateTime.UtcNow
      ));
    }
  }
}
