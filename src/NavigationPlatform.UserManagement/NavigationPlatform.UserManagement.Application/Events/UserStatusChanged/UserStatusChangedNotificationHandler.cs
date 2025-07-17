using MediatR;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Events;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Application.Events.UserStatusChanged;

public class UserStatusChangedNotificationHandler : INotificationHandler<UserStatusChangedNotification>
{
    private readonly IUserAuditRepository _auditRepository;
    private readonly IOutboxPublisher _eventPublisher;

    public UserStatusChangedNotificationHandler(IUserAuditRepository auditRepo, IOutboxPublisher publisher)
    {
        _auditRepository = auditRepo;
        _eventPublisher = publisher;
    }

    public async Task Handle(UserStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        // Add audit log
        var auditLog = new UserAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            AdminUserId = notification.AdminUserId,
            Action = "StatusChanged",
            OldStatus = notification.OldStatus,
            NewStatus = notification.NewStatus,
            Timestamp = DateTime.UtcNow,
            Reason = notification.Reason
        };
        await _auditRepository.AddUserLog(auditLog);

        // Publish event
        var domainEvent = new UserStatusChangedEvent
        {
            UserId = notification.UserId,
            OldStatus = notification.OldStatus,
            NewStatus = notification.NewStatus,
            AdminUserId = notification.AdminUserId,
            Reason = notification.Reason,
            ChangedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(domainEvent);
    }
}
