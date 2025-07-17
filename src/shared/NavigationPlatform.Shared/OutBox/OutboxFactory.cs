using System.Text.Json;

namespace NavigationPlatform.Shared.OutBox;

public static class OutboxFactory
{
    public static OutboxEvent Create<T>(T domainEvent, string? eventTypeOverride = null)
        where T : class
    {
        return new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventTypeOverride ?? typeof(T).Name,
            EventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false,
        };
    }
}