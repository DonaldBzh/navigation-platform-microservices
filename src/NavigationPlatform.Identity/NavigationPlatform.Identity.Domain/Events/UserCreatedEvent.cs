using NavigationPlatform.Identity.Domain.Enums;

namespace NavigationPlatform.Identity.Domain.Events;

public class UserCreatedEvent
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
