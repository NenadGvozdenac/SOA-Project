using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.CreateComment
{
  public record CreateCommentCommand(UserDTO UserDTO, string BlogId, string? Content) : IRequest<Result<CreateCommentDTO>>;
} 