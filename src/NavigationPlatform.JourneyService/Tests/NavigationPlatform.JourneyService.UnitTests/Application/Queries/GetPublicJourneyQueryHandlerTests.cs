using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Queries;

public class GetPublicJourneyQueryHandlerTests
{
    private readonly IPublicJourneyRepository _publicJourneyRepository = Substitute.For<IPublicJourneyRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetPublicJourneyQueryHandler _handler;

    public GetPublicJourneyQueryHandlerTests()
    {
        _handler = new GetPublicJourneyQueryHandler(_publicJourneyRepository, _currentUserService);
    }

    [Fact]
    public async Task Handle_Should_Return_PublicJourneyResponse_When_Valid()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid();

        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Journey",
            TransportationType = TransportationType.Walking,
            StartDate = DateTime.UtcNow,
            ArrivalDate = DateTime.UtcNow.AddHours(2),
            RouteDistanceKm = 12.5m
        };

        var publicJourney = new PublicJourney
        {
            Journey = journey,
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _currentUserService.GetUserId().Returns(userId);
        _publicJourneyRepository.GetPublicJourneyByToken(token, Arg.Any<CancellationToken>())
            .Returns(publicJourney);

        var query = new GetPublicJourneyQuery(token);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(journey.Id);
        result.Title.Should().Be(journey.Name);
        result.TransportationType.Should().Be(journey.TransportationType.ToString());
        result.DistanceKm.Should().Be(journey.RouteDistanceKm);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Journey_Is_Null()
    {
        // Arrange
        var token = "missing-token";
 
        _publicJourneyRepository.GetPublicJourneyByToken(token, Arg.Any<CancellationToken>())
            .Returns((PublicJourney?)null);

        var query = new GetPublicJourneyQuery(token);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Public link not found.");
    }

    [Fact]
    public async Task Handle_Should_Throw_GoneException_When_Journey_Is_Revoked()
    {
        // Arrange
        var token = "revoked-token";
        var userId = Guid.NewGuid();

        var publicJourney = new PublicJourney
        {
            IsRevoked = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Journey = new Journey { Id = Guid.NewGuid() }
        };

        _currentUserService.GetUserId().Returns(userId);
        _publicJourneyRepository.GetPublicJourneyByToken(token, Arg.Any<CancellationToken>())
            .Returns(publicJourney);

        var query = new GetPublicJourneyQuery(token);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<GoneException>()
            .WithMessage("This link has been revoked or expired.");
    }

    [Fact]
    public async Task Handle_Should_Throw_GoneException_When_Journey_Is_Expired()
    {
        // Arrange
        var token = "expired-token";
        var userId = Guid.NewGuid();

        var publicJourney = new PublicJourney
        {
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Journey = new Journey { Id = Guid.NewGuid() }
        };

        _currentUserService.GetUserId().Returns(userId);
        _publicJourneyRepository.GetPublicJourneyByToken(token, Arg.Any<CancellationToken>())
            .Returns(publicJourney);

        var query = new GetPublicJourneyQuery(token);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<GoneException>()
            .WithMessage("This link has been revoked or expired.");
    }
}
