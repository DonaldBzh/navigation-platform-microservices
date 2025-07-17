using MediatR;

namespace NavigationPlatform.Identity.Application.Commands.LoginUser;

public record LoginUserCommand(string Email,string Password) : IRequest<AuthResult>
{
}
