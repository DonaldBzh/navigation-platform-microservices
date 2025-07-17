using NavigationPlatform.Identity.Domain.Enums;

namespace NavigationPlatform.Identity.Domain.Entities;

public class User 
{
    public Guid Id { get; set; }
    public string Email { get;  set; } = null!;
    public string PasswordHash { get;  set; } = null!;
    public UserStatus Status { get;  set; }
    public UserRole Role { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public DateTime? UpdatedAt { get;  set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>(); 

    public User() { }
    private User(string email, string passwordHash, UserRole role = UserRole.User)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string email, string passwordHash, UserRole role = UserRole.User)
    {
        var user = new User(email, passwordHash, role);
        return user;
    }

    public void ChangeStatus(UserStatus newStatus, Guid changedByUserId)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
