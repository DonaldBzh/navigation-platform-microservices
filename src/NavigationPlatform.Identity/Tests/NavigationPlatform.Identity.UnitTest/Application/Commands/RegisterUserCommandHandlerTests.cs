using FluentAssertions;
using MediatR;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.RegisterUser;
using NavigationPlatform.Identity.Application.Events.UserCreated;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenRepository _tokenRepository = Substitute.For<ITokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _unitOfWork,
            _tokenRepository,
            _tokenGenerator,
            _mediator
        );
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_EmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("existing@example.com", "SecurePass123!", UserRole.User);

        _userRepository.ExistsByEmailAsync(command.Email).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User with this email already exists.");
    }

    [Fact]
    public async Task Handle_Should_CreateUser_And_ReturnAuthResult()
    {
        // Arrange
        var command = new RegisterUserCommand("newuser@example.com", "SecurePass123!", UserRole.User);
        var hashedPassword = "hashedpassword";
        var user = User.Create(command.Email, hashedPassword, UserRole.User);
        var refreshToken = RefreshToken.Create(user.Id, "refresh-token", DateTime.UtcNow.AddDays(7));
        var accessToken = ("access-token", DateTime.UtcNow.AddMinutes(15));

        _userRepository.ExistsByEmailAsync(command.Email).Returns(false);
        _passwordHasher.Hash(command.Password).Returns(hashedPassword);
        _tokenGenerator.GenerateRefreshToken(Arg.Any<int>()).Returns(refreshToken.Token);
        _tokenGenerator.GenerateAccessToken(Arg.Any<User>()).Returns(accessToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.Email == command.Email));
        await _tokenRepository.Received(1).AddRefreshTokenAsync(Arg.Any<RefreshToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None); 
        await _mediator.Received(1).Publish(Arg.Any<UserCreatedNotification>(), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken.Item1);
        result.RefreshToken.Should().Be(refreshToken.Token);
        result.AccessTokenExpiresAt.Should().BeCloseTo(accessToken.Item2, TimeSpan.FromSeconds(1));
    }
}
