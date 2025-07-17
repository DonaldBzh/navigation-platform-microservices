using MediatR;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.Identity.Application.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserStatusCommandHandler(
        IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user is null)
            throw new NotFoundException("User Not Found");

        user.Status = request.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}