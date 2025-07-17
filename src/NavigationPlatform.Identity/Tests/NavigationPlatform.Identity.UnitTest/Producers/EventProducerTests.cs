using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Infrastructure.Services.Consumers;
using NavigationPlatform.Identity.Infrastructure.Services.Kafka;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text;
using System.Text.Json;

namespace NavigationPlatform.Identity.UnitTest.Producers;

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

        _eventProducer = new EventProducer(_kafkaFactory, _logger);
    }

    [Fact]
    public void Constructor_CallsCreateProducerOnFactory()
    {
        // Act is in constructor
        // Assert
        _kafkaFactory.Received(1).CreateProducer();
    }

    [Fact]
    public async Task PublishAsync_WithStringMessage_PublishesDirectlyWithoutSerialization()
    {
        // Arrange
        var topic = "test-topic";
        var message = "test message";
        var deliveryResult = new DeliveryResult<string, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", 0, 123)
        };

        _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
            .Returns(deliveryResult);

        // Act
        await _eventProducer.PublishAsync(topic, message);

        // Assert
        await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
            m.Value == message &&
            !string.IsNullOrEmpty(m.Key) &&
            m.Headers.Count == 2 &&
            m.Headers.Any(h => h.Key == "event-version") &&
            m.Headers.Any(h => h.Key == "timestamp")));
    }

    [Fact]
    public async Task PublishAsync_WithObjectMessage_SerializesToJson()
    {
        // Arrange
        var topic = "test-topic";
        var message = new { Id = 1, Name = "Test" };
        var expectedJson = JsonSerializer.Serialize(message);
        var deliveryResult = new DeliveryResult<string, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", 0, 456)
        };

        _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
            .Returns(deliveryResult);

        // Act
        await _eventProducer.PublishAsync(topic, message);

        // Assert
        await _producer.Received(1).ProduceAsync(topic, Arg.Is<Message<string, string>>(m =>
            m.Value == expectedJson &&
            !string.IsNullOrEmpty(m.Key)));
    }

    [Fact]
    public async Task PublishAsync_GeneratesUniqueKeyForEachMessage()
    {
        // Arrange
        var topic = "test-topic";
        var message = "test message";
        var deliveryResult = new DeliveryResult<string, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", 0, 123)
        };

        _producer.ProduceAsync(topic, Arg.Any<Message<string, string>>())
            .Returns(deliveryResult);

        var capturedKeys = new List<string>();
        await _producer.ProduceAsync(topic, Arg.Do<Message<string, string>>(m => capturedKeys.Add(m.Key)));

        // Act
        await _eventProducer.PublishAsync(topic, message);
        await _eventProducer.PublishAsync(topic, message);

        // Assert
        capturedKeys.Should().HaveCount(2);
        capturedKeys[0].Should().NotBe(capturedKeys[1]);
        capturedKeys.Should().AllSatisfy(key => Guid.TryParse(key, out _).Should().BeTrue());
    }

    [Fact]
    public async Task PublishAsync_AddsCorrectHeaders()
    {
        // Arrange
        var topic = "test-topic";
        var message = "test message";
        var deliveryResult = new DeliveryResult<string, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", 0, 123)
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
    public async Task PublishAsync_WithNullMessage_ThrowsArgumentException()
    {
        // Arrange
        var topic = "test-topic";
        string message = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventProducer.PublishAsync(topic, message));
    }

    [Fact]
    public void Dispose_DisposesProducer()
    {
        // Act
        _eventProducer.Dispose();

        // Assert
        _producer.Received(1).Dispose();
    }


    [Fact]
    public void Dispose_WhenProducerIsNull_DoesNotThrow()
    {
        // Arrange
        var eventProducerWithNullProducer = new EventProducer(_kafkaFactory, _logger);
        _kafkaFactory.CreateProducer().Returns((IProducer<string, string>)null);

        // Act & Assert
        var exception = Record.Exception(() => eventProducerWithNullProducer.Dispose());
        exception.Should().BeNull();
    }

    public void Dispose()
    {
        _eventProducer?.Dispose();
    }
}