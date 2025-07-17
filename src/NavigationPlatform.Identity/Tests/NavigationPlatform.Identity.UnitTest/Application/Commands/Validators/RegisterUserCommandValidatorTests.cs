using FluentValidation.TestHelper;
using NavigationPlatform.Identity.Application.Commands.RegisterUser;
using NavigationPlatform.Identity.Domain.Enums;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands.Validators;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_When_EmailAndPassword_AreValid()
    {
        var command = new RegisterUserCommand("user@example.com", "StrongP@ss1",UserRole.User);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "StrongP@ss1", "Email is required.")]
    [InlineData("invalid-email", "StrongP@ss1", "Email is not a valid email address.")]
    [InlineData("user@example.com", "", "Password is required.")]
    [InlineData("user@example.com", "S1!", "Password must be at least 8 characters.")]
    [InlineData("user@example.com", "alllower1!", "Password must contain at least one uppercase character.")]
    [InlineData("user@example.com", "ALLUPPER1!", "Password must contain at least one lowercase character.")]
    [InlineData("user@example.com", "NoNumber!", "Password must contain at least one number.")]
    [InlineData("user@example.com", "NoSpecial1", "Password must contain at least one special character.")]
    public void Should_Fail_WithExpectedError(string email, string password, string expectedMessage)
    {
        // Arrange
        var command = new RegisterUserCommand(email, password, UserRole.User);
        var result = _validator.TestValidate(command);

        // Assert
        if (expectedMessage.Contains("Email"))
            result.ShouldHaveValidationErrorFor(c => c.Email).WithErrorMessage(expectedMessage);
        else
            result.ShouldHaveValidationErrorFor(c => c.Password).WithErrorMessage(expectedMessage);
    }
}