using NavigationPlatform.JourneyService.Domain.Enums;

namespace NavigationPlatform.JourneyService.Domain.Entities;

public class Journey
{
    public Guid Id { get;  set; }
    public Guid UserId { get;  set; }
    public string Name { get; set; }
    public string StartLocation { get;  set; } = string.Empty;
    public decimal? StartLatitude { get;  set; }
    public decimal? StartLongitude { get;  set; }
    public DateTime StartDate { get;  set; }

    public string ArrivalLocation { get;  set; } = string.Empty;
    public decimal? ArrivalLatitude { get;  set; }
    public decimal? ArrivalLongitude { get;  set; }
    public DateTime? ArrivalDate { get;  set; }

    public TransportationType TransportationType { get;  set; }
    public decimal RouteDistanceKm { get;  set; }
    public string? Notes { get;  set; }
    public bool IsDailyGoalAchieved { get;  set; }

    // Audit
    public DateTime CreatedAt { get;  set; }
    public DateTime? UpdatedAt { get;  set; }
    public bool IsDeleted { get;  set; }

    public List<JourneyShare> Shares { get;  set; } = new();
    public List<PublicJourney> PublicLinks { get;  set; } = new();
    public List<SharingAuditLog> AuditLogs { get;  set; } = new();

    
}
