using MediatR;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;

namespace NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;

public class GetPublicJourneyQueryHandler : IRequestHandler<GetPublicJourneyQuery, PublicJourneyResponse>
{
    private readonly IPublicJourneyRepository _publicJourneyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPublicJourneyQueryHandler(IPublicJourneyRepository publicJourneyRepository, ICurrentUserService currentUserService)
    {
        _publicJourneyRepository = publicJourneyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PublicJourneyResponse> Handle(GetPublicJourneyQuery request, CancellationToken cancellationToken)
    {
        var journey = await _publicJourneyRepository.GetPublicJourneyByToken(request.Token, cancellationToken);
        if (journey == null)
            throw new NotFoundException("Public link not found.");

        if (journey.IsRevoked || (journey.ExpiresAt.HasValue && journey.ExpiresAt.Value < DateTime.UtcNow))
            throw new GoneException("This link has been revoked or expired.");

        return new PublicJourneyResponse
        {
            Id = journey.Journey.Id,
            UserId = journey.Journey.UserId,
            Title = journey.Journey.Name,
            TransportationType = journey.Journey.TransportationType.ToString(),
            StartDate = journey.Journey.StartDate,
            ArrivalDate = journey.Journey.ArrivalDate,
            DistanceKm = journey.Journey.RouteDistanceKm
        };
    }
}
