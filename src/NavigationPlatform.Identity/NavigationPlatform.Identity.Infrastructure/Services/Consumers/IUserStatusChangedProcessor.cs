namespace NavigationPlatform.Identity.Infrastructure.Services.Consumers;

public interface IUserStatusChangedProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
