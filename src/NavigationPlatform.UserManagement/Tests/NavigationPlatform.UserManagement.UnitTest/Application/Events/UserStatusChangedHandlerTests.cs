using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.UserManagement.Application.Events;
using NavigationPlatform.UserManagement.Application.Events.UserStatusChanged;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Enums;
using NavigationPlatform.UserManagement.Domain.Events;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NSubstitute;

namespace NavigationPlatform.UserManagement.UnitTest.Application.Events
{
    public class UserStatusChangedHandlerTests
    {
        private readonly IUserAuditRepository _auditRepo = Substitute.For<IUserAuditRepository>();
        private readonly IOutboxPublisher _eventPublisher = Substitute.For<IOutboxPublisher>();
        private readonly UserStatusChangedNotificationHandler _handler;

        public UserStatusChangedHandlerTests()
        {
            _handler = new UserStatusChangedNotificationHandler(_auditRepo, _eventPublisher);
        }

        [Fact]
        public async Task Handle_Should_LogAudit_And_PublishEvent()
        {
            // Arrange
            var notification = new UserStatusChangedNotification
            {
                UserId = Guid.NewGuid(),
                OldStatus = UserStatus.Active,
                NewStatus = UserStatus.Suspended,
                AdminUserId = Guid.NewGuid(),
                Reason = "Violation of terms"
            };

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            await _auditRepo.Received(1).AddUserLog(Arg.Is<UserAuditLog>(log =>
                log.UserId == notification.UserId &&
                log.AdminUserId == notification.AdminUserId &&
                log.Action == "StatusChanged" &&
                log.OldStatus == notification.OldStatus &&
                log.NewStatus == notification.NewStatus &&
                log.Reason == notification.Reason &&
                log.Timestamp <= DateTime.UtcNow
            ));

            await _eventPublisher.Received(1).PublishAsync(Arg.Is<UserStatusChangedEvent>(e =>
                e.UserId == notification.UserId &&
                e.OldStatus == notification.OldStatus &&
                e.NewStatus == notification.NewStatus &&
                e.AdminUserId == notification.AdminUserId &&
                e.Reason == notification.Reason &&
                e.ChangedAt <= DateTime.UtcNow
            ));
        }
    }
}
