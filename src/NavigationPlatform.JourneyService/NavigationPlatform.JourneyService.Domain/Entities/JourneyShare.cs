namespace NavigationPlatform.JourneyService.Domain.Entities;

public class JourneyShare
{
    public Guid Id { get;  set; }
    public Guid JourneyId { get;  set; }
    public Guid SharedByUserId { get;  set; } 
    public Guid SharedWithUserId { get;  set; } 
    public DateTime SharedAt { get;  set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get;  set; }

    public Journey Journey { get;  set; } = null!;

    
}