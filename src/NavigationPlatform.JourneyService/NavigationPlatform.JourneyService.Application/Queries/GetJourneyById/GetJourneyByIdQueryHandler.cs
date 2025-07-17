using AutoMapper;
using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;

public class GetJourneyByIdQueryHandler : IRequestHandler<GetJourneyByIdQuery, JourneyResponse?>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetJourneyByIdQueryHandler(IJourneyRepository journeyRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _journeyRepository = journeyRepository;
        _currentUserService = currentUserService;
        _mapper = mapper; 
    }

    public async Task<JourneyResponse?> Handle(GetJourneyByIdQuery request, CancellationToken cancellationToken)
    {
        
        var userId = _currentUserService.GetUserId();
     
        var journey = await _journeyRepository.GetByIdAndUserIdAsync(request.Id, userId);

        if (journey is null)
          throw new NotFoundException("Journey not found");

        var journeyResponse = _mapper.Map<JourneyResponse>(journey);
        return journeyResponse;
        
    }
}
