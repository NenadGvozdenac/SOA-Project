using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.Application.Domain;

namespace blogs_service.src.Blogs.Application.Features.CreateComment
{
  public class CreateCommentHandler(IDocumentDatabaseContext documentDatabaseContext) : IRequestHandler<CreateCommentCommand, Result<CreateCommentDTO>>
  {
    public async Task<Result<CreateCommentDTO>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var blog = await documentDatabaseContext.GetDocumentById<Blog>("blogs", request.BlogId);
        if (blog == null)
        {
            return Result<CreateCommentDTO>.Failure("Blog does not exist.");
        }
        var comment = new Comment
        {
            BlogId = request.BlogId,
            UserId = request.UserDTO.Id,
            Content = request.Content,
            UpdatedAt = DateTime.UtcNow
        };

      await documentDatabaseContext.AddDocument("comments", comment);

        return Result<CreateCommentDTO>.Success(new CreateCommentDTO(
            comment.Id,
            comment.Content,
            comment.UserId,
            DateTime.UtcNow,
            comment.UpdatedAt
        ));
    }
  }
}
