using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Infrastructure.Persistance.Configurations;

public class PublicJourneyConfiguration : IEntityTypeConfiguration<PublicJourney>
{
    public void Configure(EntityTypeBuilder<PublicJourney> builder)
    {
        builder.ToTable("public_journeys");

        builder.HasKey(link => link.Id);

        builder.Property(link => link.Id)
            .IsRequired();

        builder.Property(link => link.JourneyId)
            .IsRequired();

        builder.Property(link => link.PublicToken)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(link => link.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(link => link.CreatedAt)
            .IsRequired();

        builder.Property(link => link.ExpiresAt)
            .IsRequired(false);

        builder.Property(link => link.IsRevoked)
            .IsRequired();

        builder.Property(link => link.RevokedAt)
            .IsRequired(false);

        builder.Property(link => link.AccessCount)
            .IsRequired();

        builder.HasOne(link => link.Journey)
            .WithMany(j => j.PublicLinks)
            .HasForeignKey(link => link.JourneyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
