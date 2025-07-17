using FluentAssertions;
using NavigationPlatform.JourneyService.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Events
{
    public class JourneyCreatedEventTests
    {
        [Fact]
        public void Should_Create_JourneyCreatedEvent_With_Expected_Properties()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddHours(2);
            var distanceKm = 42.5m;
            var transportationType = "Bike";
            var createdAt = DateTime.UtcNow;

            // Act
            var evt = new JourneyCreatedEvent
            {
                JourneyId = journeyId,
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                DistanceKm = distanceKm,
                TransportationType = transportationType,
                CreatedAt = createdAt
            };

            // Assert
            evt.JourneyId.Should().Be(journeyId);
            evt.UserId.Should().Be(userId);
            evt.StartTime.Should().Be(startTime);
            evt.EndTime.Should().Be(endTime);
            evt.DistanceKm.Should().Be(distanceKm);
            evt.TransportationType.Should().Be(transportationType);
            evt.CreatedAt.Should().Be(createdAt);
        }
    }
}
