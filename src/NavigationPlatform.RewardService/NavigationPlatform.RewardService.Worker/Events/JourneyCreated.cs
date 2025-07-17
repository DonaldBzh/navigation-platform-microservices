namespace NavigationPlatform.RewardService.Worker.Events;

public class JourneyCreated
{
    public Guid JourneyId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal DistanceKm { get; set; }
    public DateTime CreatedAt { get; set; }
}
