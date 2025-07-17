using FluentAssertions;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.UnitTest.Domain.Entities;

public class UserTests
{
    [Fact]
    public void UpdateStatus_Should_Set_UpdatedAt_And_Change_Status()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Status = UserStatus.Active,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var beforeUpdate = DateTime.UtcNow;
        var newStatus = UserStatus.Suspended;

        // Act
        user.UpdateStatus(newStatus);

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeAfter(beforeUpdate);
    }
}
