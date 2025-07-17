using FluentAssertions;
using NavigationPlatform.Identity.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Security;

public class Sha256PasswordHasherTests
{
    private readonly Sha256PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "MySecret123!";

        // Act
        var hash = _hasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_SameInput_ShouldReturnSameHash()
    {
        var hash1 = _hasher.Hash("password");
        var hash2 = _hasher.Hash("password");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_DifferentInput_ShouldReturnDifferentHash()
    {
        var hash1 = _hasher.Hash("password1");
        var hash2 = _hasher.Hash("password2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var password = "Pa$$word!";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ShouldReturnFalse()
    {
        var correctPassword = "correct";
        var wrongPassword = "wrong";
        var hash = _hasher.Hash(correctPassword);

        var result = _hasher.Verify(wrongPassword, hash);

        result.Should().BeFalse();
    }
}
