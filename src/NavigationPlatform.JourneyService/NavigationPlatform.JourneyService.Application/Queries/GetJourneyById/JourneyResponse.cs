using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;

public class JourneyResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string StartLocation { get; set; }
    public DateTime StartDate { get; set; }
    public string ArrivalLocation { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string TransportationType { get; set; }
    public decimal DistanceKm { get; set; }
    public bool IsDailyGoalAchieved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
