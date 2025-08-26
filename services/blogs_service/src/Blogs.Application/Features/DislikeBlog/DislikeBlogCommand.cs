using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.DislikeBlog
{
    public record DislikeBlogCommand(UserDTO UserDTO, string BlogId) : IRequest<Result<bool>>;
}