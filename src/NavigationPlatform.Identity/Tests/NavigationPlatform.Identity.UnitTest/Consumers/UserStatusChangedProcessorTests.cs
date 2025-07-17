using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Application.Commands.UpdateUserStatus;
using NavigationPlatform.Identity.Domain.Events;
using NavigationPlatform.Identity.Infrastructure.Services.Consumers;
using NSubstitute;
using System.Text.Json;

namespace NavigationPlatform.Identity.UnitTest.Consumers;

public class UserStatusChangedProcessorTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<UserStatusChangedProcessor> _logger = Substitute.For<ILogger<UserStatusChangedProcessor>>();
    private readonly UserStatusChangedProcessor _processor;

    public UserStatusChangedProcessorTests()
    {
        _processor = new UserStatusChangedProcessor(_mediator, _logger);
    }

    [Fact]
    public async Task ProcessAsync_Should_Send_Command_When_Message_Is_Valid()
    {
        // Arrange
        var evt = new UserStatusChangedEvent
        {
            UserId = Guid.NewGuid(),
            OldStatus = Identity.Domain.Enums.UserStatus.Active,
            NewStatus = Identity.Domain.Enums.UserStatus.Deactivated,
            Reason = "Violation",
            AdminUserId = Guid.NewGuid()
        };

        var message = JsonSerializer.Serialize(evt);
        var token = new CancellationToken();

        // Act
        await _processor.ProcessAsync(message, token);

        // Assert
        await _mediator.Received(1).Send(Arg.Is<UpdateUserStatusCommand>(cmd =>
            cmd.UserId == evt.UserId &&
            cmd.OldStatus == evt.OldStatus &&
            cmd.NewStatus == evt.NewStatus &&
            cmd.Reason == evt.Reason &&
            cmd.AdminUserId == evt.AdminUserId
        ), token);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => x.ToString().Contains(evt.UserId.ToString())),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Log_Warning_When_Message_Is_Invalid_Json()
    {
        // Arrange
        var invalidMessage = "{ this is not valid JSON";
        var token = new CancellationToken();

        // Act
        await _processor.ProcessAsync(invalidMessage, token);

        // Assert
        await _mediator.DidNotReceive().Send(Arg.Any<UpdateUserStatusCommand>(), token);

        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Log_Warning_When_Deserialized_Event_Is_Null()
    {
        // Arrange
        var nullJson = "null";
        var token = new CancellationToken();

        // Act
        await _processor.ProcessAsync(nullJson, token);

        // Assert
        await _mediator.DidNotReceive().Send(Arg.Any<UpdateUserStatusCommand>(), token);

        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("Failed to deserialize UserStatusChangedEvent")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
