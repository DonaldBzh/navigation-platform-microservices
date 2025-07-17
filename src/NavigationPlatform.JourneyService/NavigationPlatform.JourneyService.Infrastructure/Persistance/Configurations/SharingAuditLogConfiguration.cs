using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Infrastructure.Persistance.Configurations;

public class SharingAuditLogConfiguration : IEntityTypeConfiguration<SharingAuditLog>
{
    public void Configure(EntityTypeBuilder<SharingAuditLog> builder)
    {
        builder.ToTable("sharing_audit_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id)
            .IsRequired();

        builder.Property(log => log.JourneyId)
            .IsRequired();

        builder.Property(log => log.UserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(log => log.Action)
            .IsRequired();

        builder.Property(log => log.TargetUserId)
            .HasMaxLength(128);

        builder.Property(log => log.Timestamp)
            .IsRequired();

        builder.Property(log => log.IpAddress)
            .HasMaxLength(64);

        builder.Property(log => log.UserAgent)
            .HasMaxLength(512);

        builder.HasOne(log => log.Journey)
            .WithMany(j => j.AuditLogs)
            .HasForeignKey(log => log.JourneyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}