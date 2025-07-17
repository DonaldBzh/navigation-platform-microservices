using NavigationPlatform.UserManagement.Domain.Enums;

namespace NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}