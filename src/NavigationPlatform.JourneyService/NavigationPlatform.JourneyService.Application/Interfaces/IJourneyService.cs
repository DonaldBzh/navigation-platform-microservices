using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Application.Interfaces;

public interface IJourneyService
{
    Task<PaginatedResult<MonthlyUserDistance>> GetMonthlyUserDistancesAsync(
     int page, int pageSize, string orderBy, string direction, CancellationToken cancellationToken);
    Task<PaginatedResult<JourneyResponse>> GetJourneysFilteredAsync(GetFilteredJourneysRequest getFilteredJourneys, CancellationToken token = default);
}
