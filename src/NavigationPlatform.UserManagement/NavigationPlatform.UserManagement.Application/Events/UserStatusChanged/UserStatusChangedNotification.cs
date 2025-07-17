using MediatR;
using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Application.Events.UserStatusChanged;

public class UserStatusChangedNotification : INotification
{
    public Guid UserId { get; init; }
    public UserStatus OldStatus { get; set; }
    public UserStatus NewStatus { get; set; }
    public Guid AdminUserId { get; init; }
    public string Reason { get; init; }
}


