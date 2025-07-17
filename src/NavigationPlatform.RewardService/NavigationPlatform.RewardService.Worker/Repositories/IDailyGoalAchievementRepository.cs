using NavigationPlatform.RewardService.Worker.Entities;

namespace NavigationPlatform.RewardService.Worker.Repositories;

public interface IDailyGoalAchievementRepository
{
    Task<DailyGoalAchievement?> GetByUserAndDateAsync(Guid userId, DateTime date, CancellationToken token = default);
    Task<List<DailyGoalAchievement>> GetByUserIdAsync(Guid userId, CancellationToken token = default);
    void Create(DailyGoalAchievement achievement, CancellationToken token = default);
    Task<bool> ExistsForUserAndDateAsync(Guid userId, DateTime date, CancellationToken token = default);
}
