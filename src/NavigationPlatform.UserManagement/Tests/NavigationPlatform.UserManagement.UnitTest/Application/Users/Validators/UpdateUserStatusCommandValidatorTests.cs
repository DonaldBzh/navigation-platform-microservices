using FluentValidation.TestHelper;
using NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;
using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.UnitTest.Application.Users.Validators;

public class UpdateUserStatusCommandValidatorTests
{
    private readonly UpdateUserStatusCommandValidator _validator;

    public UpdateUserStatusCommandValidatorTests()
    {
        _validator = new UpdateUserStatusCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateUserStatusCommand(
            Guid.Empty,
            UserStatus.Suspended,
            "Reason"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_Status_Is_Invalid()
    {
        var command = new UpdateUserStatusCommand(
            Guid.NewGuid(),
            (UserStatus)999, // Invalid enum value
            "Reason"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Status);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new UpdateUserStatusCommand(
            Guid.NewGuid(),
            UserStatus.Active, // This is valid now
            "Justification"
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
