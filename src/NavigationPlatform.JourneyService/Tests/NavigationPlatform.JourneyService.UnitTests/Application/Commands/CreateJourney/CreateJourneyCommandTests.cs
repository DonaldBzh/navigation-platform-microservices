using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Commands.CreateJourney;
using NavigationPlatform.JourneyService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands.CreateJourney;

public class CreateJourneyCommandTests
{
    [Fact]
    public void Constructor_Should_SetDataPropertyCorrectly()
    {
        // Arrange
        var request = new CreateJourneyRequest
        {
            StartLocation = "Paris",
            StartDateTime = DateTime.UtcNow,
            ArrivalLocation = "Berlin",
            ArrivalDateTime = DateTime.UtcNow.AddHours(8),
            TransportationType = TransportationType.Train,
            RouteDistanceKm = 1050.5m
        };

        // Act
        var command = new CreateJourneyCommand(request);

        // Assert
        command.Data.Should().BeSameAs(request);
        command.Data.StartLocation.Should().Be("Paris");
        command.Data.ArrivalLocation.Should().Be("Berlin");
        command.Data.TransportationType.Should().Be(TransportationType.Train);
        command.Data.RouteDistanceKm.Should().Be(1050.5m);
    }
}
