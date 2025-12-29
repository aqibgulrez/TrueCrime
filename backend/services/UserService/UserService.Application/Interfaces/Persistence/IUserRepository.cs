using UserService.Application.DTOs;

namespace UserService.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<Guid> AddAsync(UserDto user);
    Task<IEnumerable<UserDto>> GetAllAsync();
}
