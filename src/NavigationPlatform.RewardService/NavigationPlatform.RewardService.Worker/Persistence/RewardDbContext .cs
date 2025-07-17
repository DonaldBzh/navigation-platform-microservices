using Microsoft.EntityFrameworkCore;
using NavigationPlatform.RewardService.Worker.Entities;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.RewardService.Worker.Persistence;

public class RewardDbContext : DbContext, IUnitOfWork
{
    public RewardDbContext(DbContextOptions<RewardDbContext> options) : base(options)
    {
    }

    public DbSet<DailyGoalAchievement> DailyGoalAchievements => Set<DailyGoalAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RewardDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ApplicationException("Concurrency exception occurred.", ex);
        }
    }
}