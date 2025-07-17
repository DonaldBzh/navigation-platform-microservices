using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Infrastructure.Persistance.Configurations;

public class JourneyConfiguration : IEntityTypeConfiguration<Journey>
{
    public void Configure(EntityTypeBuilder<Journey> builder)
    {
        builder.ToTable("journeys");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .IsRequired();

        builder.Property(j => j.Name)
            .IsRequired();

        builder.Property(j => j.UserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(j => j.StartLocation)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.StartLatitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(j => j.StartLongitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(j => j.StartDate)
            .IsRequired();

        builder.Property(j => j.ArrivalLocation)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.ArrivalLatitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(j => j.ArrivalLongitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(j => j.ArrivalDate);

        builder.Property(j => j.TransportationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(j => j.RouteDistanceKm)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(j => j.Notes)
            .HasMaxLength(1000);

        builder.Property(j => j.IsDailyGoalAchieved)
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.IsDeleted)
            .IsRequired();

        // Relationships
        builder.HasMany(j => j.Shares)
            .WithOne()
            .HasForeignKey("JourneyId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.PublicLinks)
            .WithOne()
            .HasForeignKey("JourneyId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.AuditLogs)
            .WithOne()
            .HasForeignKey("JourneyId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
