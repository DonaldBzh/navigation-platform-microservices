using MediatR;

namespace NavigationPlatform.JourneyService.Application.Commands.DeleteJourney;

public class DeleteJourneyCommand : IRequest
{
    public Guid Id { get; set; }
}

