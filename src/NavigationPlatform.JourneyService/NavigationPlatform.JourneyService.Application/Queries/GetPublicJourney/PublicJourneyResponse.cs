using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;

public class PublicJourneyResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Title { get; set; }
    public string TransportationType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public decimal DistanceKm { get; set; }
}
