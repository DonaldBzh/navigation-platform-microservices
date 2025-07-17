using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;

public interface IKafkaClientFactory
{
    IConsumer<string, string> CreateConsumer();
    IProducer<string, string> CreateProducer();
}
