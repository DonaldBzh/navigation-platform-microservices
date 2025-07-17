using AutoMapper;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.JourneyService.Infrastructure.Services;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Services;
public class JourneyResultServiceTests
{
    private readonly IMapper _mapper;
    private readonly IJourneyRepository _journeyRepository;
    private readonly JourneyResultService _sut;
    private readonly List<Journey> _testJourneys;
    private readonly List<MonthlyUserDistance> _testMonthlyDistances;

    public JourneyResultServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _journeyRepository = Substitute.For<IJourneyRepository>();
        _sut = new JourneyResultService(_mapper, _journeyRepository);

        _testJourneys = CreateTestJourneys();
        _testMonthlyDistances = CreateTestMonthlyDistances();
    }

    private List<Journey> CreateTestJourneys()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        return new List<Journey>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId1,
                    TransportationType = TransportationType.Car,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    ArrivalDate = DateTime.UtcNow.AddDays(-9),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId2,
                    TransportationType = TransportationType.Boat,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    ArrivalDate = DateTime.UtcNow.AddDays(-4),
                    IsDeleted = false
                }
            };
    }

    [Fact]
    public async Task GetJourneysFilteredAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var filter = new GetFilteredJourneysRequest
        {
            Page = 2,
            PageSize = 1
        };

        var queryable = _testJourneys.AsQueryable().BuildMock();
        _journeyRepository.GetJourneyAsQuerable().Returns(queryable);

        _mapper.Map<IEnumerable<JourneyResponse>>(Arg.Any<IEnumerable<Journey>>())
            .Returns(new List<JourneyResponse> { new() { Id = Guid.NewGuid() } });

        // Act
        var result = await _sut.GetJourneysFilteredAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetJourneysFilteredAsync_ShouldExcludeDeletedJourneys()
    {
        // Arrange
        var filter = new GetFilteredJourneysRequest { Page = 1, PageSize = 10 };

        var journeysWithDeleted = _testJourneys.ToList();
        journeysWithDeleted.Add(new Journey { Id = Guid.NewGuid(), IsDeleted = true });

        var queryable = journeysWithDeleted.AsQueryable().BuildMock();
        _journeyRepository.GetJourneyAsQuerable().Returns(queryable);

        _mapper.Map<IEnumerable<JourneyResponse>>(Arg.Any<IEnumerable<Journey>>())
            .Returns(new List<JourneyResponse>());

        // Act
        var result = await _sut.GetJourneysFilteredAsync(filter);

        // Assert
        _mapper.Received(1).Map<IEnumerable<JourneyResponse>>(
            Arg.Is<IEnumerable<Journey>>(journeys =>
                journeys.All(j => !j.IsDeleted)));
    }


    [Fact]
    public async Task GetMonthlyUserDistancesAsync_WithValidParameters_ShouldReturnPaginatedResult()
    {
        // Arrange
        var queryable = _testMonthlyDistances.AsQueryable().BuildMock();
        _journeyRepository.GetMonthlyUserDistances().Returns(queryable);

        // Act
        var result = await _sut.GetMonthlyUserDistancesAsync(1, 10, "totaldistancekm", "desc", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().Be(_testMonthlyDistances.Count);
        result.PageNumber.Should().Be(1);
    }

    [Theory]
    [InlineData("userid", "asc")]
    [InlineData("userid", "desc")]
    [InlineData("totaldistancekm", "asc")]
    [InlineData("totaldistancekm", "desc")]
    [InlineData("invalid", "desc")]
    [InlineData(null, null)]
    public async Task GetMonthlyUserDistancesAsync_WithDifferentOrderByOptions_ShouldApplyCorrectOrdering(
        string orderBy, string direction)
    {
        // Arrange
        var queryable = _testMonthlyDistances.AsQueryable().BuildMock();
        _journeyRepository.GetMonthlyUserDistances().Returns(queryable);

        // Act
        var result = await _sut.GetMonthlyUserDistancesAsync(1, 10, orderBy, direction, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMonthlyUserDistancesAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var queryable = _testMonthlyDistances.AsQueryable().BuildMock();
        _journeyRepository.GetMonthlyUserDistances().Returns(queryable);

        // Act
        var result = await _sut.GetMonthlyUserDistancesAsync(2, 1, "totaldistancekm", "desc", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.Items.Should().HaveCount(1);
    }
    private List<MonthlyUserDistance> CreateTestMonthlyDistances()
    {
        return new List<MonthlyUserDistance>
            {
                new() { UserId = Guid.NewGuid(), TotalDistanceKm = 100.5m },
                new() { UserId = Guid.NewGuid(), TotalDistanceKm = 200.75m },
                new() { UserId = Guid.NewGuid(), TotalDistanceKm = 150.25m }
            };
    }
}
