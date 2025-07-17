using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Options;
using NavigationPlatform.RewardService.Worker.Commands;
using NavigationPlatform.RewardService.Worker.Configuration;
using NavigationPlatform.RewardService.Worker.Events;
using System.Text.Json;

namespace NavigationPlatform.RewardService.Worker.Services;

public class JourneyConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JourneyConsumerService> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public JourneyConsumerService(
        IOptions<KafkaOptions> kafkaOptions,
        IServiceProvider serviceProvider,
        ILogger<JourneyConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.GroupId,
            EnableAutoCommit = _kafkaOptions.Consumer.EnableAutoCommit,
            AutoCommitIntervalMs = _kafkaOptions.Consumer.AutoCommitIntervalMs,
            SessionTimeoutMs = _kafkaOptions.Consumer.SessionTimeoutMs,
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_kafkaOptions.Topics.JourneyCreated);
        _logger.LogInformation("Kafka consumer started, subscribed to topic: {Topic}", _kafkaOptions.Topics.JourneyCreated);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null)
                    {
                        await ProcessMessage(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult)
    {
        try
        {
            _logger.LogDebug("Processing message from partition {Partition}, offset {Offset}",
                consumeResult.Partition, consumeResult.Offset);

            var journeyCreated = JsonSerializer.Deserialize<JourneyCreated>(consumeResult.Message.Value);

            if (journeyCreated == null)
            {
                _logger.LogWarning("Failed to deserialize JourneyCreated event");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new ProcessJourneyCreatedCommand(journeyCreated);
            await mediator.Send(command);

            _logger.LogInformation("Successfully processed JourneyCreated event for user {UserId}, journey {JourneyId}",
                journeyCreated.UserId, journeyCreated.JourneyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Kafka message: {Message}", consumeResult.Message.Value);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}