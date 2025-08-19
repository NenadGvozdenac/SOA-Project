using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.Application.Domain;

namespace blogs_service.src.Blogs.Application.Features.LikeBlog
{
    public class DislikeBlogHandler(IDocumentDatabaseContext documentDatabaseContext) : IRequestHandler<DislikeBlogCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DislikeBlogCommand request, CancellationToken cancellationToken)
        {
            var keys = new Dictionary<string, object>
            {
                { "BlogId", request.BlogId },
                { "UserId", request.UserDTO.Id }
            };
            var existingLike = await documentDatabaseContext.GetDocumentByKeys<Like>("likes", keys);
            if (existingLike == null)
            {
                return Result<bool>.Failure("You have not liked this blog.");
            }

            await documentDatabaseContext.DeleteDocument<Like>("likes", existingLike.Id);
            return Result<bool>.Success(true);
        }
    }
}