namespace NavigationPlatform.RewardService.Worker.Events;

public class DailyGoalAchieved
{
    public Guid UserId { get; set; }
    public DateTime AchievementDate { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public Guid TriggeringJourneyId { get; set; }
    public DateTime AchievedAt { get; set; }
}