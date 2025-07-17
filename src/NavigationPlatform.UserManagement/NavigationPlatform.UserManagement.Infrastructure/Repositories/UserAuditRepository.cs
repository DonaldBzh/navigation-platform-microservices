using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NavigationPlatform.UserManagement.Infrastructure.Persistance;

namespace NavigationPlatform.UserManagement.Infrastructure.Repositories;

public class UserAuditRepository : IUserAuditRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UserAuditRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddUserLog(UserAuditLog userAuditLog)
        => await _dbContext.UserAuditLogs.AddAsync(userAuditLog);
}
