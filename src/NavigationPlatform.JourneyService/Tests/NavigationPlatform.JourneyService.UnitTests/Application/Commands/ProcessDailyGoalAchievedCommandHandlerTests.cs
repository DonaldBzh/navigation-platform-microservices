using FluentAssertions;
using Microsoft.Extensions.Logging;
using NavigationPlatform.JourneyService.Application.Commands.ProcessDailyGoal;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Events;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands
{
    public class ProcessDailyGoalAchievedCommandHandlerTests
    {
        private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<ProcessDailyGoalAchievedCommandHandler> _logger = Substitute.For<ILogger<ProcessDailyGoalAchievedCommandHandler>>();

        private readonly ProcessDailyGoalAchievedCommandHandler _sut;

        public ProcessDailyGoalAchievedCommandHandlerTests()
        {
            _sut = new ProcessDailyGoalAchievedCommandHandler(_journeyRepository, _unitOfWork, _logger);
        }

        [Fact]
        public async Task Handle_Should_Set_IsDailyGoalAchieved_And_Save_When_Journey_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var journeyId = Guid.NewGuid();

            var journey = new Journey { Id = journeyId, UserId = userId, IsDailyGoalAchieved = false };

            _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId)
                .Returns(Task.FromResult(journey));

            var command = new ProcessDailyGoalAchievedCommand( new DailyGoalAchieved
                {
                    UserId = userId,
                    TriggeringJourneyId = journeyId,
                    AchievedAt = DateTime.UtcNow
                });

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            journey.IsDailyGoalAchieved.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_Throw_NotFoundException_When_Journey_NotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var journeyId = Guid.NewGuid();

            _journeyRepository
                .GetByIdAndUserIdAsync(journeyId, userId)
                .Returns(Task.FromResult<Journey>(null));

            var command = new ProcessDailyGoalAchievedCommand(new DailyGoalAchieved
            {
                UserId = userId,
                TriggeringJourneyId = journeyId,
                AchievedAt = DateTime.UtcNow
            });
            

            // Act
            var act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                 .Where(ex => ex.Message.Contains("Journey with {id} is not found"));
        }
    }
}
