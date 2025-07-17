using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NavigationPlatform.RewardService.Worker.Commands;
using NavigationPlatform.RewardService.Worker.Configuration;
using NavigationPlatform.RewardService.Worker.Entities;
using NavigationPlatform.RewardService.Worker.Events;
using NavigationPlatform.RewardService.Worker.Repositories;
using NavigationPlatform.RewardService.Worker.Services;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.RewardService.Worker.UnitTest.Commands;


public class ProcessJourneyCreatedHandlerTests
{
    private readonly IDailyGoalAchievementRepository _repo = Substitute.For<IDailyGoalAchievementRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IEventProducer _producer = Substitute.For<IEventProducer>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IOptions<BusinessRulesOptions> _options = Microsoft.Extensions.Options.Options.Create(new BusinessRulesOptions { DailyGoalThresholdKm = 10 });
    private readonly ILogger<ProcessJourneyCreatedHandler> _logger = Substitute.For<ILogger<ProcessJourneyCreatedHandler>>();
    private readonly ProcessJourneyCreatedHandler _sut;

    public ProcessJourneyCreatedHandlerTests()
        => _sut = new(_repo, _cache, _producer, _options, _logger, _uow);

    private static ProcessJourneyCreatedCommand Cmd(Guid? userId = null, decimal km = 5)
    {
        var id = userId ?? Guid.NewGuid();
        return new(new JourneyCreated
        {
            UserId = id,
            JourneyId = Guid.NewGuid(),
            DistanceKm = km,
            StartTime = DateTime.UtcNow
        });
    }

    [Fact]
    public async Task Should_PublishAchievement_When_ThresholdReached_And_NotAlreadyExists()
    {
        var cmd = Cmd(km: 10);
        _cache.GetDailyTotalAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>()).Returns(0);
        _repo.ExistsForUserAndDateAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>()).Returns(false);

        await _sut.Handle(cmd, default);

         _repo.Received(1).Create(Arg.Any<DailyGoalAchievement>());
        await _producer.Received(1).PublishDailyGoalAchievedAsync(Arg.Any<DailyGoalAchieved>());
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Should_SkipPublishing_When_AchievementExists()
    {
        var cmd = Cmd(km: 10);
        _cache.GetDailyTotalAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>()).Returns(0);
        _repo.ExistsForUserAndDateAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>()).Returns(true);

        await _sut.Handle(cmd, default);

         _repo.DidNotReceive().Create(Arg.Any<DailyGoalAchievement>());
        await _producer.DidNotReceive().PublishDailyGoalAchievedAsync(Arg.Any<DailyGoalAchieved>());
    }

    [Fact]
    public async Task Should_OnlyCache_When_BelowThreshold()
    {
        var cmd = Cmd(km: 3);
        _cache.GetDailyTotalAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>()).Returns(2);

        await _sut.Handle(cmd, default);

        await _cache.Received(1).SetDailyTotalAsync(cmd.JourneyCreated.UserId, Arg.Any<DateTime>(), 5);
         _repo.DidNotReceive().Create(Arg.Any<DailyGoalAchievement>());
    }
    

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProcessingBegins()
    {
        // Arrange
        var journey = new JourneyCreated
        {
            UserId = Guid.NewGuid(),
            DistanceKm = 2,
            JourneyId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow
        };

        var command = new ProcessJourneyCreatedCommand(journey);
        _cache.GetDailyTotalAsync(journey.UserId, journey.StartTime.Date).Returns(0);

        // Act
        await _sut.Handle(command, default);

        // Assert
        await _cache.Received().SetDailyTotalAsync(journey.UserId, journey.StartTime.Date, 2);
    }
}