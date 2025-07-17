using Microsoft.EntityFrameworkCore;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Identity.Infrastructure.Persistence;

namespace NavigationPlatform.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken token = default)
     => await _dbContext.Users.AddAsync(user,token);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default)
     => await _dbContext.Users.AnyAsync(u => u.Email == email, token);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken token = default)
     => await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, token);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken token = default)
     => await _dbContext.Users.FindAsync(id, token);

}
