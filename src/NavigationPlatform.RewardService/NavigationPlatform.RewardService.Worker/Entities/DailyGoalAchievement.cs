namespace NavigationPlatform.RewardService.Worker.Entities;

public class DailyGoalAchievement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime AchievementDate { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public Guid TriggeringJourneyId { get; set; }
    public DateTime CreatedAt { get; set; }
}
