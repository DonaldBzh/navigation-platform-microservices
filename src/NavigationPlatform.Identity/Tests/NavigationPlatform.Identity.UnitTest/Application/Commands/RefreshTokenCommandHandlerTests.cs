using FluentAssertions;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.Tokenrefresh;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly ITokenRepository _tokenRepository = Substitute.For<ITokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _tokenRepository,
            _userRepository,
            _tokenGenerator,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewAuthResult()
    {
        // Arrange
        var refreshTokenString = "valid-refresh-token";
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "hashedPass", UserRole.User);
        user.Id = userId;
        var oldRefreshToken = RefreshToken.Create(userId, refreshTokenString, DateTime.UtcNow.AddMinutes(10));

        _tokenRepository.GetRefreshTokenAsync(refreshTokenString).Returns(oldRefreshToken);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var newRefreshTokenString = "new-refresh-token";
        _tokenGenerator.GenerateRefreshToken().Returns(newRefreshTokenString);

        var newAccessToken = ("access-token", DateTime.UtcNow.AddMinutes(15));
        _tokenGenerator.GenerateAccessToken(user).Returns(newAccessToken);

        var command = new RefreshTokenCommand(refreshTokenString);

        RefreshToken? capturedRefreshToken = null;
         _tokenRepository
            .When(x => x.AddRefreshTokenAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>()))
            .Do(call => capturedRefreshToken = call.Arg<RefreshToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken.Item1);
        result.AccessTokenExpiresAt.Should().BeCloseTo(newAccessToken.Item2, TimeSpan.FromSeconds(1));
        result.RefreshToken.Should().Be(newRefreshTokenString);

        capturedRefreshToken.Should().NotBeNull();
        capturedRefreshToken!.Token.Should().Be(newRefreshTokenString);
        capturedRefreshToken.UserId.Should().Be(userId);

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}