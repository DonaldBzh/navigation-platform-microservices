using NavigationPlatform.Shared.Constants;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Enums;
using System.Text.Json;

namespace NavigationPlatform.UserManagement.Domain.Events;

public class UserStatusChangedEvent 
{
    public Guid UserId { get; set; }
    public UserStatus OldStatus { get; set; }
    public UserStatus NewStatus { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public Guid AdminUserId { get; set; }

}
