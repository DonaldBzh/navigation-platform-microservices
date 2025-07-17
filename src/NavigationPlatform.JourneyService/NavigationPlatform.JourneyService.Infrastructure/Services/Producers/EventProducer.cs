using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavigationPlatform.JourneyService.Infrastructure.Configuration;
using NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.OutBox;
using System.Text;
using System.Text.Json;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Producers;

public class EventProducer : IEventProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<EventProducer> _logger;

    public EventProducer(ILogger<EventProducer> logger, IKafkaClientFactory kafkaFactory)
    {
        _logger = logger;
        _producer = kafkaFactory.CreateProducer();
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var serializedMessage = message is string str
            ? str
            : JsonSerializer.Serialize(message);

        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = serializedMessage,
                Headers = new Headers
                {
                    { "event-version", Encoding.UTF8.GetBytes("1.0") },
                    { "timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) }
                }
            });

            _logger.LogInformation("Published message to Kafka topic {Topic} with offset {Offset}", topic, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error publishing message to Kafka topic {Topic}", topic);
            throw;
        }
    }

}
