using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NavigationPlatform.JourneyService.Infrastructure.Configuration;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;

public class KafkaClientFactory : IKafkaClientFactory
{
    private readonly KafkaOptions _kafkaOptions;

    public KafkaClientFactory(IOptions<KafkaOptions> kafkaOptions)
    {
        _kafkaOptions = kafkaOptions.Value;
    }

    public IConsumer<string, string> CreateConsumer()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.GroupId,
            EnableAutoCommit = _kafkaOptions.Consumer.EnableAutoCommit,
            AutoCommitIntervalMs = _kafkaOptions.Consumer.AutoCommitIntervalMs,
            SessionTimeoutMs = _kafkaOptions.Consumer.SessionTimeoutMs,
            HeartbeatIntervalMs = _kafkaOptions.Consumer.HeartbeatIntervalMs
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }

    public IProducer<string, string> CreateProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            MessageSendMaxRetries = _kafkaOptions.Producer.Retries,
            EnableIdempotence = _kafkaOptions.Producer.EnableIdempotence,
            MessageTimeoutMs = _kafkaOptions.Producer.MessageTimeoutMs
        };

        return new ProducerBuilder<string, string>(config).Build();
    }
}
