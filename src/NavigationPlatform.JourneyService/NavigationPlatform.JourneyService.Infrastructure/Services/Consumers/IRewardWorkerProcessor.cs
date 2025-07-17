using Confluent.Kafka;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;

public interface IRewardWorkerProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
