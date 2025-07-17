using MediatR;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Application.Queries.GetMonthlyDistance;

public class GetMonthlyRouteDistancesQuery : IRequest<PaginatedResult<MonthlyUserDistance>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string OrderBy { get; init; } = "TotalDistanceKm";
    public string Direction { get; init; } = "DESC";

}
