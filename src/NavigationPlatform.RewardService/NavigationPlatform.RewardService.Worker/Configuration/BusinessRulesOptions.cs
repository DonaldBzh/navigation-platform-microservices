namespace NavigationPlatform.RewardService.Worker.Configuration;

public class BusinessRulesOptions
{
    public const string SectionName = "BusinessRules";

    public decimal DailyGoalThresholdKm { get; set; } = 20.0m;
}