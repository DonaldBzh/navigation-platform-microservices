using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Infrastructure.Persistance.Configurations;

public class JourneyShareConfiguration : IEntityTypeConfiguration<JourneyShare>
{
    public void Configure(EntityTypeBuilder<JourneyShare> builder)
    {
        builder.ToTable("shared_journeys");

        builder.HasKey(js => js.Id);

        builder.Property(js => js.Id)
            .IsRequired();

        builder.Property(js => js.JourneyId)
            .IsRequired();

        builder.Property(js => js.SharedByUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(js => js.SharedWithUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(js => js.SharedAt)
            .IsRequired();

        builder.Property(js => js.IsRevoked)
            .IsRequired();

        builder.Property(js => js.RevokedAt)
            .IsRequired(false);

        builder.HasOne(js => js.Journey)
            .WithMany(j => j.Shares)
            .HasForeignKey(js => js.JourneyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
