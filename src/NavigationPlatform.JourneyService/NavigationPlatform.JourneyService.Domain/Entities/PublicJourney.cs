namespace NavigationPlatform.JourneyService.Domain.Entities;

public class PublicJourney
{
    public Guid Id { get;  set; }
    public Guid JourneyId { get;  set; }
    public string PublicToken { get;  set; } = string.Empty;
    public Guid CreatedByUserId { get;  set; } 
    public DateTime CreatedAt { get;  set; }
    public DateTime? ExpiresAt { get;  set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get;  set; }
    public int AccessCount { get;  set; }
    public Journey Journey { get;  set; } = null!;

   
}