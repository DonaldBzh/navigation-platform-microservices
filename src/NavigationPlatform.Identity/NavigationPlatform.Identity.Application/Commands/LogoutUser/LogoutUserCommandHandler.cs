using MediatR;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.Identity.Application.Commands.LogoutUser;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserCommandHandler(
        ITokenRepository tokenRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _tokenRepository = tokenRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var token = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);

        if (token == null || token.UserId != _currentUserService.GetUserId())
            throw new NotFoundException("Token not found or does not belong to the user.");

        token.Revoke();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return;
    }
}
