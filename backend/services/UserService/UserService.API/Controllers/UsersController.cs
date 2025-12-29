using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Persistence;
using UserService.Domain.Entities;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IAuthService _auth;

    public UsersController(IUserRepository repo, IAuthService auth)
    {
        _repo = repo;
        _auth = auth;
    }

    [HttpGet("{id}")]
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var dto = new UserService.Application.DTOs.UserDto(Guid.Empty, request.FullName, request.Email);
        var id = await _repo.AddAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = id }, new { id });
    }
}

public record CreateUserRequest(string Email, string FullName);
