using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.Identity.Domain.Entities;

namespace NavigationPlatform.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
               .IsRequired();

        builder.HasIndex(rt => rt.Token)
               .IsUnique();

        builder.Property(rt => rt.ExpiresAt)
               .IsRequired();

        builder.Property(rt => rt.CreatedAt)
               .IsRequired();

        builder.Property(rt => rt.Revoked)
               .HasDefaultValue(false);

        builder.HasOne(rt => rt.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

    }
}