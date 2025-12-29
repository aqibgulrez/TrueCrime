using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
}
