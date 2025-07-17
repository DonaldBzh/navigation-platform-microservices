using AutoMapper;
using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Application.Queries.GetSharedWithUser;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands;

public class GetJourneysSharedWithUserQueryHandlerTests
{
    private readonly IJourneyShareRepository _journeyShareRepository = Substitute.For<IJourneyShareRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private readonly GetJourneysSharedWithUserQueryHandler _handler;

    public GetJourneysSharedWithUserQueryHandlerTests()
    {
        _handler = new GetJourneysSharedWithUserQueryHandler(
            _journeyShareRepository,
            _currentUserService,
            _mapper);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoJourneysShared()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.GetUserId().Returns(userId);

        var journeys = new List<Journey>(); // Empty
        _journeyShareRepository.GetSharedJourneysWithUser(userId, Arg.Any<CancellationToken>())
            .Returns(journeys);

        _mapper.Map<List<JourneyResponse>>(journeys).Returns(new List<JourneyResponse>());

        var request = new GetJourneysSharedWithUserQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_HandleNullRepositoryResult()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserService.GetUserId().Returns(userId);

        List<Journey>? journeys = null;
        _journeyShareRepository.GetSharedJourneysWithUser(userId, Arg.Any<CancellationToken>())
            .Returns(journeys);

        _mapper.Map<List<JourneyResponse>>(null).Returns(new List<JourneyResponse>());

        var request = new GetJourneysSharedWithUserQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_ToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserService.GetUserId().Returns(userId);

        var journeys = new List<Journey>();
        var cts = new CancellationTokenSource();

        _journeyShareRepository.GetSharedJourneysWithUser(userId, cts.Token)
            .Returns(journeys);

        _mapper.Map<List<JourneyResponse>>(journeys).Returns(new List<JourneyResponse>());

        var request = new GetJourneysSharedWithUserQuery();

        // Act
        var result = await _handler.Handle(request, cts.Token);

        // Assert
        result.Should().BeEmpty();

        await _journeyShareRepository.Received(1).GetSharedJourneysWithUser(userId, cts.Token);
    }
}
