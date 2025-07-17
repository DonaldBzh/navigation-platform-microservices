using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Commands.SharePublic;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands;

public class CreatePublicShareLinkCommandHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly IPublicJourneyRepository _publicJourneyRepository = Substitute.For<IPublicJourneyRepository>();
    private readonly ISharedJourneysAuditRepository _auditRepository = Substitute.For<ISharedJourneysAuditRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly CreatePublicShareLinkCommandHandler _handler;

    public CreatePublicShareLinkCommandHandlerTests()
    {
        _handler = new CreatePublicShareLinkCommandHandler(
            _journeyRepository,
            _publicJourneyRepository,
            _auditRepository,
            _unitOfWork,
            _currentUserService);
    }

    [Fact]
    public async Task Handle_Should_Create_Public_Link_And_Save()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var journey = new Journey(); // Substitute or real object depending on your design

        var command = new CreatePublicShareLinkCommand(journeyId, expiresAt);

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId, Arg.Any<CancellationToken>())
            .Returns(journey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().StartWith("https://navigationplatform.com/journeys/public/");

         _publicJourneyRepository.Received(1).AddPublicJourney(Arg.Is<PublicJourney>(pj =>
            pj.JourneyId == journeyId &&
            pj.CreatedByUserId == userId &&
            pj.ExpiresAt == expiresAt &&
            !pj.IsRevoked));

        _auditRepository.Received(1).AddAudit(Arg.Is<SharingAuditLog>(log =>
            log.JourneyId == journeyId &&
            log.UserId == userId &&
            log.Action == SharingAction.CreatePublicLink));

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Journey_NotFound()
    {
        // Arrange
        var journeyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var command = new CreatePublicShareLinkCommand(journeyId, null);

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId, Arg.Any<CancellationToken>())
            .Returns((Journey?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Journey*");

         _publicJourneyRepository.DidNotReceive().AddPublicJourney(Arg.Any<PublicJourney>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
