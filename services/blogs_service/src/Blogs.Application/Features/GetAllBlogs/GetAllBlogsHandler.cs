using blogs_service.src.Blogs.Application.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using MongoDB.Driver;

namespace blogs_service.src.Blogs.Application.Features.GetAllBlogs;

public class GetAllBlogsHandler(IDocumentDatabaseContext context) : IRequestHandler<GetAllBlogsQuery, Result<List<BlogDTO>>>
{
    public async Task<Result<List<BlogDTO>>> Handle(GetAllBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"GetAllBlogsHandler: Getting blogs with pageNumber={request.PageNumber}, pageSize={request.PageSize}");
            
            // Get all blogs with pagination
            var blogsQueryable = await context.GetCollection<Blog>("blogs");
            var allBlogs = blogsQueryable.ToList();
            
            Console.WriteLine($"GetAllBlogsHandler: Found {allBlogs.Count} total blogs in database");
            
            var blogs = allBlogs
                .OrderByDescending(b => b.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            Console.WriteLine($"GetAllBlogsHandler: After pagination, returning {blogs.Count} blogs");

            // Get all likes for performance optimization
            var likesCollection = await context.GetCollection<Like>("likes");
            var allLikes = likesCollection.ToList();

            var blogDTOs = blogs.Select(blog => 
            {
                // Get likes for this blog
                var blogLikes = allLikes.Where(l => l.BlogId == blog.Id).ToList();
                var likesCount = blogLikes.Count;
                var isLikedByCurrentUser = !string.IsNullOrEmpty(request.UserId) && 
                                         blogLikes.Any(l => l.UserId == request.UserId);

                return new BlogDTO(
                    blog.Id,
                    blog.Title ?? string.Empty,
                    blog.DescriptionMarkdown ?? string.Empty,
                    blog.ImageBase64,
                    blog.CreatedAt,
                    blog.UserId,
                    likesCount,
                    isLikedByCurrentUser
                );
            }).ToList();

            Console.WriteLine($"GetAllBlogsHandler: Successfully created {blogDTOs.Count} DTOs with like information");
            return Result<List<BlogDTO>>.Success(blogDTOs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"GetAllBlogsHandler: Error occurred - {e.Message}");
            Console.WriteLine($"GetAllBlogsHandler: Stack trace - {e.StackTrace}");
            return Result<List<BlogDTO>>.Failure(e.Message);
        }
    }
}
