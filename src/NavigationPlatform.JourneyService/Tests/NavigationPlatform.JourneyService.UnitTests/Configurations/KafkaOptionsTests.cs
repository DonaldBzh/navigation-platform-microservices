using FluentAssertions;
using NavigationPlatform.JourneyService.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.UnitTests.Configurations;

public class KafkaOptionsTests
{
    [Fact]
    public void KafkaOptions_Should_Have_DefaultValues()
    {
        var options = new KafkaOptions();

        options.BootstrapServers.Should().BeEmpty();
        options.GroupId.Should().BeEmpty();

        options.Topics.Should().NotBeNull();
        options.Topics.JourneyCreated.Should().Be("journey-created");
        options.Topics.DailyGoalAchieved.Should().Be("daily-goal-achieved");

        options.Consumer.Should().NotBeNull();
        options.Consumer.AutoOffsetReset.Should().Be("Earliest");
        options.Consumer.EnableAutoCommit.Should().BeTrue();
        options.Consumer.AutoCommitIntervalMs.Should().Be(5000);
        options.Consumer.SessionTimeoutMs.Should().Be(10000);
        options.Consumer.HeartbeatIntervalMs.Should().Be(3000);

        options.Producer.Should().NotBeNull();
        options.Producer.Acks.Should().Be("All");
        options.Producer.Retries.Should().Be(3);
        options.Producer.EnableIdempotence.Should().BeTrue();
        options.Producer.MessageTimeoutMs.Should().Be(10000);
    }
}
