using FluentAssertions;
using NavigationPlatform.Identity.Application.Commands.ChangeUserStatus;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands;

public class ChangeUserStatusCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly ChangeUserStatusCommandHandler _handler;

    public ChangeUserStatusCommandHandlerTests()
    {
        _handler = new ChangeUserStatusCommandHandler(_unitOfWork, _userRepository);
    }

    [Fact]
    public async Task Handle_UserExists_ChangesStatusAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "hashedPass");
        user.Id = userId;

        _userRepository.GetByIdAsync(userId).Returns(user);

        var command = new ChangeUserStatusCommand(userId, UserStatus.Suspended);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User)null!);

        var command = new ChangeUserStatusCommand(userId, UserStatus.Deactivated);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User Not Found");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}