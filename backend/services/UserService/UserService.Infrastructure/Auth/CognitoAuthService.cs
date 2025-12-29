using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Auth;

/// <summary>
/// Minimal Cognito-focused auth helper. Token validation is handled by JwtBearer middleware; this
/// service provides helpers to read the current user's id and small utilities for higher layers.
/// </summary>
public class CognitoAuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CognitoAuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserIdFromClaims(ClaimsPrincipal principal)
    {
        if (principal == null) return null;
        // Cognito uses 'sub' claim for the unique user id
        return principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetCurrentUserId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.User?.Identity?.IsAuthenticated != true) return null;
        return GetUserIdFromClaims(ctx.User);
    }
}
