using MediatR;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;

namespace NavigationPlatform.JourneyService.Application.Commands.CreateJourney;

public class CreateJourneyCommand : IRequest<JourneyResponse>
{
    public CreateJourneyRequest Data { get; set; }
    public CreateJourneyCommand(CreateJourneyRequest filter)
    {
        Data = filter;
    }
}


