using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.Kafka;

public interface IEventProducer
{
    Task PublishAsync<T>(string topic, T message) where T : class;
}
