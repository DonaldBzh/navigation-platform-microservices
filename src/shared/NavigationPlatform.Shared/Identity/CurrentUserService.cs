using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace NavigationPlatform.Shared.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var userIdHeader = httpContext.Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdHeader))
        {
            throw new UnauthorizedAccessException("User ID not found.");
        }

        if (!Guid.TryParse(userIdHeader, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid User ID format");
        }

        return userId;
    }
}
