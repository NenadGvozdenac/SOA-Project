using followings_service.src.Followings.Application.Features.Followers.FollowUser;
using followings_service.src.Followings.Application.Features.Followers.GetFollowSuggestions;
using followings_service.src.Followings.Application.Features.Followers.GetMyFollowers;
using followings_service.src.Followings.Application.Features.Followers.UnfollowUser;
using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace followings_service.src.Followings.API.Controllers.Followers;

[ApiController]
[Route("api/followers")]
[Authorize]
public class FollowersController(IMediator mediator) : BaseController
{

    [HttpPost("follow/{followerId}")]
    public async Task<Result> FollowUser(string followerId)
    {
        var result = await mediator.Send(new FollowUserCommand(this.GetUser(), followerId));
        return result;
    }

    [HttpPost("unfollow/{followerId}")]
    public async Task<Result> UnfollowUser(string followerId)
    {
        var result = await mediator.Send(new UnfollowUserCommand(this.GetUser(), followerId));
        return result;
    }

    [HttpGet("my")]
    public async Task<Result> GetMyFollowers()
    {
        var result = await mediator.Send(new GetMyFollowersQuery(this.GetUser()));
        return result;
    }

    [HttpGet("suggestions")]
    public async Task<Result> GetSuggestions()
    {
        var result = await mediator.Send(new GetFollowSuggestionsQuery(this.GetUser()));
        return result;
    }
}