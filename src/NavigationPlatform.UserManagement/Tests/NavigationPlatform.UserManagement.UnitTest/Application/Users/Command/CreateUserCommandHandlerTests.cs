using FluentAssertions;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Application.Commands.CreateUser;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Enums;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NSubstitute;

namespace NavigationPlatform.UserManagement.UnitTest.Application.Users.Command;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _handler = new CreateUserCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_AddUser_And_SaveChanges()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        User? capturedUser = null;

        _userRepository
            .When(repo => repo.AddUser(Arg.Any<User>()))
            .Do(call => capturedUser = call.Arg<User>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Id.Should().Be(command.UserId);
        capturedUser.Email.Should().Be(command.Email);
        capturedUser.Role.Should().Be(command.Role);
        capturedUser.Status.Should().Be(command.Status);
        capturedUser.CreatedAt.Should().BeCloseTo(command.CreatedAt, TimeSpan.FromSeconds(1));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}