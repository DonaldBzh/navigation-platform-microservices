using MediatR;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.RevokePublic;

public class RevokePublicJourneyCommandHandler : IRequestHandler<RevokePublicJourneyCommand>
{
    private readonly IPublicJourneyRepository _publicShareLinkRepository;
    private readonly ISharedJourneysAuditRepository _shareAuditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RevokePublicJourneyCommandHandler(
        IPublicJourneyRepository publicShareLinkRepository,
        ISharedJourneysAuditRepository shareAuditRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
       
        _publicShareLinkRepository = publicShareLinkRepository;
        _shareAuditRepository = shareAuditRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RevokePublicJourneyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var journey = await _publicShareLinkRepository.GetPublicJourney(request.JourneyId, userId, cancellationToken);
        if (journey == null)
        {
            throw new NotFoundException("No active public link found.");
        }

        journey.IsRevoked = true;
        journey.RevokedAt = DateTime.UtcNow;

        _shareAuditRepository.AddAudit(new SharingAuditLog
        {
            Id = Guid.NewGuid(),
            JourneyId = request.JourneyId,
            UserId = userId,
            Action = SharingAction.RevokePublicLink,
            Timestamp = DateTime.UtcNow,
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return;
    }


}

