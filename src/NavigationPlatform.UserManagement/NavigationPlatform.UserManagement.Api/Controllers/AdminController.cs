using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;
using System.Security.Claims;

namespace NavigationPlatform.UserManagement.Api.Controllers;

[ApiController]
[Route("api/admin")]
//[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("users/{id}/status")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUserStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request)
    {
        var command = new UpdateUserStatusCommand(id,request.Status,request.Reason);

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}