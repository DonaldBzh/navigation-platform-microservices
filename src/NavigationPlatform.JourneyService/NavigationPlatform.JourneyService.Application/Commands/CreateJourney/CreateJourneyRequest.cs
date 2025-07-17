using NavigationPlatform.JourneyService.Domain.Enums;

namespace NavigationPlatform.JourneyService.Application.Commands.CreateJourney;

public class CreateJourneyRequest
{
    public string StartLocation { get; set; }
    public DateTime StartDateTime { get; set; }
    public string Name { get; set; }
    public string ArrivalLocation { get; set; }
    public DateTime ArrivalDateTime { get; set; }
    public TransportationType TransportationType { get;  set; }
    public decimal RouteDistanceKm { get;  set; }
}