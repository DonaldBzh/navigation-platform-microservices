using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavigationPlatform.Identity.Infrastructure.Services.Kafka;

namespace NavigationPlatform.Identity.Infrastructure.Services.Consumers;

public class UserStatusChangedConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<UserStatusChangedConsumer> _logger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly IServiceProvider _serviceProvider;

    public UserStatusChangedConsumer(
        IOptions<KafkaOptions> kafkaOptions,
        IKafkaClientFactory consumerFactory,
        ILogger<UserStatusChangedConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        _consumer = consumerFactory.CreateConsumer();
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IUserStatusChangedProcessor>();
        _consumer.Subscribe(_kafkaOptions.Topics.UserStatusChanged);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result != null)
                    {
                        await processor.ProcessAsync(result.Message.Value, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected Kafka consumer error");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

