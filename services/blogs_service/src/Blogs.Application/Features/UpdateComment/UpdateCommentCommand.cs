using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.UpdateComment
{
    public record UpdateCommentCommand(UserDTO UserDTO, string CommentId, string? Content) : IRequest<Result<UpdateCommentDTO>>;
} 