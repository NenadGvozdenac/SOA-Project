using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.Application.Features.CreateTour;
using tours_service.src.Tours.Application.Features.GetAllTours;
using tours_service.src.Tours.Application.Features.CreateTourReview;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.Application.Features.CreateCheckpoint;
using tours_service.src.Tours.Application.Features.GetAllCheckpoints;
using tours_service.src.Tours.Application.Features.UpdateCheckpoint;
using tours_service.src.Tours.Application.Features.DeleteCheckpoint;

namespace tours_service.src.Tours.API.Controllers;

[ApiController]
[Route("api/tours")]
[Authorize]
public class ToursController(IMediator _mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateTour([FromBody] CreatedTourDTO createdTourDTO)
    {
        var result = await _mediator.Send(new CreateTourCommand(this.GetUser(), createdTourDTO));
        return CreateResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllToursQuery(this.GetUser()));
        return CreateResponse(result);
    }

    [HttpPost]
    [Route("review")]
    public async Task<IActionResult> CreateTourReview([FromBody] TourReviewDTO tourReviewDTO)
    {
        var result = await _mediator.Send(new CreateTourReviewCommand(this.GetUser(), tourReviewDTO));
        return CreateResponse(result);
    }

    [HttpPost]
    [Route("checkpoint")]
    public async Task<IActionResult> CreateCheckpoint([FromBody] CreatedCheckpointDTO checkpointDTO)
    {
        var result = await _mediator.Send(new CreateCheckpointCommand(this.GetUser(), checkpointDTO));
        return CreateResponse(result);
    }

    [HttpGet]
    [Route("checkpoints/{tourId}")]
    public async Task<IActionResult> GetCheckpoints(long tourId)
    {
        var result = await _mediator.Send(new GetAllCheckpointsQuery(this.GetUser(), tourId));
        return CreateResponse(result);
    }

    [HttpPut]
    [Route("checkpoint/update/{checkpointId}")]
    public async Task<IActionResult> UpdateCheckpoint(long checkpointId, [FromBody] CreatedCheckpointDTO checkpointDTO)
    {
        var result = await _mediator.Send(new UpdateCheckpointCommand(this.GetUser(), checkpointId, checkpointDTO));
        return CreateResponse(result);
    }

    [HttpDelete]
    [Route("checkpoint/delete/{checkpointId}")]
    public async Task<IActionResult> DeleteCheckpoint(long checkpointId)
    {
        var result = await _mediator.Send(new DeleteCheckpointCommand(this.GetUser(), checkpointId));
        return CreateResponse(result);
    }
}
