using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Persistence;
using UserService.Infrastructure.Security;


namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IAuthService _auth;
    private readonly UserService.Application.Interfaces.IEmailSender _emails;
    private readonly IConfiguration _config;
    private readonly UserService.Application.Interfaces.IIdentityProviderService _identity;

    public UsersController(IUserRepository repo, IAuthService auth, UserService.Application.Interfaces.IEmailSender emails, IConfiguration config, UserService.Application.Interfaces.IIdentityProviderService identity)
    {
        _repo = repo;
        _auth = auth;
        _emails = emails;
        _config = config;
        _identity = identity;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Ensure caller is owner (or later: admin). Current simple check ensures a user can only retrieve their own record.
        var currentUserId = _auth.GetCurrentUserId();
        if (currentUserId == null) return Forbid();

        if (!Guid.TryParse(currentUserId, out var callerGuid) || callerGuid != id)
        {
            return Forbid();
        }

        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(new { user.Id, Email = user.Email, user.FullName, user.Role, user.IsActive });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest("Invalid registration data");

        var existing = await _repo.GetByEmailAsync(request.Email);
        if (existing != null) return Conflict("Email already registered");

        // Register in Cognito (or other identity provider)
        var regOk = await _identity.RegisterAsync(request.Email, request.Password, request.FullName);
        if (!regOk) return StatusCode(500, "Unable to create user in identity provider");

        // Create local profile without storing password and generate activation token
        var newId = await _repo.AddAsync(new UserService.Application.DTOs.UserDto(Guid.Empty, request.FullName, request.Email));

        var tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = WebEncoders.Base64UrlEncode(tokenBytes);
        var expiry = DateTime.UtcNow.AddHours(24);
        await _repo.SetActivationTokenAsync(request.Email, token, expiry);

        var baseUrl = _config["Auth:ActivationBaseUrl"] ?? _config["Frontend:ActivationUrl"] ?? _config["App:ActivationUrl"];
        var activationUrl = string.IsNullOrEmpty(baseUrl) ? $"/activate?token={token}&email={Uri.EscapeDataString(request.Email)}" :
            (baseUrl.Contains("{token}") ? baseUrl.Replace("{token}", Uri.EscapeDataString(token)) : baseUrl + $"?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}");

        var subject = "Activate your account";
        var body = $"<p>Welcome {request.FullName},</p><p>Please activate your account by clicking the link below:</p><p><a href=\"{activationUrl}\">Activate account</a></p>";
        await _emails.SendEmailAsync(request.Email, subject, body);

        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return BadRequest("Invalid login");

        var auth = await _identity.AuthenticateAsync(request.Email, request.Password);
        if (auth == null) return Unauthorized();

        var user = await _repo.GetByEmailAsync(request.Email);
        // Best-effort: return profile + tokens
        return Ok(new { Tokens = auth, Profile = user });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest("Email required");

        // Use Cognito to initiate forgot-password flow which sends a code to the user's email
        try
        {
            await _identity.InitiateForgotPasswordAsync(request.Email);
        }
        catch
        {
            // don't reveal whether email exists or if provider failed
        }

        return Ok();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // Here the client should provide email + confirmation code + new password (Cognito flow)
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest("Invalid request");

        // We treat Token as the confirmation code from Cognito and Token payload includes email in query param on client side.
        // For server-side confirm, require email in query string or in body. To keep compatibility, expect header X-Reset-Email or query param.
        var email = Request.Query["email"].FirstOrDefault() ?? Request.Headers["X-Reset-Email"].FirstOrDefault();
        if (string.IsNullOrEmpty(email)) return BadRequest("Email required for password reset confirmation");

        var ok = await _identity.ConfirmForgotPasswordAsync(email, request.Token, request.NewPassword);
        if (!ok) return BadRequest("Unable to reset password");
        return Ok();
    }

    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<IActionResult> Activate([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token required");

        var user = await _repo.GetByActivationTokenAsync(token);
        if (user == null) return BadRequest("Invalid or expired token");

        // Enable user in Cognito
        var enabled = await _identity.EnableUserAsync(user.Email);
        if (!enabled) return StatusCode(500, "Unable to enable user in identity provider");

        var ok = await _repo.ActivateByTokenAsync(token);
        if (!ok) return StatusCode(500, "Unable to activate user profile");

        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? name = null, [FromQuery] string? email = null)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 200) pageSize = 20;

        var result = await _repo.GetPagedAsync(page, pageSize, name, email);
        return Ok(result);
    }
}

public record CreateUserRequest(string Email, string FullName, string Password);
public record LoginRequest(string Email, string Password);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
