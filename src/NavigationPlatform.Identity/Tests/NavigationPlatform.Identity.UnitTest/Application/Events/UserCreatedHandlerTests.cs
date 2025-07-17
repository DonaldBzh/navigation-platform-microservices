using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Application.Events.UserCreated;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Events;
using NavigationPlatform.Shared.OutBox;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Events;

public class UserCreatedNotificationHandlerTests
{
    private readonly IOutboxPublisher _eventPublisher = Substitute.For<IOutboxPublisher>();
    private readonly ILogger<UserCreatedNotificationHandler> _logger = Substitute.For<ILogger<UserCreatedNotificationHandler>>();

    private readonly UserCreatedNotificationHandler _handler;

    public UserCreatedNotificationHandlerTests()
    {
        _handler = new UserCreatedNotificationHandler(_eventPublisher, _logger);
    }

    [Fact]
    public async Task Handle_Should_PublishUserCreatedEvent_WithExpectedProperties()
    {
        // Arrange
        var notification = new UserCreatedNotification
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(Arg.Is<UserCreatedEvent>(e =>
            e.Id == notification.Id &&
            e.Email == notification.Email &&
            e.CreatedAt == notification.CreatedAt &&
            e.Role == notification.Role &&
            e.Status == notification.Status
        ));
    }
}
