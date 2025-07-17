using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.RewardService.Worker.Entities;

namespace NavigationPlatform.RewardService.Worker.Persistence.Configurations;

public class DailyGoalAchievementConfiguration : IEntityTypeConfiguration<DailyGoalAchievement>
{
    public void Configure(EntityTypeBuilder<DailyGoalAchievement> builder)
    {
        builder.ToTable("daily_goal_achievements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.AchievementDate)
            .HasColumnName("achievement_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.TotalDistanceKm)
            .HasColumnName("total_distance_km")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(e => e.TriggeringJourneyId)
            .HasColumnName("triggering_journey_id")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.AchievementDate })
            .IsUnique()
            .HasDatabaseName("idx_user_achievement_date_unique");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_daily_goal_achievements_user_id");

        builder.HasIndex(e => e.AchievementDate)
            .HasDatabaseName("idx_daily_goal_achievements_date");

    }
}
