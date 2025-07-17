
using MediatR;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.ShareWithUsers;

public class ShareJourneyWithUsersCommandHandler : IRequestHandler<ShareJourneyWithUsersCommand>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly IJourneyShareRepository _shareRepository;
    private readonly ISharedJourneysAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ShareJourneyWithUsersCommandHandler(
        IJourneyRepository journeyRepository,
        IJourneyShareRepository shareRepository,
        ISharedJourneysAuditRepository auditRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _journeyRepository = journeyRepository;
        _shareRepository = shareRepository;
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ShareJourneyWithUsersCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var journey = await _journeyRepository.GetByIdAndUserIdAsync(request.JourneyId, userId, cancellationToken);
        if (journey is null)
            throw new NotFoundException(nameof(Journey), request.JourneyId);

        foreach (var id in request.UserIds)
        {
            var share = new JourneyShare
            {
                Id = Guid.NewGuid(),
                JourneyId = request.JourneyId,
                SharedByUserId = userId,
                SharedWithUserId = id,
                SharedAt = DateTime.UtcNow
            };

             _shareRepository.AddJourney(share, cancellationToken);

            var audit = new SharingAuditLog
            {
                Id = Guid.NewGuid(),
                JourneyId = request.JourneyId,
                UserId = userId,
                TargetUserId = id,
                Action = SharingAction.Share,
                Timestamp = DateTime.UtcNow
            };

            _auditRepository.AddAudit(audit, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return;
    }
}
