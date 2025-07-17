using AutoMapper;
using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneys;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Queries;

public class GetJourneysQueryHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetJourneysQueryHandler _handler;

    public GetJourneysQueryHandlerTests()
    {
        _handler = new GetJourneysQueryHandler(_journeyRepository, _currentUserService, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_JourneyResponses_For_CurrentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var journeys = new List<Journey>
        {
            new Journey { Id = Guid.NewGuid(), UserId = userId },
            new Journey { Id = Guid.NewGuid(), UserId = userId }
        };

        var expectedResponses = journeys.Select(j => new JourneyResponse { Id = j.Id, UserId = j.UserId }).ToList();

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetJourneysAsync(userId).Returns(journeys);
        _mapper.Map<IEnumerable<JourneyResponse>>(journeys).Returns(expectedResponses);

        var query = new GetJourneysQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().BeEquivalentTo(journeys.Select(j => j.Id));
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_No_Journeys()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyList = new List<Journey>();

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetJourneysAsync(userId).Returns(emptyList);
        _mapper.Map<IEnumerable<JourneyResponse>>(emptyList).Returns(new List<JourneyResponse>());

        var query = new GetJourneysQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
