using blogs_service.src.Blogs.API.DTOs;
using blogs_service.src.Blogs.Application.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using blogs_service.src.Blogs.BuildingBlocks.Infrastructure;
using blogs_service.src.Blogs.Application.Features.CreateBlog;
using blogs_service.src.Blogs.Application.Features.CreateComment;
using blogs_service.src.Blogs.Application.Features.UpdateComment;
using blogs_service.src.Blogs.Application.Features.LikeBlog;
using blogs_service.src.Blogs.Application.Features.DislikeBlog;
using blogs_service.src.Blogs.Application.Features.GetAllBlogs;
using blogs_service.src.Blogs.Application.Features.GetBlogById;
using blogs_service.src.Blogs.Application.Features.GetComments;
using Microsoft.AspNetCore.Authorization;
using APIBlogDTO = blogs_service.src.Blogs.API.DTOs.BlogDTO;
using APICommentDTO = blogs_service.src.Blogs.API.DTOs.CommentDTO;
using System.Drawing.Printing;

namespace blogs_service.src.Blogs.API.Controllers
{
    [ApiController]
    [Route("api/blogs")]
    [Authorize]
    public class BlogsController(IMediator mediator) : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetBlogById(string id)
        {
            var currentUser = this.GetUser(); // Get current user for like status
            var result = await mediator.Send(new GetBlogByIdQuery(id, currentUser?.Id));
            return CreateResponse(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs(int pageNumber = 1, int pageSize = 10)
        {
            Console.WriteLine($"GetBlogs called with pageNumber={pageNumber}, pageSize={pageSize}");
            var currentUser = this.GetUser(); // Get current user for like status
            var result = await mediator.Send(new GetAllBlogsQuery(pageNumber, pageSize, currentUser?.Id));
            return CreateResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] APIBlogDTO blogDTO)
        {
            if (string.IsNullOrEmpty(blogDTO.Title))
            {
                return BadRequest("Title is required");
            }

            var result = await mediator.Send(new CreateBlogCommand(this.GetUser(), blogDTO.Title, blogDTO.DescriptionMarkdown, blogDTO.ImageBase64));
            return CreateResponse(result);
        }

        [HttpPost]
        [Route("comment")]
        public async Task<IActionResult> CreateComment([FromBody] APICommentDTO commentDTO)
        {
            var result = await mediator.Send(new CreateCommentCommand(this.GetUser(), commentDTO.BlogId, commentDTO.Content));
            return CreateResponse(result);
        }

        [HttpGet]
        [Route("{blogId}/comments")]
        public async Task<IActionResult> GetComments(string blogId)
        {
            var result = await mediator.Send(new GetCommentsQuery(blogId));
            return CreateResponse(result);
        }

        [HttpPut]
        [Route("comment/update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdatedCommentDTO updatedCommentDTO)
        {
            Console.WriteLine("=== UPDATE COMMENT ENDPOINT HIT ===");
            Console.WriteLine($"DateTime: {DateTime.Now}");
            
            if (updatedCommentDTO == null)
            {
                Console.WriteLine("ERROR: updatedCommentDTO is NULL");
                return BadRequest("Request body is required");
            }
            
            Console.WriteLine($"CommentId: '{updatedCommentDTO.CommentId}'");
            Console.WriteLine($"Content: '{updatedCommentDTO.Content}'");
            Console.WriteLine("=====================================");
            
            var result = await mediator.Send(new UpdateCommentCommand(this.GetUser(), updatedCommentDTO.CommentId, updatedCommentDTO.Content));
            return CreateResponse(result);
        }

        [HttpPost]
        [Route("like/{blogId}")]
        public async Task<IActionResult> LikeBlog(string blogId)
        {
            var result = await mediator.Send(new LikeBlogCommand(this.GetUser(), blogId));
            return CreateResponse(result);
        }

        [HttpDelete]
        [Route("dislike/{blogId}")]
        public async Task<IActionResult> DislikeBlog(string blogId)
        {
            var result = await mediator.Send(new DislikeBlogCommand(this.GetUser(), blogId));
            return CreateResponse(result);
        }
    }
}
