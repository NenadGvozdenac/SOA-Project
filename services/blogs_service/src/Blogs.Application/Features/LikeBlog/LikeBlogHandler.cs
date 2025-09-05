using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.Application.Domain;

namespace blogs_service.src.Blogs.Application.Features.LikeBlog
{
    public class LikeBlogHandler(IDocumentDatabaseContext documentDatabaseContext) : IRequestHandler<LikeBlogCommand, Result<LikeBlogDTO>>
    {
        
        public async Task<Result<LikeBlogDTO>> Handle(LikeBlogCommand request, CancellationToken cancellationToken)
        {
            var blog = await documentDatabaseContext.GetDocumentById<Blog>("blogs", request.BlogId);
            if (blog == null)
            {
                return Result<LikeBlogDTO>.Failure("Blog does not exist.");
            }
            
            var keys = new Dictionary<string, object>
            {
                { "BlogId", request.BlogId },
                { "UserId", request.UserDTO.Id }
            };
            var existingLike = await documentDatabaseContext.GetDocumentByKeys<Like>("likes", keys);
            if (existingLike != null)
            {
                return Result<LikeBlogDTO>.Failure("You have already liked this blog.");
            }

            var like = new Like
            {
                BlogId = request.BlogId,
                UserId = request.UserDTO.Id
            };

            await documentDatabaseContext.AddDocument("likes", like);

            return Result<LikeBlogDTO>.Success(new LikeBlogDTO(
                like.Id,
                like.BlogId,
                like.UserId,
                DateTime.UtcNow
            ));
        }
    }
}

