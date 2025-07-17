using MediatR;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.LoginUser;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;
using System.ComponentModel.DataAnnotations;


namespace NavigationPlatform.Identity.Application.Commands.Tokenrefresh;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        ITokenRepository tokenRepository,
        IUserRepository userRepository,
        ITokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _tokenRepository.GetRefreshTokenAsync(request.Token);
        if (refreshToken is null || refreshToken.Revoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new ApplicationValidationException("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User not found.");

        refreshToken.Revoke();

        var newRefreshTokenString = _tokenGenerator.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.Id, newRefreshTokenString, DateTime.UtcNow.AddDays(7));
        await _tokenRepository.AddRefreshTokenAsync(newRefreshToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = newRefreshToken.Token
        };
    }
}