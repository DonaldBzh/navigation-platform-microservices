using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Domain.Events;

public class JourneyCreatedEvent
{
    public Guid JourneyId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal DistanceKm { get; set; }
    public string TransportationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
