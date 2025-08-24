using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.GetBlogById;

public record GetBlogByIdQuery(string BlogId, string? UserId = null) : IRequest<Result<BlogDetailDTO>>;
