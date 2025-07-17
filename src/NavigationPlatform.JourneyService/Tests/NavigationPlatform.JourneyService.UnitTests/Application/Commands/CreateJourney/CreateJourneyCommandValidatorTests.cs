using FluentValidation.TestHelper;
using NavigationPlatform.JourneyService.Application.Commands.CreateJourney;
using NavigationPlatform.JourneyService.Domain.Enums;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands.CreateJourney;

public class CreateJourneyCommandValidatorTests
{
    private readonly CreateJourneyCommandValidator _validator = new();

    [Theory]
    [InlineData("", "Rome", 10, "2024-01-01", "2024-01-02", "Start location is required")]
    [InlineData("Rome", "", 10, "2024-01-01", "2024-01-02", "Arrival location is required")]
    [InlineData("Rome", "Vienna", 10, "2024-01-03", "2024-01-02", "Start time must be before arrival time")]
    [InlineData("Rome", "Vienna", 10, "2024-01-01", "2024-01-01", "Arrival time must be after start time")]
    [InlineData("Rome", "Vienna", -1, "2024-01-01", "2024-01-02", "Distance must be greater than 0")]
    [InlineData("Rome", "Vienna", 10001, "2024-01-01", "2024-01-02", "Distance seems unrealistic")]
    public void Should_Fail_WithExpectedError(string startLocation, string arrivalLocation, decimal distance, string start, string end, string expectedMessage)
    {
        var command = new CreateJourneyCommand(new CreateJourneyRequest
        {
            StartLocation = startLocation,
            ArrivalLocation = arrivalLocation,
            RouteDistanceKm = distance,
            StartDateTime = DateTime.Parse(start),
            ArrivalDateTime = DateTime.Parse(end),
            TransportationType = TransportationType.Car
        });

        var result = _validator.TestValidate(command);
        result.ShouldHaveAnyValidationError().WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Pass_With_Valid_Input()
    {
        var command = new CreateJourneyCommand(new CreateJourneyRequest
        {
            StartLocation = "Rome",
            ArrivalLocation = "Vienna",
            RouteDistanceKm = 300,
            StartDateTime = DateTime.UtcNow,
            ArrivalDateTime = DateTime.UtcNow.AddHours(3),
            TransportationType = TransportationType.Car
        });

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}