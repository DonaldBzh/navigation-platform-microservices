using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Application.Queries.GetMonthlyDistance;

public class GetMonthlyRouteDistancesQueryHandler
    : IRequestHandler<GetMonthlyRouteDistancesQuery, PaginatedResult<MonthlyUserDistance>>
{
    private readonly IJourneyService _journeyService;

    public GetMonthlyRouteDistancesQueryHandler(IJourneyService journeyService)
    {
        _journeyService = journeyService;
    }

    public async Task<PaginatedResult<MonthlyUserDistance>> Handle(
        GetMonthlyRouteDistancesQuery request,
        CancellationToken cancellationToken)
    {
        var journeys = await _journeyService.GetMonthlyUserDistancesAsync(
            page: request.Page,
            pageSize: request.PageSize,
            orderBy: request.OrderBy,
            direction: request.Direction,
            cancellationToken: cancellationToken);


        return journeys;

    }
}