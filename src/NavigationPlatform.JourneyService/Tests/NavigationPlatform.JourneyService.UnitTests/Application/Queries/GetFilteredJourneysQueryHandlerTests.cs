using AutoMapper;
using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Pagination;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Queries
{
    public class GetFilteredJourneysQueryHandlerTests
    {
        private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IJourneyService _journeyService = Substitute.For<IJourneyService>();

        private readonly GetFilteredJourneysQueryHandler _handler;

        public GetFilteredJourneysQueryHandlerTests()
        {
            _handler = new GetFilteredJourneysQueryHandler(_currentUserService, _mapper, _journeyService);
        }

        [Fact]
        public async Task Handle_Should_Return_Filtered_Journeys()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _currentUserService.GetUserId().Returns(userId);

            var request = new GetFilteredJourneysQuery(new GetFilteredJourneysRequest
            {
                Page = 1,
                PageSize = 10,
                TransportationType = TransportationType.Car
            });

            var items = new List<JourneyResponse>
                {
                    new JourneyResponse
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        TransportationType = "Car"
                    }
                };

            var expectedResult = new PaginatedResult<JourneyResponse>(
                items,
                count: 1,
                pageNumber: 1,
                pageSize: 10
            );

            _journeyService.GetJourneysFilteredAsync(request.Filter)
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items[0].TransportationType.Should().Be("Car");
        }
    }
}
