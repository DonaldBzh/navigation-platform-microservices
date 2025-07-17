using MediatR;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.SharePublic;

public class CreatePublicShareLinkCommandHandler : IRequestHandler<CreatePublicShareLinkCommand, string>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly IPublicJourneyRepository _publicShareLinkRepository;
    private readonly ISharedJourneysAuditRepository _shareAuditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePublicShareLinkCommandHandler(
        IJourneyRepository journeyRepository,
        IPublicJourneyRepository publicShareLinkRepository,
        ISharedJourneysAuditRepository shareAuditRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _journeyRepository = journeyRepository;
        _publicShareLinkRepository = publicShareLinkRepository;
        _shareAuditRepository = shareAuditRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(CreatePublicShareLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var journey = await _journeyRepository.GetByIdAndUserIdAsync(request.JourneyId, userId, cancellationToken);
        if (journey == null)
            throw new NotFoundException("Journey", request.JourneyId);

        var uniqueToken = Guid.NewGuid().ToString("N");

        var publicLink = new PublicJourney
        {
            Id = Guid.NewGuid(),
            JourneyId = request.JourneyId,
            PublicToken = uniqueToken,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsRevoked = false,
            RevokedAt = null,
            AccessCount = 0
        };

         _publicShareLinkRepository.AddPublicJourney(publicLink);

         _shareAuditRepository.AddAudit(new SharingAuditLog
        {
            Id = Guid.NewGuid(),
            JourneyId = request.JourneyId,
            UserId = userId,
            Action = SharingAction.CreatePublicLink,
            Timestamp = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return GeneratePublicUrl(uniqueToken);
    }

    private string GeneratePublicUrl(string token) => $"https://navigationplatform.com/journeys/public/{token}";
}
