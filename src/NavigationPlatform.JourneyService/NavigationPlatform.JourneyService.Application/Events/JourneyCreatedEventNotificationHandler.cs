using MediatR;
using NavigationPlatform.JourneyService.Application.Events;
using NavigationPlatform.JourneyService.Domain.Events;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.OutBox;

public class JourneyCreatedEventNotificationHandler : INotificationHandler<JourneyCreatedEventNotification>
{
    private readonly IEventProducer _eventProducer;

    public JourneyCreatedEventNotificationHandler(IEventProducer eventProducer)
    {
        _eventProducer = eventProducer;
    }

    public async Task Handle(JourneyCreatedEventNotification notification, CancellationToken cancellationToken)
    {

        // Publish event
        var domainEvent = new JourneyCreatedEvent
        {
            JourneyId = notification.JourneyId,
            UserId = notification.UserId,
            StartTime = notification.StartTime,
            EndTime = notification.EndTime,
            DistanceKm = notification.DistanceKm,
            CreatedAt = notification.CreatedAt
        };

        await _eventProducer.PublishAsync("journey-created-events", domainEvent);
    }
}

