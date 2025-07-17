using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.LoginUser;
using NavigationPlatform.Identity.Application.Events;
using NavigationPlatform.Identity.Application.Events.UserCreated;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;
using System;

namespace NavigationPlatform.Identity.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public RegisterUserCommandHandler( IUserRepository userRepository,IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,ITokenRepository tokenRepository,ITokenGenerator tokenGenerator,
        IMediator mediator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _tokenRepository = tokenRepository;
        _tokenGenerator = tokenGenerator;
        _mediator = mediator;
    }
    public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        bool userExists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (userExists)
            throw new BadRequestException("User with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Create(
            request.Email,
            passwordHash,
            request.Role
        );

        await _userRepository.AddAsync(user);

        var token = _tokenGenerator.GenerateRefreshToken(64);
        var refreshToken = RefreshToken.Create(user.Id, token, DateTime.UtcNow.AddDays(7));

        await _tokenRepository.AddRefreshTokenAsync(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(user);

        await _mediator.Publish(new UserCreatedNotification
        {
            Id = user.Id,
            Status = user.Status,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Role = user.Role
        });


        return new AuthResult
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt
        };
    }


}


