using System.Security.Claims;

namespace UserService.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Returns the subject/user id from a validated ClaimsPrincipal, or null if not present.
    /// </summary>
    string? GetUserIdFromClaims(ClaimsPrincipal principal);

    /// <summary>
    /// Convenience to get the current authenticated user's id from the HTTP context.
    /// Returns null if no user is authenticated.
    /// </summary>
    string? GetCurrentUserId();
}
