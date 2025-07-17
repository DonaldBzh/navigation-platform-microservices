using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Application.Commands.UpdateUserStatus;
using NavigationPlatform.Identity.Domain.Events;
using System.Text.Json;

namespace NavigationPlatform.Identity.Infrastructure.Services.Consumers;

public class UserStatusChangedProcessor : IUserStatusChangedProcessor
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserStatusChangedProcessor> _logger;

    public UserStatusChangedProcessor(IMediator mediator, ILogger<UserStatusChangedProcessor> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<UserStatusChangedEvent>(message);
            if (evt == null)
            {
                _logger.LogWarning("Failed to deserialize UserStatusChangedEvent");
                return;
            }

            await _mediator.Send(new UpdateUserStatusCommand
            {
                UserId = evt.UserId,
                OldStatus = evt.OldStatus,
                NewStatus = evt.NewStatus,
                Reason = evt.Reason,
                AdminUserId = evt.AdminUserId
            }, cancellationToken);

            _logger.LogInformation("Processed user status changed event for user {UserId}", evt.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserStatusChangedEvent message: {Message}", message);
        }
    }
}
