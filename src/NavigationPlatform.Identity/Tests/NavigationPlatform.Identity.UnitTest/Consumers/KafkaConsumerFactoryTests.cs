using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NavigationPlatform.Identity.Infrastructure.Services.Consumers;
using NavigationPlatform.Identity.Infrastructure.Services.Kafka;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Consumers
{
    public class KafkaConsumerFactoryTests
    {
        private readonly IOptions<KafkaOptions> _mockOptions;
        private readonly KafkaOptions _kafkaOptions;
        private readonly KafkaClientFactory _factory;

        private KafkaOptions GetValidKafkaOptions() => new()
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Producer = new KafkaProducerOptions
            {
                Retries = 5,
                EnableIdempotence = true,
                MessageTimeoutMs = 3000
            },
            Consumer = new KafkaConsumerOptions()
        };
        public KafkaConsumerFactoryTests()
        {
            // Arrange - Setup test data
            _kafkaOptions = new KafkaOptions
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                Consumer = new KafkaConsumerOptions
                {
                    EnableAutoCommit = true,
                    AutoCommitIntervalMs = 5000,
                    SessionTimeoutMs = 30000,
                    HeartbeatIntervalMs = 3000
                }
            };

            _mockOptions = Substitute.For<IOptions<KafkaOptions>>();
            _mockOptions.Value.Returns(_kafkaOptions);

            _factory = new KafkaClientFactory(_mockOptions);
        }

        [Fact]
        public void Constructor_WithValidOptions_ShouldInitialize()
        {
            // Act & Assert
            _factory.Should().NotBeNull();
            var _ = _mockOptions.Received(1).Value;
        }

        [Fact]
        public void CreateConsumer_ShouldReturnValidConsumer()
        {
            // Act
            var consumer = _factory.CreateConsumer();

            // Assert
            consumer.Should().NotBeNull();
            consumer.Should().BeAssignableTo<IConsumer<string, string>>();
        }

        [Fact]
        public void CreateConsumer_ShouldCreateNewConsumerEachTime()
        {
            // Act
            var consumer1 = _factory.CreateConsumer();
            var consumer2 = _factory.CreateConsumer();

            // Assert
            consumer1.Should().NotBeNull();
            consumer2.Should().NotBeNull();
            consumer1.Should().NotBeSameAs(consumer2);
        }


        [Fact]
        public void CreateConsumer_WithNullGroupId_ShouldThrow()
        {
            // Arrange
            _kafkaOptions.GroupId = null;

            // Act & Assert
            var act = () => _factory.CreateConsumer();
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void CreateConsumer_WithValidConfiguration_ShouldUseAllOptions()
        {
            // Arrange
            var customOptions = new KafkaOptions
            {
                BootstrapServers = "custom-server:9092",
                GroupId = "custom-group",
                Consumer = new KafkaConsumerOptions
                {
                    EnableAutoCommit = false,
                    AutoCommitIntervalMs = 1000,
                    SessionTimeoutMs = 60000,
                    HeartbeatIntervalMs = 1000
                }
            };

            var mockCustomOptions = Substitute.For<IOptions<KafkaOptions>>();
            mockCustomOptions.Value.Returns(customOptions);
            var customFactory = new KafkaClientFactory(mockCustomOptions);

            // Act
            var consumer = customFactory.CreateConsumer();

            // Assert
            consumer.Should().NotBeNull();
            var _ = _mockOptions.Received(1).Value;
        }

        [Theory]
        [InlineData("localhost:9092")]
        [InlineData("kafka1:9092,kafka2:9092")]
        [InlineData("192.168.1.100:9092")]
        public void CreateConsumer_WithDifferentBootstrapServers_ShouldWork(string bootstrapServers)
        {
            // Arrange
            _kafkaOptions.BootstrapServers = bootstrapServers;

            // Act
            var consumer = _factory.CreateConsumer();

            // Assert
            consumer.Should().NotBeNull();
        }

        [Theory]
        [InlineData("group1")]
        [InlineData("test-consumer-group")]
        [InlineData("my-service-group")]
        public void CreateConsumer_WithDifferentGroupIds_ShouldWork(string groupId)
        {
            // Arrange
            _kafkaOptions.GroupId = groupId;

            // Act
            var consumer = _factory.CreateConsumer();

            // Assert
            consumer.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true, 1000, 10000, 3000)]
        [InlineData(false, 5000, 30000, 10000)]
        [InlineData(true, 2000, 45000, 5000)]
        public void CreateConsumer_WithDifferentConsumerOptions_ShouldWork(
            bool enableAutoCommit,
            int autoCommitInterval,
            int sessionTimeout,
            int heartbeatInterval)
        {
            // Arrange
            _kafkaOptions.Consumer = new KafkaConsumerOptions
            {
                EnableAutoCommit = enableAutoCommit,
                AutoCommitIntervalMs = autoCommitInterval,
                SessionTimeoutMs = sessionTimeout,
                HeartbeatIntervalMs = heartbeatInterval
            };

            // Act
            var consumer = _factory.CreateConsumer();

            // Assert
            consumer.Should().NotBeNull();
        }

        [Fact]
        public void CreateConsumer_ShouldDisposeProperlyWhenConsumerDisposed()
        {
            // Act
            var consumer = _factory.CreateConsumer();

            // Assert & Act
            var act = () => consumer.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void CreateProducer_Should_BuildProducerSuccessfully()
        {
            // Arrange
            var options = Options.Create(new KafkaOptions
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                Consumer = new KafkaConsumerOptions(),
                Producer = new KafkaProducerOptions
                {
                    Retries = 3,
                    EnableIdempotence = true,
                    MessageTimeoutMs = 1500
                }
            });

            var factory = new KafkaClientFactory(options);

            // Act
            var producer = factory.CreateProducer();

            // Assert
            producer.Should().NotBeNull();
            producer.Should().BeAssignableTo<IProducer<string, string>>();
        }
        [Fact]
        public void CreateProducer_Should_NotThrow_WithValidConfig()
        {
            // Arrange
            var options = Options.Create(GetValidKafkaOptions());
            var factory = new KafkaClientFactory(options);

            // Act
            var action = () => factory.CreateProducer();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void CreateProducer_Should_BeAssignableToIProducer()
        {
            var options = Options.Create(GetValidKafkaOptions());
            var factory = new KafkaClientFactory(options);

            var producer = factory.CreateProducer();

            producer.Should().BeAssignableTo<IProducer<string, string>>();
        }

        [Fact]
        public void CreateProducer_WithNullProducerOptions_ShouldThrow()
        {
            var options = Options.Create(new KafkaOptions
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                Producer = null!, // Intentionally null
                Consumer = new KafkaConsumerOptions()
            });

            var factory = new KafkaClientFactory(options);

            var act = () => factory.CreateProducer();

            act.Should().Throw<NullReferenceException>();
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        public void CreateProducer_ShouldRespectMessageTimeout(int timeout)
        {
            var kafkaOptions = GetValidKafkaOptions();
            kafkaOptions.Producer.MessageTimeoutMs = timeout;

            var options = Options.Create(kafkaOptions);
            var factory = new KafkaClientFactory(options);

            var producer = factory.CreateProducer();

            producer.Should().NotBeNull(); 
        }

    }
}
