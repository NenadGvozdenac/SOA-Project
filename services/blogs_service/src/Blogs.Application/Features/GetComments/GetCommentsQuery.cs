using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using MediatR;

namespace blogs_service.src.Blogs.Application.Features.GetComments;

public record GetCommentsQuery(string BlogId) : IRequest<Result<List<CommentDTO>>>;
