using AutoMapper;
using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;

namespace NavigationPlatform.JourneyService.Application.Queries.GetJourneys;

public class GetJourneysQueryHandler : IRequestHandler<GetJourneysQuery, IEnumerable<JourneyResponse>>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetJourneysQueryHandler(IJourneyRepository journeyRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _journeyRepository = journeyRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JourneyResponse>> Handle(GetJourneysQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var journeys = await _journeyRepository.GetJourneysAsync(userId);

        var journeysResponse = _mapper.Map<IEnumerable<JourneyResponse>>(journeys);
        return journeysResponse;

    }
}
