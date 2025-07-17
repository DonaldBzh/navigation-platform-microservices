using AutoMapper;
using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Pagination;

namespace NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;

public class GetFilteredJourneysQueryHandler : IRequestHandler<GetFilteredJourneysQuery, PaginatedResult<JourneyResponse>>
{
    private readonly IJourneyService _journeyService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetFilteredJourneysQueryHandler(ICurrentUserService currentUserService, IMapper mapper, IJourneyService journeyService)
    {
        _currentUserService = currentUserService;
        _mapper = mapper;
        _journeyService = journeyService;
    }

    public async Task<PaginatedResult<JourneyResponse>> Handle(GetFilteredJourneysQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var journeysResponse = await _journeyService.GetJourneysFilteredAsync(request.Filter);
        return journeysResponse;
    }
}

