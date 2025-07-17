using NavigationPlatform.Identity.Domain.Entities;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Identity.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Security;

public class TokenGeneratorTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly TokenGenerator _tokenGenerator;

    public TokenGeneratorTests()
    {
        _jwtSettings = new JwtSettings
        {
            Key = "supersecretkeymustbeatleast16characters",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpiryMinutes = 30
        };

        _tokenGenerator = new TokenGenerator(_jwtSettings);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnValidBase64String()
    {
        // Act
        var token = _tokenGenerator.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        var bytes = Convert.FromBase64String(token); // Should not throw
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedPassword", UserRole.Admin);

        // Act
        var (token, expiresAt) = _tokenGenerator.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtSettings.Audience, jwtToken.Audiences.First());
        Assert.True(expiresAt > DateTime.UtcNow);
    }
}
