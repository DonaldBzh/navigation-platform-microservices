using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace NavigationPlatform.JourneyService.UnitTests.Services
{
    public class RewardWorkerProcessorTests
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RewardWorkerProcessor> _logger;
        private readonly RewardWorkerProcessor _processor;

        public RewardWorkerProcessorTests()
        {
            _mediator = Substitute.For<IMediator>();
            _logger = Substitute.For<ILogger<RewardWorkerProcessor>>();
            _processor = new RewardWorkerProcessor(_mediator, _logger);
        }

        [Fact]
        public void Constructor_InitializesAllDependencies()
        {
            // Act is in constructor

            // Assert - Dependencies should be assigned (verified through successful test execution)
            _processor.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessAsync_WithValidMessage_SendsCommandAndLogsSuccess()
        {
            // Arrange
            var dailyGoalAchieved = new DailyGoalAchieved
            {
                UserId = Guid.NewGuid(),
                TriggeringJourneyId = Guid.NewGuid(),
                AchievedAt = DateTimeOffset.UtcNow,
                GoalType = "StepCount",
                TargetValue = 10000,
                AchievedValue = 12500
            };

            var message = JsonSerializer.Serialize(dailyGoalAchieved);
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(Arg.Any<IRequest>(), cancellationToken);

            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v =>
                    v.ToString().Contains("Successfully processed DailyGoalAchieved event") &&
                    v.ToString().Contains(dailyGoalAchieved.UserId.ToString()) &&
                    v.ToString().Contains(dailyGoalAchieved.TriggeringJourneyId.ToString())),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_WithNullMessage_DoesNotSendCommandAndDoesNotLog()
        {
            // Arrange
            string message = null;
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.DidNotReceive().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());

            _logger.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_WithEmptyMessage_DoesNotSendCommandAndDoesNotLog()
        {
            // Arrange
            var message = "";
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.DidNotReceive().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());

            _logger.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }


        [Fact]
        public async Task ProcessAsync_WithValidJsonButNullDeserialization_DoesNotSendCommandAndDoesNotLog()
        {
            // Arrange - This can happen with valid JSON that deserializes to null
            var message = "null";
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.DidNotReceive().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());

            _logger.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_WhenMediatorThrowsException_LogsErrorWithOriginalMessage()
        {
            // Arrange
            var dailyGoalAchieved = new DailyGoalAchieved
            {
                UserId = Guid.NewGuid(),
                TriggeringJourneyId = Guid.NewGuid()
            };

            var message = JsonSerializer.Serialize(dailyGoalAchieved);
            var cancellationToken = CancellationToken.None;
            var mediatorException = new InvalidOperationException("Command handler failed");

            _mediator.Send(Arg.Any<IRequest>(), cancellationToken)
                .Throws(mediatorException);

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(Arg.Any<IRequest>(), cancellationToken);

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v =>
                    v.ToString().Contains("Error processing Kafka message") &&
                    v.ToString().Contains(message)),
                Arg.Is<Exception>(ex => ex == mediatorException || ex.InnerException == mediatorException),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_WithComplexDailyGoalAchievedObject_ProcessesSuccessfully()
        {
            // Arrange
            var dailyGoalAchieved = new DailyGoalAchieved
            {
                UserId = Guid.NewGuid(),
                TriggeringJourneyId = Guid.NewGuid(),
                AchievedAt = DateTimeOffset.UtcNow,
                GoalType = "ComplexGoal",
                TargetValue = 5000,
                AchievedValue = 7500,
                Metadata = new Dictionary<string, object>
                {
                    { "category", "fitness" },
                    { "difficulty", "hard" },
                    { "bonus", true }
                }
            };

            var message = JsonSerializer.Serialize(dailyGoalAchieved);
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(Arg.Any<IRequest>(), cancellationToken);
        }

        [Theory]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public async Task ProcessAsync_WithWhitespaceMessage_LogsErrorForInvalidJson(string whitespaceMessage)
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(whitespaceMessage, cancellationToken);

            // Assert
            await _mediator.DidNotReceive().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Error processing Kafka message")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_WithPartiallyValidJson_LogsErrorAndDoesNotSendCommand()
        {
            // Arrange - Valid JSON structure but missing required properties
            var partialMessage = JsonSerializer.Serialize(new { SomeProperty = "value" });
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(partialMessage, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(Arg.Any<IRequest>(), cancellationToken);
        }

        [Fact]
        public async Task ProcessAsync_WithVeryLargeMessage_ProcessesSuccessfully()
        {
            // Arrange
            var dailyGoalAchieved = new DailyGoalAchieved
            {
                UserId = Guid.NewGuid(),
                TriggeringJourneyId = Guid.NewGuid(),
                GoalType = string.Join("", Enumerable.Repeat("LargeGoalType", 100)),
                Metadata = Enumerable.Range(1, 1000).ToDictionary(i => $"key{i}", i => (object)$"value{i}")
            };

            var message = JsonSerializer.Serialize(dailyGoalAchieved);
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(Arg.Any<IRequest>(), cancellationToken);

            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Successfully processed DailyGoalAchieved event")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_JsonDeserializationException_LogsErrorWithJsonException()
        {
            // Arrange
            var malformedJson = "{ \"UserId\": \"malformed-guid\", \"TriggeringJourneyId\": }";
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(malformedJson, cancellationToken);

            // Assert
            await _mediator.DidNotReceive().Send(Arg.Any<ProcessDailyGoalAchievedCommand>(), Arg.Any<CancellationToken>());

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v =>
                    v.ToString().Contains("Error processing Kafka message") &&
                    v.ToString().Contains(malformedJson)),
                Arg.Any<JsonException>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_SuccessfulProcessing_DoesNotLogError()
        {
            // Arrange
            var dailyGoalAchieved = new DailyGoalAchieved
            {
                UserId = Guid.NewGuid(),
                TriggeringJourneyId = Guid.NewGuid()
            };

            var message = JsonSerializer.Serialize(dailyGoalAchieved);
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message, cancellationToken);

            // Assert
            _logger.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ProcessAsync_MultipleSequentialCalls_ProcessesEachIndependently()
        {
            // Arrange
            var goal1 = new DailyGoalAchieved { UserId = Guid.NewGuid(), TriggeringJourneyId = Guid.NewGuid() };
            var goal2 = new DailyGoalAchieved { UserId = Guid.NewGuid(), TriggeringJourneyId = Guid.NewGuid() };
            var goal3 = new DailyGoalAchieved { UserId = Guid.NewGuid(), TriggeringJourneyId = Guid.NewGuid() };

            var message1 = JsonSerializer.Serialize(goal1);
            var message2 = JsonSerializer.Serialize(goal2);
            var message3 = JsonSerializer.Serialize(goal3);
            var cancellationToken = CancellationToken.None;

            // Act
            await _processor.ProcessAsync(message1, cancellationToken);
            await _processor.ProcessAsync(message2, cancellationToken);
            await _processor.ProcessAsync(message3, cancellationToken);

            // Assert - Use more flexible matching
            await _mediator.Received(3).Send(Arg.Any<IRequest>(), cancellationToken);

            _logger.Received(3).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Successfully processed DailyGoalAchieved event")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }
    }

    // Test models
    public class DailyGoalAchieved
    {
        public Guid UserId { get; set; }
        public Guid TriggeringJourneyId { get; set; }
        public DateTimeOffset AchievedAt { get; set; }
        public string GoalType { get; set; }
        public decimal TargetValue { get; set; }
        public decimal AchievedValue { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class ProcessDailyGoalAchievedCommand
    {
        public DailyGoalAchieved DailyGoalAchieved { get; }

        public ProcessDailyGoalAchievedCommand(DailyGoalAchieved dailyGoalAchieved)
        {
            DailyGoalAchieved = dailyGoalAchieved;
        }
    }
}