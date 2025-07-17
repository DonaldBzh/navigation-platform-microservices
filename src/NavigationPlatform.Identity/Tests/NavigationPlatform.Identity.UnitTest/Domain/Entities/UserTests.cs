using FluentAssertions;
using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Domain.Entities;


public class UserTests
{
    [Fact]
    public void Create_ShouldInitializeAllFieldsCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed123";
        var role = UserRole.Admin;

        // Act
        var user = User.Create(email, passwordHash, role);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
        user.Status.Should().Be(UserStatus.Active);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
        user.RefreshTokens.Should().BeEmpty();
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndUpdatedAt_WhenDifferent()
    {
        // Arrange
        var user = User.Create("user@example.com", "pwd");
        var newStatus = UserStatus.Suspended;

        // Act
        user.ChangeStatus(newStatus, Guid.NewGuid());

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeStatus_ShouldNotUpdate_WhenSameStatus()
    {
        // Arrange
        var user = User.Create("user@example.com", "pwd");
        var initialUpdatedAt = user.UpdatedAt;

        // Act
        user.ChangeStatus(UserStatus.Active, Guid.NewGuid());

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.UpdatedAt.Should().Be(initialUpdatedAt); // Should still be null
    }
}
