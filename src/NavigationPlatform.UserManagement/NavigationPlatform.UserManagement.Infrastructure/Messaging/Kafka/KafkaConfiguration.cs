using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Infrastructure.Messaging.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = null!;
    public KafkaConfiguration Topics { get; set; } = new();
}


public class KafkaConfiguration
{
    public string UserCreated { get; set; } = default!;
    public string UserStatusChanged { get; set; } = default!;
}
