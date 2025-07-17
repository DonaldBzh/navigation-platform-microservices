using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;
using NavigationPlatform.JourneyService.Infrastructure.Services.Producers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NavigationPlatform.JourneyService.UnitTests.Services
{
    public class EventProducerTests
    {
        private readonly IKafkaClientFactory _kafkaFactory;
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<EventProducer> _logger;
        private readonly EventProducer _eventProducer;

        public EventProducerTests()
        {
            _kafkaFactory = Substitute.For<IKafkaClientFactory>();
            _producer = Substitute.For<IProducer<string, string>>();
            _logger = Substitute.For<ILogger<EventProducer>>();

            _kafkaFactory.CreateProducer().Returns(_producer);

            _eventProducer = new EventProducer(_logger, _kafkaFactory);
        }

        [Fact]
        public void Constructor_InitializesAllDependencies()
        {
            // Act is in constructor

            // Assert
            _kafkaFactory.Received(1).CreateProducer();
        }

        [Fact]
        public async Task PublishAsync_WithStringMessage_UsesCorrectTopicParameter()
        {
            // Arrange
            var topic = "string-message-topic";
            var message = "test string message";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
                m.Value == message &&
                !string.IsNullOrEmpty(m.Key)));
        }

        [Fact]
        public async Task PublishAsync_WithObjectMessage_SerializesToJsonAndUsesCorrectTopic()
        {
            // Arrange
            var topic = "object-message-topic";
            var message = new { Id = 42, Name = "Test Object", IsActive = true };
            var expectedJson = JsonSerializer.Serialize(message);
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 456)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
                m.Value == expectedJson));
        }

        [Fact]
        public async Task PublishAsync_GeneratesUniqueGuidKeys()
        {
            // Arrange
            var topic = "unique-keys-topic";
            var message = "test";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            var capturedKeys = new List<string>();

            // Capture keys from multiple calls
            await _producer.ProduceAsync(topic, Arg.Do<Message<string, string>>(m => capturedKeys.Add(m.Key)));

            // Act
            await _eventProducer.PublishAsync(topic, message);
            await _eventProducer.PublishAsync(topic, message);
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            capturedKeys.Should().HaveCount(3);
            capturedKeys.Should().OnlyHaveUniqueItems();
            capturedKeys.Should().AllSatisfy(key => Guid.TryParse(key, out _).Should().BeTrue());
        }

        [Fact]
        public async Task PublishAsync_AddsCorrectHeaders_EventVersionAndTimestamp()
        {
            // Arrange
            var topic = "headers-topic";
            var message = "test";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            Headers capturedHeaders = null;
            await _producer.ProduceAsync(topic, Arg.Do<Message<string, string>>(m => capturedHeaders = m.Headers));

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            capturedHeaders.Should().NotBeNull();
            capturedHeaders.Should().HaveCount(2);

            var eventVersionHeader = capturedHeaders.FirstOrDefault(h => h.Key == "event-version");
            eventVersionHeader.Should().NotBeNull();
            Encoding.UTF8.GetString(eventVersionHeader.GetValueBytes()).Should().Be("1.0");

            var timestampHeader = capturedHeaders.FirstOrDefault(h => h.Key == "timestamp");
            timestampHeader.Should().NotBeNull();
            var timestampValue = Encoding.UTF8.GetString(timestampHeader.GetValueBytes());
            DateTimeOffset.TryParse(timestampValue, out var parsedTimestamp).Should().BeTrue();
            parsedTimestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task PublishAsync_OnSuccess_LogsInformationWithCorrectTopicAndOffset()
        {
            // Arrange
            var topic = "success-log-topic";
            var message = "test";
            var expectedOffset = new Offset(789);
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, expectedOffset)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Published message to Kafka topic") &&
                                   v.ToString().Contains(topic) &&
                                   v.ToString().Contains(expectedOffset.ToString())),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task PublishAsync_OnProduceException_LogsErrorWithCorrectTopicAndRethrows()
        {
            // Arrange
            var topic = "error-topic";
            var message = "test";
            var produceException = new ProduceException<string, string>(
                new Error(ErrorCode.Local_MsgTimedOut, "Timeout"),
                new DeliveryResult<string, string>());

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Throws(produceException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ProduceException<string, string>>(
                () => _eventProducer.PublishAsync(topic, message));

            exception.Should().Be(produceException);

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Error publishing message to Kafka topic") &&
                                   v.ToString().Contains(topic)),
                produceException,
                Arg.Any<Func<object, Exception, string>>());
        }

        [Theory]
        [InlineData("simple-topic")]
        [InlineData("topic.with.dots")]
        [InlineData("topic-with-dashes")]
        [InlineData("topic_with_underscores")]
        [InlineData("TopicWithCamelCase")]
        [InlineData("very-long-topic-name-that-exceeds-normal-length-expectations-but-should-still-work")]
        public async Task PublishAsync_WithVariousTopicNames_UsesCorrectTopicParameter(string topic)
        {
            // Arrange
            var message = "test";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Any<Message<string, string>>());
        }

        [Fact]
        public async Task PublishAsync_WithComplexNestedObject_SerializesCorrectly()
        {
            // Arrange
            var topic = "complex-object-topic";
            var complexMessage = new ComplexTestObject
            {
                Id = Guid.NewGuid(),
                Name = "Complex Object",
                Metadata = new Dictionary<string, object>
                {
                    { "version", 2.1 },
                    { "tags", new[] { "urgent", "customer-facing" } },
                    { "config", new { enabled = true, maxRetries = 3 } }
                },
                CreatedAt = DateTimeOffset.UtcNow,
                Items = new List<string> { "item1", "item2", "item3" }
            };

            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, complexMessage);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
                m.Value.Contains(complexMessage.Id.ToString()) &&
                m.Value.Contains(complexMessage.Name) &&
                m.Value.Contains("urgent") &&
                m.Value.Contains("customer-facing") &&
                !string.IsNullOrEmpty(m.Value)));
        }

        [Fact]
        public async Task PublishAsync_WithEmptyStringMessage_ProcessesCorrectly()
        {
            // Arrange
            var topic = "empty-string-topic";
            var emptyMessage = "";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, emptyMessage);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
                m.Value == emptyMessage));
        }

        [Fact]
        public async Task PublishAsync_WithDifferentTopicsInSequence_UsesCorrectTopicForEach()
        {
            // Arrange
            var topic1 = "first-topic";
            var topic2 = "second-topic";
            var topic3 = "third-topic";
            var message = "test";

            var deliveryResult1 = new DeliveryResult<string, string> { TopicPartitionOffset = new TopicPartitionOffset(topic1, 0, 1) };
            var deliveryResult2 = new DeliveryResult<string, string> { TopicPartitionOffset = new TopicPartitionOffset(topic2, 0, 2) };
            var deliveryResult3 = new DeliveryResult<string, string> { TopicPartitionOffset = new TopicPartitionOffset(topic3, 0, 3) };

            _producer.ProduceAsync(topic1, Arg.Any<Message<string, string>>()).Returns(deliveryResult1);
            _producer.ProduceAsync(topic2, Arg.Any<Message<string, string>>()).Returns(deliveryResult2);
            _producer.ProduceAsync(topic3, Arg.Any<Message<string, string>>()).Returns(deliveryResult3);

            // Act
            await _eventProducer.PublishAsync(topic1, message);
            await _eventProducer.PublishAsync(topic2, message);
            await _eventProducer.PublishAsync(topic3, message);

            // Assert
            await _producer.Received(1).ProduceAsync(topic1, Arg.Any<Message<string, string>>());
            await _producer.Received(1).ProduceAsync(topic2, Arg.Any<Message<string, string>>());
            await _producer.Received(1).ProduceAsync(topic3, Arg.Any<Message<string, string>>());
        }

        [Fact]
        public async Task PublishAsync_ProducerReturnsNullResult_ThrowsNullReferenceException()
        {
            // Arrange
            var topic = "null-result-topic";
            var message = "test";

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns((DeliveryResult<string, string>)null);

            // Act & Assert - This exposes a bug in the production code
            var exception = await Assert.ThrowsAsync<NullReferenceException>(
                () => _eventProducer.PublishAsync(topic, message));

            await _producer.Received(1).ProduceAsync(topic, Arg.Any<Message<string, string>>());

            // The logger should not be called because the exception occurs before logging
            _logger.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        public async Task PublishAsync_WithEmptyOrWhitespaceTopics_StillCallsProducer(string topic)
        {
            // Arrange
            var message = "test";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, message);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Any<Message<string, string>>());
        }

        [Fact]
        public async Task PublishAsync_WithLargeMessage_SerializesSuccessfully()
        {
            // Arrange
            var topic = "large-message-topic";
            var largeMessage = new
            {
                Data = string.Join("", Enumerable.Repeat("Large data content ", 1000)),
                Items = Enumerable.Range(1, 500).Select(i => new { Id = i, Value = $"Item {i}" }).ToList(),
                Metadata = Enumerable.Range(1, 100).ToDictionary(i => $"key{i}", i => $"value{i}")
            };

            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            // Act
            await _eventProducer.PublishAsync(topic, largeMessage);

            // Assert
            await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
                m.Value.Contains("Large data content") &&
                m.Value.Contains("Item 1") &&
                m.Value.Contains("key1") &&
                m.Value.Length > 10000));
        }

        [Fact]
        public async Task PublishAsync_MessageKeyIsDifferentForConcurrentCalls()
        {
            // Arrange
            var topic = "concurrent-topic";
            var message = "test";
            var deliveryResult = new DeliveryResult<string, string>
            {
                TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 123)
            };

            _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
                .Returns(deliveryResult);

            var capturedKeys = new ConcurrentQueue<string>();
            await _producer.ProduceAsync(topic, Arg.Do<Message<string, string>>(m => capturedKeys.Enqueue(m.Key)));

            // Act - Simulate concurrent calls
            var tasks = Enumerable.Range(0, 10).Select(_ => _eventProducer.PublishAsync(topic, message));
            await Task.WhenAll(tasks);

            // Assert
            capturedKeys.Should().HaveCount(10);
            capturedKeys.Should().OnlyHaveUniqueItems();
        }
    }

    // Test models for comprehensive testing
    public class ComplexTestObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<string> Items { get; set; }
    }
}