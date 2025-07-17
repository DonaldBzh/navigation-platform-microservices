using FluentAssertions;
using NavigationPlatform.Identity.Domain.Entities;

namespace NavigationPlatform.Identity.UnitTest.Domain.Entities
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Create_ShouldInitializeAllPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "abc123";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            // Act
            var refreshToken = RefreshToken.Create(userId, token, expiresAt);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Id.Should().NotBeEmpty();
            refreshToken.UserId.Should().Be(userId);
            refreshToken.Token.Should().Be(token);
            refreshToken.ExpiresAt.Should().Be(expiresAt);
            refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            refreshToken.Revoked.Should().BeFalse();
            refreshToken.RevokedAt.Should().BeNull();
        }

        [Fact]
        public void Revoke_ShouldSetRevokedToTrue_AndSetRevokedAt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "test-token";
            var expiresAt = DateTime.UtcNow.AddDays(1);
            var refreshToken = RefreshToken.Create(userId, token, expiresAt);

            // Act
            refreshToken.Revoke();

            // Assert
            refreshToken.Revoked.Should().BeTrue();
            refreshToken.RevokedAt.Should().NotBeNull();
            refreshToken.RevokedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Revoke_ShouldNotChangeRevokedAt_IfAlreadyRevoked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "test-token";
            var expiresAt = DateTime.UtcNow.AddDays(1);
            var refreshToken = RefreshToken.Create(userId, token, expiresAt);

            refreshToken.Revoke();
            var firstRevokedAt = refreshToken.RevokedAt;

            // Wait briefly to simulate delay
            System.Threading.Thread.Sleep(100);

            // Act
            refreshToken.Revoke(); // call again

            // Assert
            refreshToken.Revoked.Should().BeTrue();
            refreshToken.RevokedAt.Should().Be(firstRevokedAt); // Should not have changed
        }
    }

}