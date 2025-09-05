using blogs_service.src.Blogs.Application.Features.GetBlogsByAuthors;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace blogs_service.src.Blogs.API.Controllers
{
    [ApiController]
    [Route("api/internal/blogs")]
    public class InternalBlogsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Route("by-authors")]
        public async Task<IActionResult> GetBlogsByAuthors([FromBody] BatchAuthorRequest request)
        {
            var result = await mediator.Send(new GetBlogsByAuthorsQuery(request.AuthorIds));
            
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Success",
                    Code = 200,
                    Data = result.Value
                });
            }
            
            return BadRequest(new
            {
                Message = result.Error,
                Code = 400,
                Data = (object?)null
            });
        }
    }
}
