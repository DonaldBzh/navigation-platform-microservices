using Confluent.Kafka;

namespace NavigationPlatform.Identity.Infrastructure.Services.Consumers;

public interface IKafkaClientFactory
{
    IConsumer<string, string> CreateConsumer();
    IProducer<string, string> CreateProducer();
}

