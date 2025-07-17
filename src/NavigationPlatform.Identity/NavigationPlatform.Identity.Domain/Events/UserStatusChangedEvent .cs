using NavigationPlatform.Identity.Domain.Enums;

namespace NavigationPlatform.Identity.Domain.Events;

public class UserStatusChangedEvent 
{
    public Guid UserId { get; set; }
    public UserStatus OldStatus { get; set; }
    public UserStatus NewStatus { get; set; }
    public Guid AdminUserId { get; set; }
    public string Reason { get; set; }

}
