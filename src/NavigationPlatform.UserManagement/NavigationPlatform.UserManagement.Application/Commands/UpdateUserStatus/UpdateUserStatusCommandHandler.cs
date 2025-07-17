using MediatR;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Application.Events;
using NavigationPlatform.UserManagement.Application.Events.UserStatusChanged;
using NavigationPlatform.UserManagement.Domain.Interfaces;

namespace NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuditRepository _userAuditRepository;
    private readonly IOutboxEventRepository _outboxEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxPublisher _outboxPublisher;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    public UpdateUserStatusCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IOutboxPublisher outboxPublisher, IUserAuditRepository userAuditRepository, IOutboxEventRepository outboxEventRepository, IMediator mediator, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _outboxPublisher = outboxPublisher;
        _userAuditRepository = userAuditRepository;
        _outboxEventRepository = outboxEventRepository;
        _mediator = mediator;
        _currentUserService = currentUserService;
    }
    public async Task<UserResponse?> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {

        var adminId = _currentUserService.GetUserId();

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found");

        var oldStatus = user.Status;
        user.UpdateStatus(request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify rest of the system
        await _mediator.Publish(new UserStatusChangedNotification
        {
            UserId = user.Id,
            OldStatus = oldStatus,
            NewStatus = request.Status,
            AdminUserId = adminId,
            Reason = request.Reason
        });

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Status = user.Status,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
