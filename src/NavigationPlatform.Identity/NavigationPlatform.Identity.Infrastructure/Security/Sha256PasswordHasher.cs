using System.Text;
using System.Security.Cryptography;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
namespace NavigationPlatform.Identity.Infrastructure.Security;

public class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool Verify(string password, string storedHash)
    {
        var hashOfInput = Hash(password);
        return hashOfInput == storedHash;
    }
}
