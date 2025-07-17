using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavigationPlatform.JourneyService.Infrastructure.Configuration;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;

public class RewardWorkerConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RewardWorkerConsumer> _logger;
    private readonly KafkaOptions _kafkaOptions;
    public RewardWorkerConsumer(IOptions<KafkaOptions> kafkaOptions, IKafkaClientFactory consumerFactory,
        ILogger<RewardWorkerConsumer> logger, IServiceProvider serviceProvider)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        _consumer = consumerFactory.CreateConsumer();
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_kafkaOptions.Topics.DailyGoalAchieved);
        _logger.LogInformation("Kafka consumer started, subscribed to topic: {Topic}", _kafkaOptions.Topics.DailyGoalAchieved);
        
        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IRewardWorkerProcessor>();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null)
                    {
                        await processor.ProcessAsync(consumeResult.Message.Value, stoppingToken);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }
    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

