using FluentValidation;

namespace NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(c => c.UserId)
             .NotEmpty().WithMessage("UserId is required.");

        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Invalid UserStatus.");

    }
}