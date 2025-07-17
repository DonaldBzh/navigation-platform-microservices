namespace NavigationPlatform.RewardService.Worker.Configuration;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public KafkaTopicsOptions Topics { get; set; } = new();
    public KafkaConsumerOptions Consumer { get; set; } = new();
    public KafkaProducerOptions Producer { get; set; } = new();
}

public class KafkaTopicsOptions
{
    public string JourneyCreated { get; set; } = "journey-created";
    public string DailyGoalAchieved { get; set; } = "daily-goal-achieved";
}

public class KafkaConsumerOptions
{
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 10000;
}

public class KafkaProducerOptions
{
    public string Acks { get; set; } = "All";
    public int Retries { get; set; } = 3;
    public bool EnableIdempotence { get; set; } = true;
    public int MessageTimeoutMs { get; set; } = 10000;
}
