using MediatR;
using NavigationPlatform.Identity.Application.Commands.LoginUser;


namespace NavigationPlatform.Identity.Application.Commands.Tokenrefresh;

public record RefreshTokenCommand(string Token) : IRequest<AuthResult>;
