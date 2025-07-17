using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Shared.Constants;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Infrastructure.Persistance;

namespace NavigationPlatform.UserManagement.Infrastructure.Outbox;

public class OutboxEventProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxEventProcessor> _logger;

    public OutboxEventProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxEventProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var unitofWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var kafkaProducer = scope.ServiceProvider.GetRequiredService<IEventProducer>();

                var unprocessedEvents = await context.OutboxEvents
                    .Where(e => !e.IsProcessed)
                    .OrderBy(e => e.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var outboxEvent in unprocessedEvents)
                {
                    try
                    {
                        var topic = KafkaConsts.GetTopicFor(outboxEvent.EventType);

                        await kafkaProducer.PublishAsync(topic, outboxEvent.EventData);

                        outboxEvent.IsProcessed = true;
                        outboxEvent.ProcessedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process outbox event {EventId}", outboxEvent.Id);
                    }
                }

                if (unprocessedEvents.Any())
                {
                    await unitofWork.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OutboxEventProcessor");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}