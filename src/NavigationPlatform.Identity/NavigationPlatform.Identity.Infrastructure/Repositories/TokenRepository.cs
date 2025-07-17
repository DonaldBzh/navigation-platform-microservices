using Microsoft.EntityFrameworkCore;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Identity.Infrastructure.Persistence;

public class TokenRepository : ITokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public TokenRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken token = default)
     => await _dbContext.RefreshTokens.AddAsync(refreshToken);

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.Revoked == false, cancellationToken);
    }
}