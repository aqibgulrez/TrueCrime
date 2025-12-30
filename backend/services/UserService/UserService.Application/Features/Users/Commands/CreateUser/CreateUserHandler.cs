using System;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Persistence;

namespace UserService.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler
{
    private readonly IUserRepository _repo;

    public CreateUserHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(CreateUserRequestDto request)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email required");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

        var existing = await _repo.GetByEmailAsync(request.Email);
        if (existing != null) throw new InvalidOperationException("Email already registered");

        var dto = new UserDto(Guid.Empty, request.FullName, request.Email);
        var id = await _repo.AddAsync(dto);
        // After create, set password hash directly in DB (simpler path: repository can expose method to set password)
        // For now, load entity, set hash via repo implementation helper (not present) — quick workaround: not implemented.
        return id;
    }
}
