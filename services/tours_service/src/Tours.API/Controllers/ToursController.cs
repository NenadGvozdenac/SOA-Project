using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.Application.Features.CreateTour;
using tours_service.src.Tours.Application.Features.GetAllTours;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;

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
}
