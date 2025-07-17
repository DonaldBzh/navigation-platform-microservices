using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetMonthlyDistance;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.Shared.Pagination;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Queries;

public class GetMonthlyRouteDistancesQueryHandlerTests
{
    private readonly IJourneyService _journeyService = Substitute.For<IJourneyService>();
    private readonly GetMonthlyRouteDistancesQueryHandler _handler;

    public GetMonthlyRouteDistancesQueryHandlerTests()
    {
        _handler = new GetMonthlyRouteDistancesQueryHandler(_journeyService);
    }

    [Fact]
    public async Task Handle_Should_Return_PaginatedResult_With_Data()
    {
        // Arrange
        var query = new GetMonthlyRouteDistancesQuery
        {
            Page = 1,
            PageSize = 10,
            OrderBy = "TotalDistanceKm",
            Direction = "DESC"
        };

        var data = new List<MonthlyUserDistance>
        {
            new MonthlyUserDistance { UserId = Guid.NewGuid(), TotalDistanceKm = 150, Month = new DateTime(2025, 6, 1).Month },
            new MonthlyUserDistance { UserId = Guid.NewGuid(), TotalDistanceKm = 120, Month = new DateTime(2025, 6, 1).Month }
        };

        var paginatedResult = new PaginatedResult<MonthlyUserDistance>(data, data.Count, query.Page, query.PageSize);

        _journeyService.GetMonthlyUserDistancesAsync(
            query.Page,
            query.PageSize,
            query.OrderBy,
            query.Direction,
            Arg.Any<CancellationToken>())
            .Returns(paginatedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyResult_When_NoData()
    {
        // Arrange
        var query = new GetMonthlyRouteDistancesQuery
        {
            Page = 1,
            PageSize = 10
        };

        var emptyResult = new PaginatedResult<MonthlyUserDistance>(
            new List<MonthlyUserDistance>(), 0, query.Page, query.PageSize);

        _journeyService.GetMonthlyUserDistancesAsync(
            query.Page,
            query.PageSize,
            query.OrderBy,
            query.Direction,
            Arg.Any<CancellationToken>())
            .Returns(emptyResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
