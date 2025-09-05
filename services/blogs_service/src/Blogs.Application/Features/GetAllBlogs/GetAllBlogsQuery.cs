using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.GetAllBlogs;

public record GetAllBlogsQuery(int PageNumber, int PageSize, string? UserId = null) : IRequest<Result<List<BlogDTO>>>;
