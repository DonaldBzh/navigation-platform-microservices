using FluentAssertions;
using MediatR;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;
using NavigationPlatform.UserManagement.Application.Events.UserStatusChanged;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Enums;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NSubstitute;

namespace NavigationPlatform.UserManagement.UnitTest.Application.Users.Command;

public class UpdateUserStatusCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxPublisher _eventPublisher = Substitute.For<IOutboxPublisher>();
    private readonly IUserAuditRepository _userAuditRepository = Substitute.For<IUserAuditRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly UpdateUserStatusCommandHandler _handler;

    public UpdateUserStatusCommandHandlerTests()
    {
        _handler = new UpdateUserStatusCommandHandler(
            _userRepository,
            _unitOfWork,
            _eventPublisher,
            _userAuditRepository,
            _outboxEventRepository,
            _mediator,
            _currentUserService
        );
    }

    [Fact]
    public async Task Handle_Should_Update_Status_And_Publish_Event()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Status = UserStatus.Active,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _currentUserService.GetUserId().Returns(adminId);

        var command = new UpdateUserStatusCommand(
            userId,
            UserStatus.Deactivated,
            "Policy violation"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(UserStatus.Deactivated);
        result.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _mediator.Received(1).Publish(
            Arg.Is<UserStatusChangedNotification>(e =>
                e.UserId == userId &&
                e.OldStatus == UserStatus.Active &&
                e.NewStatus == UserStatus.Deactivated &&
                e.AdminUserId == adminId &&
                e.Reason == "Policy violation"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserStatusCommand(
            Guid.NewGuid(),
            UserStatus.Deactivated,
            "Reason"
        );

        _userRepository.GetByIdAsync(command.UserId).Returns((User?)null);

        // Act
        var result = () => _handler.Handle(command, CancellationToken.None);


        // Assert
        await result.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Publish(Arg.Any<UserStatusChangedNotification>(), Arg.Any<CancellationToken>());
    }
}
