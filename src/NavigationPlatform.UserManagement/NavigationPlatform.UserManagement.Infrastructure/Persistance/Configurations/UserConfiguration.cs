using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NavigationPlatform.UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Infrastructure.Persistance.Configurations;


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

        builder.Property(u => u.Status)
                .IsRequired();

        builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();
    }
}




public class UserAuditLogConfiguration : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> builder)
    {
        builder.ToTable("users_audit");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
                .IsRequired();

        builder.Property(u => u.AdminUserId)
                .IsRequired();

        builder.Property(u => u.Action)
                .IsRequired()
                .HasMaxLength(255);

        builder.Property(u => u.NewStatus)
                .IsRequired();

        builder.Property(u => u.Timestamp)
                .IsRequired();

        builder.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
    }
}

