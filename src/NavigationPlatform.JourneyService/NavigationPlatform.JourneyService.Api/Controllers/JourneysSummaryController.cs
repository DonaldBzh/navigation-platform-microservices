using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavigationPlatform.JourneyService.Application.Queries.GetMonthlyDistance;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JourneysSummaryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JourneysSummaryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("monthly-distances")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(PaginatedResult<MonthlyUserDistance>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMonthlyDistances([FromQuery] GetMonthlyRouteDistancesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
