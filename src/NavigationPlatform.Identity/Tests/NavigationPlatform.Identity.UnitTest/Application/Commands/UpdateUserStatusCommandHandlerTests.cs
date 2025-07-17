using FluentAssertions;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Application.Commands.UpdateUserStatus;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;

public class UpdateUserStatusCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<UpdateUserStatusCommandHandler> _logger = Substitute.For<ILogger<UpdateUserStatusCommandHandler>>();

    private readonly UpdateUserStatusCommandHandler _handler;

    public UpdateUserStatusCommandHandlerTests()
    {
        _handler = new UpdateUserStatusCommandHandler(_logger, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Update_Status_When_User_Exists()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedPwd", UserRole.User);
        var command = new UpdateUserStatusCommand
        {
            UserId = user.Id,
            OldStatus = UserStatus.Active,
            NewStatus = UserStatus.Suspended,
            AdminUserId = Guid.NewGuid(),
            Reason = "Policy violation"
        };

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("User status updated")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Handle_Should_LogWarning_When_User_Not_Found()
    {
        // Arrange
        var command = new UpdateUserStatusCommand
        {
            UserId = Guid.NewGuid(),
            OldStatus = UserStatus.Active,
            NewStatus = UserStatus.Suspended,
            AdminUserId = Guid.NewGuid(),
            Reason = "Test"
        };

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("User not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }
}
