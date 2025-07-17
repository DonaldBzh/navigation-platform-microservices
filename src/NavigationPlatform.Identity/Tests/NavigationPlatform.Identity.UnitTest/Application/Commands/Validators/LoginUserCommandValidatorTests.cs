using FluentValidation.TestHelper;
using NavigationPlatform.Identity.Application.Commands.LoginUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.UnitTest.Application.Commands.Validators;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_When_EmailAndPassword_AreValid()
    {
        var command = new LoginUserCommand("user@example.com", "SecurePassword123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Password123", "Email is required.")]
    [InlineData("invalid", "Password123", "Email is not a valid email address.")]
    [InlineData("user@example.com", "", "Password is required.")]
    public void Should_Fail_WithExpectedErrors(string email, string password, string expectedMessage)
    {
        var command = new LoginUserCommand(email, password);
        var result = _validator.TestValidate(command);

        if (expectedMessage.Contains("Email"))
            result.ShouldHaveValidationErrorFor(c => c.Email).WithErrorMessage(expectedMessage);
        else
            result.ShouldHaveValidationErrorFor(c => c.Password).WithErrorMessage(expectedMessage);
    }
}
