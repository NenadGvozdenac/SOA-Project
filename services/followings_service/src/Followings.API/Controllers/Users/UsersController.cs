using followings_service.src.Followings.Application.Features.Users.CreateUser;
using followings_service.src.Followings.Application.Features.Users.GetAllUsers;
using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace followings_service.src.Followings.API.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.UserId);
        var result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new { userId = result.Value });
        }

        return BadRequest(new { error = result.Error });
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { error = result.Error });
    }
}

public record CreateUserRequest(string UserId);
