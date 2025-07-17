using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Domain.Events;
using NavigationPlatform.Shared.OutBox;

namespace NavigationPlatform.Identity.Application.Events.UserCreated;

public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly IOutboxPublisher _eventPublisher;
    private readonly ILogger<UserCreatedNotificationHandler> _logger;

    public UserCreatedNotificationHandler(IOutboxPublisher publisher, ILogger<UserCreatedNotificationHandler> logger)
    {

        _eventPublisher = publisher;
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = new UserCreatedEvent
        {
            Id = notification.Id,
            Email = notification.Email,
            CreatedAt = notification.CreatedAt,
            Role = notification.Role,
            Status = notification.Status,
        };

        await _eventPublisher.PublishAsync(domainEvent);
    }
}
