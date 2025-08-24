using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.GetBlogsByAuthors;

public record GetBlogsByAuthorsQuery(List<string> AuthorIds) : IRequest<Result<List<BlogsByAuthorsDTO>>>;
