using NavigationPlatform.UserManagement.Domain.Enums;

namespace NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;

public class UpdateUserStatusRequest
{
    public UserStatus Status { get; set; }
    public string? Reason { get; set; }
}