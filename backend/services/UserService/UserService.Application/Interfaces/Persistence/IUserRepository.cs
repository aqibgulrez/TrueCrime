using UserService.Application.DTOs;

namespace UserService.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<Guid> AddAsync(UserDto user);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByEmailAsync(string email);
    Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? nameFilter, string? emailFilter);
    Task<Guid> CreateWithPasswordAsync(string email, string fullName, string passwordHash, string role = "User");
    Task<string?> GetPasswordHashByEmailAsync(string email);
    Task SetPasswordResetTokenAsync(string email, string token, DateTime expires);
    Task<UserDto?> GetByResetTokenAsync(string token);
    Task<bool> ResetPasswordByTokenAsync(string token, string newPasswordHash);
    Task SetActivationTokenAsync(string email, string token, DateTime expires);
    Task<UserDto?> GetByActivationTokenAsync(string token);
    Task<bool> ActivateByTokenAsync(string token);
}
