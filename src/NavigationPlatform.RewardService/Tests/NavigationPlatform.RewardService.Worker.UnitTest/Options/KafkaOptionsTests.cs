using FluentAssertions;
using NavigationPlatform.RewardService.Worker.Configuration;

namespace NavigationPlatform.RewardService.Worker.UnitTest.Options;

public class KafkaOptionsTests
{
    [Fact]
    public void KafkaOptions_Should_Have_Correct_DefaultValues()
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

        options.Producer.Should().NotBeNull();
        options.Producer.Acks.Should().Be("All");
        options.Producer.Retries.Should().Be(3);
        options.Producer.EnableIdempotence.Should().BeTrue();
        options.Producer.MessageTimeoutMs.Should().Be(10000);
    }

    [Fact]
    public void KafkaOptions_SectionName_Should_Be_Kafka()
    {
        KafkaOptions.SectionName.Should().Be("Kafka");
    }

    [Fact]
    public void KafkaOptions_Should_Allow_Overriding_Values()
    {
        var options = new KafkaOptions
        {
            BootstrapServers = "localhost:9092",
            GroupId = "group-1",
            Topics = new KafkaTopicsOptions
            {
                JourneyCreated = "jc-topic",
                DailyGoalAchieved = "dg-topic"
            },
            Consumer = new KafkaConsumerOptions
            {
                AutoOffsetReset = "Latest",
                EnableAutoCommit = false,
                AutoCommitIntervalMs = 1000,
                SessionTimeoutMs = 30000
            },
            Producer = new KafkaProducerOptions
            {
                Acks = "1",
                Retries = 1,
                EnableIdempotence = false,
                MessageTimeoutMs = 3000
            }
        };

        options.BootstrapServers.Should().Be("localhost:9092");
        options.GroupId.Should().Be("group-1");

        options.Topics.JourneyCreated.Should().Be("jc-topic");
        options.Topics.DailyGoalAchieved.Should().Be("dg-topic");

        options.Consumer.AutoOffsetReset.Should().Be("Latest");
        options.Consumer.EnableAutoCommit.Should().BeFalse();
        options.Consumer.AutoCommitIntervalMs.Should().Be(1000);
        options.Consumer.SessionTimeoutMs.Should().Be(30000);

        options.Producer.Acks.Should().Be("1");
        options.Producer.Retries.Should().Be(1);
        options.Producer.EnableIdempotence.Should().BeFalse();
        options.Producer.MessageTimeoutMs.Should().Be(3000);
    }
}
