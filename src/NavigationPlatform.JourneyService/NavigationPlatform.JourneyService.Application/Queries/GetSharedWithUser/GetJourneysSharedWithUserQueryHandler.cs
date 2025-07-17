using AutoMapper;
using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;

namespace NavigationPlatform.JourneyService.Application.Queries.GetSharedWithUser;

public class GetJourneysSharedWithUserQueryHandler : IRequestHandler<GetJourneysSharedWithUserQuery, List<JourneyResponse>>
{
    private readonly IMapper _mapper;
    private readonly IJourneyShareRepository _sharedjourneysRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetJourneysSharedWithUserQueryHandler(
        IJourneyShareRepository sharedjourneysRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _sharedjourneysRepository = sharedjourneysRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<JourneyResponse>> Handle(GetJourneysSharedWithUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var sharedJourneys = await _sharedjourneysRepository.GetSharedJourneysWithUser(userId, cancellationToken);
        return _mapper.Map<List<JourneyResponse>>(sharedJourneys) ?? new List<JourneyResponse>(); ;
    }
}