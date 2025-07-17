using FluentValidation;
using System.Text.RegularExpressions;


namespace NavigationPlatform.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
             .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid email address.");


        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasLowerChar = new Regex(@"[a-z]+");
        var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        RuleFor(c => c.Password)
             .NotEmpty().WithMessage("Password is required.")
             .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
             .Matches(hasNumber).WithMessage("Password must contain at least one number.")
             .Matches(hasUpperChar).WithMessage("Password must contain at least one uppercase character.")
             .Matches(hasLowerChar).WithMessage("Password must contain at least one lowercase character.")
             .Matches(hasSymbols).WithMessage("Password must contain at least one special character.");

    }
}
