using blogs_service.src.Blogs.Application.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using MediatR;
using MongoDB.Driver;

namespace blogs_service.src.Blogs.Application.Features.GetBlogsByAuthors;

public class GetBlogsByAuthorsHandler(IDocumentDatabaseContext context) : IRequestHandler<GetBlogsByAuthorsQuery, Result<List<BlogsByAuthorsDTO>>>
{
    public async Task<Result<List<BlogsByAuthorsDTO>>> Handle(GetBlogsByAuthorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.AuthorIds.Any())
            {
                return Result<List<BlogsByAuthorsDTO>>.Success(new List<BlogsByAuthorsDTO>());
            }

            // Get all blogs where userId is in the list of AuthorIds
            var blogsQueryable = await context.GetCollection<Blog>("blogs");
            var allBlogs = blogsQueryable.ToList();
            var blogs = allBlogs.Where(b => request.AuthorIds.Contains(b.UserId)).ToList();

            var blogDTOs = blogs.Select(blog => new BlogsByAuthorsDTO(
                blog.Id,
                blog.Title ?? string.Empty,
                blog.DescriptionMarkdown ?? string.Empty,
                blog.CreatedAt,
                blog.UserId
            ))
            .OrderByDescending(b => b.CreationTime)
            .ToList();

            return Result<List<BlogsByAuthorsDTO>>.Success(blogDTOs);
        }
        catch (Exception e)
        {
            return Result<List<BlogsByAuthorsDTO>>.Failure(e.Message);
        }
    }
}
