using NavigationPlatform.JourneyService.Application.Commands.DeleteJourney;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands;

public class DeleteJourneyCommandHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly DeleteJourneyCommandHandler _handler;

    public DeleteJourneyCommandHandlerTests()
    {
        _handler = new DeleteJourneyCommandHandler(_journeyRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_DeleteJourney_When_Exists_And_NotDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var command = new DeleteJourneyCommand { Id = journeyId };

        var journey = Substitute.For<Journey>();
        journey.IsDeleted = false;

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId, Arg.Any<CancellationToken>())
            .Returns(journey);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _journeyRepository.Received(1).DeleteById(journey);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_Journey_NotFound()
    {
        // Arrange
        var command = new DeleteJourneyCommand { Id = Guid.NewGuid() };
        _currentUserService.GetUserId().Returns(Guid.NewGuid());

        _journeyRepository.GetByIdAndUserIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Journey)null!);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _journeyRepository.DidNotReceive().DeleteById(Arg.Any<Journey>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_Journey_AlreadyDeleted()
    {
        // Arrange
        var journey = Substitute.For<Journey>();
        journey.IsDeleted = true;

        var command = new DeleteJourneyCommand { Id = Guid.NewGuid() };
        _currentUserService.GetUserId().Returns(Guid.NewGuid());

        _journeyRepository.GetByIdAndUserIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(journey);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _journeyRepository.DidNotReceive().DeleteById(journey);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
