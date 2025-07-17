using MediatR;
using NavigationPlatform.RewardService.Worker.Events;

namespace NavigationPlatform.RewardService.Worker.Commands;

public record ProcessJourneyCreatedCommand(JourneyCreated JourneyCreated) : IRequest;
