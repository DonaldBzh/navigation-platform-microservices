using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Domain.Interfaces;

public interface IJourneyShareRepository
{
    void AddJourney(JourneyShare journey, CancellationToken token = default);
    Task<IEnumerable<Journey>> GetSharedJourneysWithUser(Guid userId,CancellationToken token = default);

}
