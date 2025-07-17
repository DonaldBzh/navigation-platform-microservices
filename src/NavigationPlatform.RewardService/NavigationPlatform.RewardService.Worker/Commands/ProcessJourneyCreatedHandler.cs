using MediatR;
using Microsoft.Extensions.Options;
using NavigationPlatform.RewardService.Worker.Configuration;
using NavigationPlatform.RewardService.Worker.Entities;
using NavigationPlatform.RewardService.Worker.Events;
using NavigationPlatform.RewardService.Worker.Repositories;
using NavigationPlatform.RewardService.Worker.Services;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.RewardService.Worker.Commands;

public class ProcessJourneyCreatedHandler : IRequestHandler<ProcessJourneyCreatedCommand>
{
    private readonly IDailyGoalAchievementRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IEventProducer _eventProducer;
    private readonly BusinessRulesOptions _businessRules;
    private readonly ILogger<ProcessJourneyCreatedHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessJourneyCreatedHandler(
        IDailyGoalAchievementRepository repository,
        ICacheService cacheService,
        IEventProducer eventProducer,
        IOptions<BusinessRulesOptions> businessRules,
        ILogger<ProcessJourneyCreatedHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _cacheService = cacheService;
        _eventProducer = eventProducer;
        _businessRules = businessRules.Value;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessJourneyCreatedCommand request, CancellationToken cancellationToken)
    {
        var journey = request.JourneyCreated;
        var journeyDate = journey.StartTime.Date;
        var thresholdKm = _businessRules.DailyGoalThresholdKm;

        _logger.LogInformation("Processing journey {JourneyId} for user {UserId}, distance: {Distance}km",
            journey.JourneyId, journey.UserId, journey.DistanceKm);

        //Get current daily total from cache
        var currentTotal = await _cacheService.GetDailyTotalAsync(journey.UserId, journeyDate);

        var newTotal = currentTotal + journey.DistanceKm;

        //Update cache
        await _cacheService.SetDailyTotalAsync(journey.UserId, journeyDate, newTotal);

        _logger.LogDebug("Updated daily total for user {UserId} on {Date}: {OldTotal} + {JourneyDistance} = {NewTotal}",
            journey.UserId, journeyDate, currentTotal, journey.DistanceKm, newTotal);

        await CheckAndCreateAchievement(journey, journeyDate, newTotal, thresholdKm);
    }

    private async Task CheckAndCreateAchievement(JourneyCreated journey, DateTime journeyDate, decimal newTotal, decimal thresholdKm)
    {
        if (newTotal >= thresholdKm)
        {
            // Check if achievement already exists for this day
            var existingAchievement = await _repository.ExistsForUserAndDateAsync(journey.UserId, journeyDate);

            if (!existingAchievement)
            {
                _logger.LogInformation("User {UserId} achieved daily goal! Total: {Total}km on {Date}, triggered by journey {JourneyId}",
                    journey.UserId, newTotal, journeyDate, journey.JourneyId);

                var achievement = new DailyGoalAchievement
                {
                    UserId = journey.UserId,
                    AchievementDate = journeyDate,
                    TotalDistanceKm = newTotal,
                    TriggeringJourneyId = journey.JourneyId,
                    CreatedAt = DateTime.UtcNow
                };

                _repository.Create(achievement);

                await _unitOfWork.SaveChangesAsync();
                // Publish achievement event 
                var achievementEvent = new DailyGoalAchieved
                {
                    UserId = journey.UserId,
                    AchievementDate = journeyDate,
                    TotalDistanceKm = newTotal,
                    TriggeringJourneyId = journey.JourneyId,
                    AchievedAt = DateTime.UtcNow
                };

                await _eventProducer.PublishDailyGoalAchievedAsync(achievementEvent);

                _logger.LogInformation("Published DailyGoalAchieved event for user {UserId}. Journey Service will handle updating journey {JourneyId}",
                    journey.UserId, journey.JourneyId);
            }
            else
            {
                _logger.LogDebug("User {UserId} already has achievement for {Date}, skipping", journey.UserId, journeyDate);
            }
        }
        else
        {
            _logger.LogDebug("User {UserId} daily total {Total}km is below threshold {Threshold}km",
                journey.UserId, newTotal, thresholdKm);
        }
    }
}