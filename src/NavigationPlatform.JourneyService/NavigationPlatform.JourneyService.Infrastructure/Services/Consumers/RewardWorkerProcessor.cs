using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.JourneyService.Application.Commands.ProcessDailyGoal;
using NavigationPlatform.JourneyService.Domain.Events;
using System.Text.Json;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;

public class RewardWorkerProcessor : IRewardWorkerProcessor
{
    private readonly IMediator _mediator;
    private readonly ILogger<RewardWorkerProcessor> _logger;

    public RewardWorkerProcessor(IMediator mediator, ILogger<RewardWorkerProcessor> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var daiylyGoalAchieved = JsonSerializer.Deserialize<DailyGoalAchieved>(message);

            if (daiylyGoalAchieved == null)
                return;
            
            var command = new ProcessDailyGoalAchievedCommand(daiylyGoalAchieved);
            await _mediator.Send(command);

            _logger.LogInformation("Successfully processed DailyGoalAchieved event for user {UserId}, journey {JourneyId}",
                daiylyGoalAchieved.UserId, daiylyGoalAchieved.TriggeringJourneyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Kafka message: {Message}", message);
        }
    }
}