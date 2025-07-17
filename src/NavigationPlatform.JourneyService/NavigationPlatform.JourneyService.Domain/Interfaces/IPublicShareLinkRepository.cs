using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Domain.Interfaces;

public interface IPublicJourneyRepository
{
    void AddPublicJourney(PublicJourney journey, CancellationToken token = default);
    Task<PublicJourney?> GetPublicJourney(Guid journeyId, Guid userId, CancellationToken token = default);
    Task<PublicJourney?> GetPublicJourneyByToken(string publicToken, CancellationToken token = default);
}
