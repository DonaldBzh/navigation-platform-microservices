using Microsoft.EntityFrameworkCore;
using NavigationPlatform.RewardService.Worker.Entities;
using NavigationPlatform.RewardService.Worker.Persistence;

namespace NavigationPlatform.RewardService.Worker.Repositories;

public class DailyGoalAchievementRepository : IDailyGoalAchievementRepository
{
    private readonly RewardDbContext _context;

    public DailyGoalAchievementRepository(RewardDbContext context)
    {
        _context = context;
    }

    public async Task<DailyGoalAchievement?> GetByUserAndDateAsync(Guid userId, DateTime date, CancellationToken token = default)
    {
        var dateOnly = date.Date;
        return await _context.DailyGoalAchievements
            .FirstOrDefaultAsync(a => a.UserId == userId && a.AchievementDate == dateOnly);
    }

    public async Task<List<DailyGoalAchievement>> GetByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _context.DailyGoalAchievements
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AchievementDate)
            .ToListAsync();
    }

    public void Create(DailyGoalAchievement achievement, CancellationToken token = default)
    {
        _context.DailyGoalAchievements.Add(achievement);
    }

    public async Task<bool> ExistsForUserAndDateAsync(Guid userId, DateTime date, CancellationToken token = default)
    {
        var dateOnly = date.Date;
        return await _context.DailyGoalAchievements
            .AnyAsync(a => a.UserId == userId && a.AchievementDate == dateOnly);
    }
}
