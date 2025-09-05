using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.Application.Domain;

namespace blogs_service.src.Blogs.Application.Features.UpdateComment
{
    public class UpdateCommentHandler(IDocumentDatabaseContext documentDatabaseContext) : IRequestHandler<UpdateCommentCommand, Result<UpdateCommentDTO>>
    {
        public async Task<Result<UpdateCommentDTO>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await documentDatabaseContext.GetDocumentById<Comment>("comments", request.CommentId);
            if (comment == null)
            {
                return Result<UpdateCommentDTO>.Failure("Comment does not exist.");
            }

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await documentDatabaseContext.UpdateDocument("comments", comment);

            return Result<UpdateCommentDTO>.Success(new UpdateCommentDTO(
                comment.Id,
                comment.Content,
                comment.UserId,
                DateTime.UtcNow,
                comment.UpdatedAt
            ));
        }
    }
}

