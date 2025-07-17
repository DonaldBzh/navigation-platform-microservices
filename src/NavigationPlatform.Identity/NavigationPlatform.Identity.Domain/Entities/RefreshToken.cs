namespace NavigationPlatform.Identity.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
   
    public string Token { get;  set; } = null!;
    public DateTime ExpiresAt { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public bool Revoked { get;  set; }
    public DateTime? RevokedAt { get;  set; }

    public Guid UserId { get;  set; }
    public User User { get; set; } = default!;

    private RefreshToken() { } // EF Core

    private RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        Revoked = false;
    }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken(userId, token, expiresAt);
        return refreshToken;
    }
    public void Revoke()
    {
        if (!Revoked)
        {
            Revoked = true;
            RevokedAt = DateTime.UtcNow;
        }
    }
}

