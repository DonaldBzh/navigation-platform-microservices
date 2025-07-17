namespace NavigationPlatform.RewardService.Worker.Configuration;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int DefaultTtlMinutes { get; set; } = 1440; // 24 hours
    public int DailyTotalsTtlHours { get; set; } = 25;
}