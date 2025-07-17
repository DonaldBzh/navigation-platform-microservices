using MediatR;

namespace NavigationPlatform.JourneyService.Application.Events;

public class JourneyCreatedEventNotification : INotification
{
    public Guid JourneyId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal DistanceKm { get; set; }
    public DateTime CreatedAt { get; set; }
}

