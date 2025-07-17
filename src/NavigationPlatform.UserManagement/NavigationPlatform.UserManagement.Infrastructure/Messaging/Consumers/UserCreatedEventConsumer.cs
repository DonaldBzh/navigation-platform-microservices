using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavigationPlatform.Shared.Constants;
using NavigationPlatform.UserManagement.Application.Commands.CreateUser;
using NavigationPlatform.UserManagement.Domain.Events;
using NavigationPlatform.UserManagement.Infrastructure.Messaging.Kafka;
using System.Text.Json;

namespace NavigationPlatform.UserManagement.Infrastructure.Messaging.Consumers;
public class UserCreatedEventConsumer : BackgroundService
{
    private readonly ILogger<UserCreatedEventConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;

    public UserCreatedEventConsumer(
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UserCreatedEventConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
            GroupId = KafkaConsts.ConsumersGroups.UserManagementConsumer,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topic = KafkaConsts.GetTopicFor(KafkaConsts.Topics.UserCreatedEvent);
        _consumer.Subscribe(topic);

        _logger.LogInformation("Kafka consumer subscribed to topic: {topic}", topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (consumeResult is null || consumeResult.Message?.Value == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;

                    }

                    await ProcessMessage(consumeResult, stoppingToken);

                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer operation was cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer");
                    // Add delay to prevent tight error loops
                    await Task.Delay(1000, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Kafka consumer");
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped and closed");
        }


    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            _logger.LogDebug("Processing message: {Message}", consumeResult.Message.Value);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(
                consumeResult.Message.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userCreatedEvent == null)
            {
                _logger.LogWarning("Deserialized message is null. Raw message: {RawMessage}",
                    consumeResult.Message.Value);
                return;
            }

            var userCreatedCommand= new CreateUserCommand
            {
                UserId = userCreatedEvent.Id,
                Email = userCreatedEvent.Email,
                Status = userCreatedEvent.Status,
                Role = userCreatedEvent.Role,
                CreatedAt = userCreatedEvent.CreatedAt
            };
            await mediator.Send(userCreatedCommand, cancellationToken);

            _logger.LogInformation("Successfully processed UserStatusChanged event for user {UserId}",
                userCreatedCommand.UserId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message. Raw message: {RawMessage}",
                consumeResult.Message.Value);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserStatusChanged event");
            throw;
        }
    }
}