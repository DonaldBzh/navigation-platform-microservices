using MediatR;
using Microsoft.AspNetCore.Mvc;
using NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;

namespace NavigationPlatform.JourneyService.Api.Controllers
{
    [ApiController]
    [Route("api/journeys/public")]
    public class PublicJourneyController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PublicJourneyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{token}")]
        [ProducesResponseType(typeof(PublicJourneyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> GetJourneyByPublicToken(string token)
        {
            var result = await _mediator.Send(new GetPublicJourneyQuery(token));
            return Ok(result);
        }
    }
}
