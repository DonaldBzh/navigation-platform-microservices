using MediatR;
using NavigationPlatform.Identity.Domain.Enums;

namespace NavigationPlatform.Identity.Application.Events.UserCreated;

public class UserCreatedNotification : INotification
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

}
