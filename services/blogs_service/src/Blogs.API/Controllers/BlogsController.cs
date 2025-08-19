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
using Microsoft.AspNetCore.Authorization;

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
            // var blog = await _documentDatabaseContext.GetDocumentById<Blog>("Blogs", id);
            // return Ok(blog);
            return Ok(new { message = "This endpoint is not implemented yet." });
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs(int pageNumber = 1, int pageSize = 10)
        {
            // var blogs = await _documentDatabaseContext.GetCollection<Blog>("Blogs", pageNumber, pageSize);
            // return Ok(blogs);
            return Ok(new { message = "This endpoint is not implemented yet." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] BlogDTO blogDTO)
        {
            var result = await mediator.Send(new CreateBlogCommand(this.GetUser(), blogDTO.Title, blogDTO.DescriptionMarkdown, blogDTO.ImageBase64));
            return CreateResponse(result);

            // PREBACIVANJE SLIKE U DOBAR FORMAT NA FRONTU I SLANJE KA BACKU
            // function handleImageChange(event) {
            // const file = event.target.files[0];
            // const reader = new FileReader();
            // reader.onloadend = () => {
            //     // Dobijaš base64 string bez prefiksa
            //     const base64String = reader.result.split(',')[1];
            //     // Sada možeš da ga staviš u telo zahteva
            //     const blogData = {
            //     title: "Naslov",
            //     descriptionMarkdown: "Opis",
            //     imageBase64: base64String
            //     };
            //     // Pošalji blogData kao JSON u POST/PUT zahtev
            // };
            // reader.readAsDataURL(file);
            // }

            // PRIKAZ NA FRONTU
            // const base64String = "iVBORw0KGgoAAAANSUhEUgAA..."; // iz baze

            // <img src={`data:image/png;base64,${base64String}`} alt="Blog slika" />
        }

        [HttpPost]
        [Route("comment")]
        public async Task<IActionResult> CreateComment([FromBody] CommentDTO commentDTO)
        {
            var result = await mediator.Send(new CreateCommentCommand(this.GetUser(), commentDTO.BlogId, commentDTO.Content));
            return CreateResponse(result);
        }

        [HttpPut]
        [Route("comment/update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdatedCommentDTO upadtedCommentDTO)
        {
            var result = await mediator.Send(new UpdateCommentCommand(this.GetUser(), upadtedCommentDTO.CommentId, upadtedCommentDTO.Content));
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
