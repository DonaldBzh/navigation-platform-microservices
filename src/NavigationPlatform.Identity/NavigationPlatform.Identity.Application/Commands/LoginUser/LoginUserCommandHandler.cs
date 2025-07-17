using MediatR;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.Identity.Application.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;


    public LoginUserCommandHandler(IUserRepository userRepository,IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork, ITokenRepository tokenRepository,ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _tokenRepository = tokenRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
       
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            throw new NotFoundException("User not found");

        bool passwordMatch = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (passwordMatch == false)
            throw new ApplicationValidationException("Invalid credentials");

        if (user.Status == UserStatus.Suspended || user.Status == UserStatus.Deactivated)
        {
            throw new ApplicationValidationException("User is not active, please contact admin.");
        }

        var token = _tokenGenerator.GenerateRefreshToken(64);

        var refreshToken = RefreshToken.Create(user.Id, token, DateTime.UtcNow.AddDays(7));

        await _tokenRepository.AddRefreshTokenAsync(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(user);

        return new AuthResult
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt
        };

    }
}
