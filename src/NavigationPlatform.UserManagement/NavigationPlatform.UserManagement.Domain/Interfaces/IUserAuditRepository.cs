using NavigationPlatform.UserManagement.Domain.Entities;

namespace NavigationPlatform.UserManagement.Domain.Interfaces;

public interface IUserAuditRepository
{
    Task AddUserLog(UserAuditLog userAuditLog);

}
