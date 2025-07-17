using NavigationPlatform.RewardService.Worker.Events;

namespace NavigationPlatform.RewardService.Worker.Services;

public interface IEventProducer
{
    Task PublishDailyGoalAchievedAsync(DailyGoalAchieved eventData);
}
