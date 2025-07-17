using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Domain.Events;
using NavigationPlatform.Identity.Infrastructure.Services.Consumers;
using NavigationPlatform.Identity.Infrastructure.Services.Kafka;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Consumers
{
    public class UserStatusChangedConsumerTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldProcessConsumedMessage()
        {
            // Arrange
            var kafkaOptions = new KafkaOptions
            {
                Topics = new KafkaTopicsOptions { UserStatusChanged = "user-status-changed" }
            };

            var consumer = Substitute.For<IConsumer<string, string>>();
            var factory = Substitute.For<IKafkaClientFactory>();
            factory.CreateConsumer().Returns(consumer);

            var mockProcessor = Substitute.For<IUserStatusChangedProcessor>();

            var scope = Substitute.For<IServiceScope>();
            var scopeFactory = Substitute.For<IServiceScopeFactory>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            scope.ServiceProvider.Returns(serviceProvider);
            scopeFactory.CreateScope().Returns(scope);
            serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
            serviceProvider.GetService(typeof(IUserStatusChangedProcessor)).Returns(mockProcessor);

            var logger = Substitute.For<ILogger<UserStatusChangedConsumer>>();
            var options = Microsoft.Extensions.Options.Options.Create(kafkaOptions);

            var messageValue = JsonSerializer.Serialize(new UserStatusChangedEvent
            {
                UserId = Guid.NewGuid(),
                OldStatus = Identity.Domain.Enums.UserStatus.Active,
                NewStatus = Identity.Domain.Enums.UserStatus.Deactivated,
                Reason = "Reason",
                AdminUserId = Guid.NewGuid()
            });

            consumer.Consume(Arg.Any<TimeSpan>())
                    .Returns(new ConsumeResult<string, string>
                    {
                        Message = new Message<string, string> { Value = messageValue }
                    },
                    (ConsumeResult<string, string>)null!); // to break the loop on next iteration

            var sut = new UserStatusChangedConsumer(options, factory, logger, serviceProvider);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // Act
            await sut.StartAsync(cts.Token);

            // Assert
            await mockProcessor.Received(1).ProcessAsync(messageValue, Arg.Any<CancellationToken>());
        }
    }
}
