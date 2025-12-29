using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Persistence;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
	private readonly UserDbContext _db;

	public UserRepository(UserDbContext db)
	{
		_db = db;
	}

	public async Task AddAsync(User user)
	{
		await _db.Users.AddAsync(user);
		await _db.SaveChangesAsync();
	}

	public async Task<IEnumerable<User>> GetAllAsync()
	{
		return await _db.Users.AsNoTracking().ToListAsync();
	}

	public async Task<User> GetByIdAsync(Guid id)
	{
		return await _db.Users.FindAsync(id);
	}
}
