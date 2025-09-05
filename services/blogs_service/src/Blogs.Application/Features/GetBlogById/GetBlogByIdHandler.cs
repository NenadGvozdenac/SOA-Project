using blogs_service.src.Blogs.Application.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using MongoDB.Driver;

namespace blogs_service.src.Blogs.Application.Features.GetBlogById;

public class GetBlogByIdHandler(IDocumentDatabaseContext context) : IRequestHandler<GetBlogByIdQuery, Result<BlogDetailDTO>>
{
    public async Task<Result<BlogDetailDTO>> Handle(GetBlogByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var blog = await context.GetDocumentById<Blog>("blogs", request.BlogId);
            
            if (blog == null)
            {
                return Result<BlogDetailDTO>.Failure("Blog not found");
            }

            // Get like information for this blog
            var likesCollection = await context.GetCollection<Like>("likes");
            var blogLikes = likesCollection.Where(l => l.BlogId == request.BlogId).ToList();
            var likesCount = blogLikes.Count;
            var isLikedByCurrentUser = !string.IsNullOrEmpty(request.UserId) && 
                                     blogLikes.Any(l => l.UserId == request.UserId);

            var blogDetailDTO = new BlogDetailDTO(
                blog.Id,
                blog.Title ?? string.Empty,
                blog.DescriptionMarkdown ?? string.Empty,
                blog.ImageBase64,
                blog.CreatedAt,
                blog.UserId,
                likesCount,
                isLikedByCurrentUser
            );

            return Result<BlogDetailDTO>.Success(blogDetailDTO);
        }
        catch (Exception e)
        {
            return Result<BlogDetailDTO>.Failure(e.Message);
        }
    }
}
