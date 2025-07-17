using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.Constants;

public static class KafkaConsts
{
    private static Dictionary<string, string> TopicsMapping = new Dictionary<string, string>()
    {
         { "UserStatusChangedEvent", "user-status-changed-events" },
         { "UserCreatedEvent", "user-created-events" }
    };

    public static class Topics
    {
        public const string UserStatusChangedEvent = nameof(UserStatusChangedEvent);
        public const string UserCreatedEvent = nameof(UserCreatedEvent);

    }

    public static string GetTopicFor(string eventName) 
    {
        return TopicsMapping.TryGetValue(eventName, out var topic)
            ? topic
            : throw new KeyNotFoundException($"No topic mapping found for event {eventName} " );
    }
    public static class ConsumersGroups
    {
        public const string IdentityConsumer = "identity-service-consumer";
        public const string UserManagementConsumer = "user-management-service-consumer";

    }
}
