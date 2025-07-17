using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NavigationPlatform.UserManagement.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void AddUser(User user)
      => _dbContext.Add(user);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken token = default)
        => await _dbContext.Users.FindAsync(id, token);

    public void Update(User user)
    => _dbContext.Update(user);
}
