using Microsoft.AspNetCore.Mvc;
using UserService.Application.Interfaces.Persistence;
using UserService.Domain.Entities;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(new { user.Id, Email = user.Email.ToString(), user.FullName, user.Role, user.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var email = new UserService.Domain.ValueObjects.Email(request.Email);
        var user = new User(request.FullName, email);
        await _repo.AddAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id });
    }
}

public record CreateUserRequest(string Email, string FullName);
