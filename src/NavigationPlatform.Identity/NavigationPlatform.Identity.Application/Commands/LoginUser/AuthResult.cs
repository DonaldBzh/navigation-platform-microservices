namespace NavigationPlatform.Identity.Application.Commands.LoginUser;

public class AuthResult
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpiresAt { get; set; }
}