using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.Identity.Domain.Entities;

namespace NavigationPlatform.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email)
                .IsUnique();

        builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
                .IsRequired();

        builder.Property(u => u.Status)
                .IsRequired();

        builder.Property(u => u.CreatedAt)
                .IsRequired();

        builder.Property(u => u.Role)
                .HasConversion<string>()
           .    IsRequired();
    }
}
