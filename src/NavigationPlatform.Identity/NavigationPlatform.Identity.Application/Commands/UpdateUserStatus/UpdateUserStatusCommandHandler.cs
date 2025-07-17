using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.Identity.Application.Commands.UpdateUserStatus;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand>
{
    private readonly ILogger<UpdateUserStatusCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserStatusCommandHandler(
        ILogger<UpdateUserStatusCommandHandler> logger,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            return;
        }

        user.ChangeStatus(request.NewStatus, request.AdminUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User status updated in Identity Service for user: {UserId}", user.Id);
    }
}