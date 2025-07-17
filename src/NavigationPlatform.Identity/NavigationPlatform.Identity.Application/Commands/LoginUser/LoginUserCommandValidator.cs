using FluentValidation;

namespace NavigationPlatform.Identity.Application.Commands.LoginUser;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
             .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid email address.");

        RuleFor(c => c.Password)
             .NotEmpty().WithMessage("Password is required.");

    }
}