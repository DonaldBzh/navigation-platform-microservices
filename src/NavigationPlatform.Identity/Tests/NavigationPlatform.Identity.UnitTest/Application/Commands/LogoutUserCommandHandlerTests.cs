using FluentAssertions;
using MediatR;
using NavigationPlatform.Identity.Application.Commands.LogoutUser;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;
public class LogoutUserCommandHandlerTests
{
    private readonly ITokenRepository _tokenRepository = Substitute.For<ITokenRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly LogoutUserCommandHandler _handler;

    public LogoutUserCommandHandlerTests()
    {
        _handler = new LogoutUserCommandHandler(_tokenRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldRevokeToken_WhenTokenIsValidAndOwnedByUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenValue = "valid-token";
        var refreshToken = RefreshToken.Create(userId, tokenValue, DateTime.UtcNow.AddDays(7));

        _currentUserService.GetUserId().Returns(userId);
        _tokenRepository.GetRefreshTokenAsync(tokenValue).Returns(refreshToken);

        var command = new LogoutUserCommand(tokenValue);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        refreshToken.Revoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().NotBeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTokenIsNotFound()
    {
        // Arrange
        var tokenValue = "nonexistent-token";

        _tokenRepository.GetRefreshTokenAsync(tokenValue).Returns((RefreshToken?)null);
        _currentUserService.GetUserId().Returns(Guid.NewGuid());

        var command = new LogoutUserCommand(tokenValue);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Token not found or does not belong to the user.");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTokenBelongsToAnotherUser()
    {
        // Arrange
        var actualUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var tokenValue = "another-users-token";

        var refreshToken = RefreshToken.Create(actualUserId, tokenValue, DateTime.UtcNow.AddDays(7));

        _tokenRepository.GetRefreshTokenAsync(tokenValue).Returns(refreshToken);
        _currentUserService.GetUserId().Returns(currentUserId);

        var command = new LogoutUserCommand(tokenValue);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Token not found or does not belong to the user.");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}