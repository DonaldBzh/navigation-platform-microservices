using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.UserManagement.Domain.Entities;

namespace NavigationPlatform.UserManagement.Infrastructure.Persistance.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{

    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("outbox_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventData)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ProcessedAt);
            
        builder.Property(e => e.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

    }
}

