using NavigationPlatform.Identity.Domain.Entities;

namespace NavigationPlatform.Identity.Domain.Interfaces;

public interface ITokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken,CancellationToken token = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

}

