using MediatR;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Interfaces;

namespace NavigationPlatform.UserManagement.Application.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var newUser = new User
        {
            Id = request.UserId,
            Email = request.Email,
            Role = request.Role,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
        };

        _userRepository.AddUser(newUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}