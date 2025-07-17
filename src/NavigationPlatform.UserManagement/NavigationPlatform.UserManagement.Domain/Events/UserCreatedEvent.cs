using NavigationPlatform.UserManagement.Domain.Enums;

namespace NavigationPlatform.UserManagement.Domain.Events;

public class UserCreatedEvent
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}