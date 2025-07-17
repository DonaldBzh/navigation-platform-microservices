using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavigationPlatform.JourneyService.Application.Commands.CreateJourney;
using NavigationPlatform.JourneyService.Application.Commands.DeleteJourney;
using NavigationPlatform.JourneyService.Application.Commands.RevokePublic;
using NavigationPlatform.JourneyService.Application.Commands.SharePublic;
using NavigationPlatform.JourneyService.Application.Commands.ShareWithUsers;
using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;
using NavigationPlatform.JourneyService.Application.Queries.GetSharedWithUser;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using System.Security.Claims;

namespace NavigationPlatform.JourneyService.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JourneysController : ControllerBase
{
    private readonly IMediator _mediator;

    public JourneysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JourneyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JourneyResponse>> Create(
        [FromBody] CreateJourneyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateJourneyCommand(request);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JourneyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JourneyResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetJourneyByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null) 
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<JourneyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<JourneyResponse>>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetJourneysQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteJourneyCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    //[Authorize(Roles = "Admin")]
    [HttpGet("admin/filtered-journeys")]
    [ProducesResponseType(typeof(List<Journey>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetJourneys([FromQuery] GetFilteredJourneysRequest request, CancellationToken cancellationToken)
    {
        var command = new GetFilteredJourneysQuery(request);
        
        var result = await _mediator.Send(command, cancellationToken);
        Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
        return Ok(result);
    }

   
    [HttpPost("{id}/share")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareJourney(Guid id, [FromBody] List<Guid> userIds)
    {
        await _mediator.Send(new ShareJourneyWithUsersCommand(id, userIds));
        return NoContent();
    }

   
    [HttpPost("{id:guid}/public-link")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePublicLink(Guid id, [FromBody] CreatePublicLinkRequest request)
    {
        var token = await _mediator.Send(new CreatePublicShareLinkCommand(id,  request.ExpiresAt));
        return Ok(new { Link = token });
    }

   
    [HttpDelete("{id}/revoke-public-link")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokePublicLink(Guid id)
    {
        await _mediator.Send(new RevokePublicJourneyCommand(id));
        return NoContent();
    }

    //[AllowAnonymous]
    [HttpGet("feed")]
    [ProducesResponseType(typeof(PublicJourneyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJourneysSharedWithUser()
    {
        var result = await _mediator.Send(new GetJourneysSharedWithUserQuery());
        return Ok(result);
    }

    
}
