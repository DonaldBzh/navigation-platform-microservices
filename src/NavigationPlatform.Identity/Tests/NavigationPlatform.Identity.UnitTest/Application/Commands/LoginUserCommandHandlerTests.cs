using FluentAssertions;
using FluentValidation;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.LoginUser;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenRepository _tokenRepository = Substitute.For<ITokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _unitOfWork,
            _tokenRepository,
            _tokenGenerator
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedpwd");
        var command = new LoginUserCommand("test@example.com", "password");

        _userRepository.GetByEmailAsync(command.Email).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(true);
        _tokenGenerator.GenerateRefreshToken(64).Returns("refresh-token");
        _tokenGenerator.GenerateAccessToken(user).Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        await _tokenRepository.Received(1).AddRefreshTokenAsync(Arg.Any<RefreshToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsValidationException()
    {
        // Arrange
        var command = new LoginUserCommand("notfound@example.com", "password");
        _userRepository.GetByEmailAsync(command.Email).Returns((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
            
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsValidationException()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedpwd");
        var command = new LoginUserCommand("test@example.com", "wrongpassword");

        _userRepository.GetByEmailAsync(command.Email).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationValidationException>()
            .WithMessage("Invalid credentials");
    }

    [Theory]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Deactivated)]
    public async Task Handle_UserInactive_ThrowsValidationException(UserStatus status)
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedpwd");
        user.Status = status;

        var command = new LoginUserCommand("test@example.com", "password");

        _userRepository.GetByEmailAsync(command.Email).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationValidationException>()
            .WithMessage("User is not active, please contact admin.");
    }
}