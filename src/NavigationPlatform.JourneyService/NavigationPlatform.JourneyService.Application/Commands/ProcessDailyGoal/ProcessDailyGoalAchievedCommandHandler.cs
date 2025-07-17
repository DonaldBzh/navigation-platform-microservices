using MediatR;
using Microsoft.Extensions.Logging;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.ProcessDailyGoal;

public class ProcessDailyGoalAchievedCommandHandler : IRequestHandler<ProcessDailyGoalAchievedCommand>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDailyGoalAchievedCommandHandler> _logger;

    public ProcessDailyGoalAchievedCommandHandler(IJourneyRepository journeyRepository, IUnitOfWork unitOfWork, ILogger<ProcessDailyGoalAchievedCommandHandler> logger)
    {
        _journeyRepository = journeyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProcessDailyGoalAchievedCommand request, CancellationToken cancellationToken)
    {
        var journey = await _journeyRepository.GetByIdAndUserIdAsync( request.DailyGoal.TriggeringJourneyId, request.DailyGoal.UserId);
        if (journey is null)
            throw new NotFoundException("Journey with {id} is not found", request.DailyGoal.TriggeringJourneyId);

        journey.IsDailyGoalAchieved = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return;
    }


}
