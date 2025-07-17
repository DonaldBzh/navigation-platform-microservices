using NavigationPlatform.UserManagement.Domain.Enums;

namespace NavigationPlatform.UserManagement.Domain.Entities;

public class UserAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AdminUserId { get; set; }
    public string Action { get; set; } = null!;
    public UserStatus? OldStatus { get; set; }
    public UserStatus NewStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Reason { get; set; }

    public User User { get; set; } = null!;
}