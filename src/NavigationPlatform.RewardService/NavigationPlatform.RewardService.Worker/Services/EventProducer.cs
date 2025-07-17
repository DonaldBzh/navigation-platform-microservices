using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NavigationPlatform.RewardService.Worker.Configuration;
using NavigationPlatform.RewardService.Worker.Events;
using System.Text.Json;

namespace NavigationPlatform.RewardService.Worker.Services;

public class EventProducer : IEventProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<EventProducer> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public EventProducer(IOptions<KafkaOptions> kafkaOptions, ILogger<EventProducer> logger)
    {
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            MessageSendMaxRetries = _kafkaOptions.Producer.Retries,
            EnableIdempotence = _kafkaOptions.Producer.EnableIdempotence,
            MessageTimeoutMs = _kafkaOptions.Producer.MessageTimeoutMs
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishDailyGoalAchievedAsync(DailyGoalAchieved eventData)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = $"user-{eventData.UserId}",
                Value = JsonSerializer.Serialize(eventData),
                Headers = new Headers
                    {
                        { "eventType", System.Text.Encoding.UTF8.GetBytes("DailyGoalAchieved") },
                        { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) }
                    }
            };

            var result = await _producer.ProduceAsync(_kafkaOptions.Topics.DailyGoalAchieved, message);
            _logger.LogInformation("Published DailyGoalAchieved event for user {UserId} to partition {Partition}",
                eventData.UserId, result.Partition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish DailyGoalAchieved event for user {UserId}", eventData.UserId);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}