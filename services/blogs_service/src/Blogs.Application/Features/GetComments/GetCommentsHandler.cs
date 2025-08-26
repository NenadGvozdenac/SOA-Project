using blogs_service.src.Blogs.Application.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using MongoDB.Driver;

namespace blogs_service.src.Blogs.Application.Features.GetComments;

public class GetCommentsHandler(IDocumentDatabaseContext context) : IRequestHandler<GetCommentsQuery, Result<List<CommentDTO>>>
{
    public async Task<Result<List<CommentDTO>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var commentsCollection = await context.GetCollection<Comment>("comments");
            var comments = commentsCollection
                .Where(c => c.BlogId == request.BlogId)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var commentDTOs = comments.Select(comment => new CommentDTO(
                comment.Id,
                comment.BlogId,
                comment.UserId,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt
            )).ToList();

            return Result<List<CommentDTO>>.Success(commentDTOs);
        }
        catch (Exception e)
        {
            return Result<List<CommentDTO>>.Failure(e.Message);
        }
    }
}
