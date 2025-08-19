using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.CreateBlog
{
  public record CreateBlogCommand(UserDTO UserDTO, string Title, string? DescriptionMarkdown, string? ImageBase64) : IRequest<Result<CreateBlogDTO>>;
}
