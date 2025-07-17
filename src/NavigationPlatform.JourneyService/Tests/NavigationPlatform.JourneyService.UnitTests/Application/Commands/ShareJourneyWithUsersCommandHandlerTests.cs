using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Commands.ShareWithUsers;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands;

public class ShareJourneyWithUsersCommandHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly IJourneyShareRepository _shareRepository = Substitute.For<IJourneyShareRepository>();
    private readonly ISharedJourneysAuditRepository _auditRepository = Substitute.For<ISharedJourneysAuditRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly ShareJourneyWithUsersCommandHandler _handler;

    public ShareJourneyWithUsersCommandHandlerTests()
    {
        _handler = new ShareJourneyWithUsersCommandHandler(
            _journeyRepository,
            _shareRepository,
            _auditRepository,
            _unitOfWork,
            _currentUserService);
    }

    [Fact]
    public async Task Handle_Should_Share_With_All_Users_And_Audit()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var sharedUserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var command = new ShareJourneyWithUsersCommand(journeyId, sharedUserIds);
        var journey = new Journey(); 

        _currentUserService.GetUserId().Returns(ownerId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, ownerId, Arg.Any<CancellationToken>())
            .Returns(journey);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        foreach (var userId in sharedUserIds)
        {
             _shareRepository.Received(1).AddJourney(
                Arg.Is<JourneyShare>(s =>
                    s.JourneyId == journeyId &&
                    s.SharedByUserId == ownerId &&
                    s.SharedWithUserId == userId),
                Arg.Any<CancellationToken>());

            _auditRepository.Received(1).AddAudit(
                Arg.Is<SharingAuditLog>(a =>
                    a.JourneyId == journeyId &&
                    a.UserId == ownerId &&
                    a.Action == SharingAction.Share &&
                    a.TargetUserId == userId), 
                Arg.Any<CancellationToken>());
        }

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Journey_Not_Found()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var command = new ShareJourneyWithUsersCommand(journeyId, new List<Guid> { Guid.NewGuid() });

        _currentUserService.GetUserId().Returns(ownerId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, ownerId, Arg.Any<CancellationToken>())
            .Returns((Journey?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Journey*");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
         _shareRepository.DidNotReceiveWithAnyArgs().AddJourney(default!, default);
         _auditRepository.DidNotReceiveWithAnyArgs().AddAudit(default!, default);
    }
}
