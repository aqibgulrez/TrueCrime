using System;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Persistence;

namespace UserService.Application.Features.Users.Commands.Auth;

public class LoginHandler
{
    private readonly IUserRepository _repo;

    public LoginHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<UserDto?> Handle(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return null;
        var user = await _repo.GetByEmailAsync(request.Email);
        if (user == null) return null;
        // Retrieve stored hash via repository (not implemented) — placeholder logic
        return user;
    }
}
