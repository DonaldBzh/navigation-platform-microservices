using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Commands.RevokePublic;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands;

public class RevokePublicJourneyCommandHandlerTests
{
    private readonly IPublicJourneyRepository _publicJourneyRepository = Substitute.For<IPublicJourneyRepository>();
    private readonly ISharedJourneysAuditRepository _auditRepository = Substitute.For<ISharedJourneysAuditRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly RevokePublicJourneyCommandHandler _handler;

    public RevokePublicJourneyCommandHandlerTests()
    {
        _handler = new RevokePublicJourneyCommandHandler(
            _publicJourneyRepository,
            _auditRepository,
            _unitOfWork,
            _currentUserService
        );
    }

    [Fact]
    public async Task Handle_Should_Revoke_Journey_And_Save_When_Found()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RevokePublicJourneyCommand(journeyId);

        var publicJourney = new PublicJourney
        {
            JourneyId = journeyId,
            CreatedByUserId = userId,
            IsRevoked = false
        };

        _currentUserService.GetUserId().Returns(userId);
        _publicJourneyRepository.GetPublicJourney(journeyId, userId, Arg.Any<CancellationToken>())
            .Returns(publicJourney);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        publicJourney.IsRevoked.Should().BeTrue();
        publicJourney.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _auditRepository.Received(1).AddAudit(Arg.Is<SharingAuditLog>(a =>
            a.JourneyId == journeyId &&
            a.UserId == userId &&
            a.Action == SharingAction.RevokePublicLink
        ));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Journey_NotFound()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RevokePublicJourneyCommand(journeyId);

        _currentUserService.GetUserId().Returns(userId);
        _publicJourneyRepository.GetPublicJourney(journeyId, userId, Arg.Any<CancellationToken>())
            .Returns((PublicJourney?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("No active public link found.");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _auditRepository.DidNotReceive().AddAudit(Arg.Any<SharingAuditLog>());
    }
}


