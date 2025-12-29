using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Persistence;
using UserService.Infrastructure.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
	private readonly UserDbContext _db;

	public UserRepository(UserDbContext db)
	{
		_db = db;
	}

	public async Task<Guid> AddAsync(UserDto userDto)
	{
		var entity = new UserEntity
		{
			FullName = userDto.FullName,
			Email = userDto.Email,
			Role = userDto.Role,
			IsActive = userDto.IsActive
		};

		await _db.Users.AddAsync(entity);
		await _db.SaveChangesAsync();
		return entity.Id;
	}

	public async Task<IEnumerable<UserDto>> GetAllAsync()
	{
		return await _db.Users.AsNoTracking()
			.Select(u => new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive))
			.ToListAsync();
	}

	public async Task<UserDto?> GetByIdAsync(Guid id)
	{
		var u = await _db.Users.FindAsync(id);
		if (u == null) return null;
		return new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
	}
}
