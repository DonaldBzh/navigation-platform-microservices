using AutoMapper;
using MediatR;
using NavigationPlatform.JourneyService.Application.Events;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.CreateJourney;

public class CreateJourneyCommandHandler : IRequestHandler<CreateJourneyCommand, JourneyResponse>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateJourneyCommandHandler(IJourneyRepository journeyRepository, ICurrentUserService currentUserService, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator)
    {

        _journeyRepository = journeyRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<JourneyResponse> Handle(
        CreateJourneyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
       
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Data.Name,
            StartLocation = request.Data.StartLocation,
            StartDate = request.Data.StartDateTime,
            ArrivalLocation = request.Data.ArrivalLocation,
            ArrivalDate = request.Data.ArrivalDateTime,
            TransportationType = request.Data.TransportationType,
            RouteDistanceKm = request.Data.RouteDistanceKm,
            IsDailyGoalAchieved = false,
            CreatedAt = DateTime.UtcNow,
        };

         _journeyRepository.AddJourney(journey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        await _mediator.Publish(new JourneyCreatedEventNotification
        {
            CreatedAt = journey.CreatedAt,
            UserId = journey.UserId,
            DistanceKm = journey.RouteDistanceKm,
            EndTime= journey.ArrivalDate,
            JourneyId = journey.Id,
            StartTime = journey.StartDate
        });


        return _mapper.Map<JourneyResponse>(journey);
    }
}
