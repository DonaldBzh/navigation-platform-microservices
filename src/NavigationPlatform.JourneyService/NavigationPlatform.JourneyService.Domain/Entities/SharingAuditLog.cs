using NavigationPlatform.JourneyService.Domain.Enums;

namespace NavigationPlatform.JourneyService.Domain.Entities;

public class SharingAuditLog
{
    public Guid Id { get; set; }
    public Guid JourneyId { get; set; }
    public Guid UserId { get; set; }
    public SharingAction Action { get; set; }
    public Guid? TargetUserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public Journey Journey { get; set; } = null!;



}