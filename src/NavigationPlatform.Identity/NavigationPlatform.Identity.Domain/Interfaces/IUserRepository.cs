using NavigationPlatform.Identity.Domain.Entities;

namespace NavigationPlatform.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email,CancellationToken token = default);
    Task AddAsync(User user, CancellationToken token = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken token = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);
}
