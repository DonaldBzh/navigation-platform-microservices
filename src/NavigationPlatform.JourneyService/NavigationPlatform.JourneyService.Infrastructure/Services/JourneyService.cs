using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Infrastructure.Services;

public class JourneyResultService : IJourneyService
{
    private readonly IMapper _mapper;
    private readonly IJourneyRepository _journeyRepository;

    public JourneyResultService(IMapper mapper, IJourneyRepository journeyRepository)
    {
        _mapper = mapper;
        _journeyRepository = journeyRepository;
    }


    public async Task<PaginatedResult<JourneyResponse>> GetJourneysFilteredAsync(GetFilteredJourneysRequest filter, CancellationToken token = default)
    {
        var query = _journeyRepository.GetJourneyAsQuerable().Where(j => j.IsDeleted == false);

        query = ApplyFilters(query, filter);
        var totalCount = await query.CountAsync();

        query = ApplyOrdering(query, filter.OrderBy,filter.Direction);

        var items = await ApplyPagination(query, filter.Page, filter.PageSize, token);

        var journeysResponse = _mapper.Map<IEnumerable<JourneyResponse>>(items).ToList();
        return new PaginatedResult<JourneyResponse>(journeysResponse, totalCount, filter.Page, filter.PageSize);

    }
    private  IQueryable<Journey> ApplyOrdering(IQueryable<Journey> query, string? orderBy, string? direction)
    {
        var isAsc = string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);

        return (orderBy?.ToLowerInvariant()) switch
        {
            "userid" => isAsc ? query.OrderBy(j => j.UserId) : query.OrderByDescending(j => j.UserId),
            "transportationtype" => isAsc ? query.OrderBy(j => j.TransportationType) : query.OrderByDescending(j => j.TransportationType),
            "startdate" => isAsc ? query.OrderBy(j => j.StartDate) : query.OrderByDescending(j => j.StartDate),
            "arrivaldate" => isAsc ? query.OrderBy(j => j.ArrivalDate) : query.OrderByDescending(j => j.ArrivalDate),
            _ => query.OrderByDescending(j => j.StartDate) 
        };
    }

    private async Task<List<Journey>> ApplyPagination(
            IQueryable<Journey> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<MonthlyUserDistance>> GetMonthlyUserDistancesAsync(int page, int pageSize, string orderBy, string direction, CancellationToken cancellationToken)
    {
        var groupedQuery = BuildGroupedQuery();
        var orderedQuery = ApplyOrdering(groupedQuery, orderBy, direction);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var items = await ApplyPagination(orderedQuery, page, pageSize, cancellationToken);

        return new PaginatedResult<MonthlyUserDistance>(items, totalCount, page, pageSize);
    }

    private  IQueryable<MonthlyUserDistance> BuildGroupedQuery()
    => _journeyRepository.GetMonthlyUserDistances();

    private IQueryable<MonthlyUserDistance> ApplyOrdering(
    IQueryable<MonthlyUserDistance> query,
    string orderBy,
    string direction)
    {
        var normalizedOrderBy = orderBy?.ToLower() ?? string.Empty;
        var normalizedDirection = direction?.ToLower() ?? string.Empty;

        return (normalizedOrderBy, normalizedDirection) switch
        {
            ("userid", "asc") => query.OrderBy(x => x.UserId),
            ("userid", "desc") => query.OrderByDescending(x => x.UserId),
            ("totaldistancekm", "asc") => query.OrderBy(x => x.TotalDistanceKm),
            _ => query.OrderByDescending(x => x.TotalDistanceKm),
        };
    }

    private async Task<List<MonthlyUserDistance>> ApplyPagination(
    IQueryable<MonthlyUserDistance> query,
    int page,
    int pageSize,
    CancellationToken cancellationToken)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }


    private IQueryable<Journey> ApplyFilters(IQueryable<Journey> query, GetFilteredJourneysRequest filters)
    {
        if (filters.UserId.HasValue)
            query = query.Where(j => j.UserId == filters.UserId.Value);

        if (filters.TransportationType.HasValue)
            query = query.Where(j => j.TransportationType == filters.TransportationType.Value);

        if (filters.StartDateFrom.HasValue)
            query = query.Where(j => j.StartDate >= filters.StartDateFrom.Value);

        if (filters.StartDateTo.HasValue)
            query = query.Where(j => j.StartDate <= filters.StartDateTo.Value);

        if (filters.ArrivalDateFrom.HasValue)
            query = query.Where(j => j.ArrivalDate >= filters.ArrivalDateFrom.Value);

        if (filters.ArrivalDateTo.HasValue)
            query = query.Where(j => j.ArrivalDate <= filters.ArrivalDateTo.Value);

        return query;
    }

}