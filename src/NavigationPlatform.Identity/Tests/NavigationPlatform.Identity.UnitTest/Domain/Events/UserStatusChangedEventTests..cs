using FluentAssertions;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Domain.Events;
using System.Text.Json;

namespace NavigationPlatform.Identity.UnitTest.Domain.Events;

public class UserStatusChangedEventTests
{
    [Fact]
    public void UserStatusChangedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        var original = new UserStatusChangedEvent
        {
            UserId = Guid.NewGuid(),
            AdminUserId = Guid.NewGuid(),
            OldStatus = UserStatus.Deactivated,
            NewStatus = UserStatus.Active,
            Reason = "Reinstated"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<UserStatusChangedEvent>(json);

        deserialized.Should().BeEquivalentTo(original);
    }
}
