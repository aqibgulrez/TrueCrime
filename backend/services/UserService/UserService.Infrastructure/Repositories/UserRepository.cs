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

		// If PasswordHash exists on DTO (overload later), this will be set by higher layer. For now leave blank.

		await _db.Users.AddAsync(entity);
		await _db.SaveChangesAsync();
		return entity.Id;
	}

	public async Task<UserDto?> GetByEmailAsync(string email)
	{
		var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
		if (u == null) return null;
		return new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
	}

	public async Task<Guid> CreateWithPasswordAsync(string email, string fullName, string passwordHash, string role = "User")
	{
		var entity = new UserEntity
		{
			Email = email,
			FullName = fullName,
			Role = role,
			PasswordHash = passwordHash,
			IsActive = true
		};

		await _db.Users.AddAsync(entity);
		await _db.SaveChangesAsync();
		return entity.Id;
	}

	public async Task<string?> GetPasswordHashByEmailAsync(string email)
	{
		var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
		return u?.PasswordHash;
	}

	public async Task SetPasswordResetTokenAsync(string email, string token, DateTime expires)
	{
		var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
		if (u == null) return;
		u.PasswordResetToken = token;
		u.PasswordResetExpiry = expires;
		await _db.SaveChangesAsync();
	}

	public async Task SetActivationTokenAsync(string email, string token, DateTime expires)
	{
		var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
		if (u == null) return;
		u.ActivationToken = token;
		u.ActivationExpiry = expires;
		u.IsActive = false;
		await _db.SaveChangesAsync();
	}

	public async Task<UserDto?> GetByResetTokenAsync(string token)
	{
		var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.PasswordResetToken == token && x.PasswordResetExpiry >= DateTime.UtcNow);
		if (u == null) return null;
		return new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
	}

	public async Task<UserDto?> GetByActivationTokenAsync(string token)
	{
		var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.ActivationToken == token && x.ActivationExpiry >= DateTime.UtcNow);
		if (u == null) return null;
		return new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
	}

	public async Task<bool> ResetPasswordByTokenAsync(string token, string newPasswordHash)
	{
		var u = await _db.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token && x.PasswordResetExpiry >= DateTime.UtcNow);
		if (u == null) return false;
		u.PasswordHash = newPasswordHash;
		u.PasswordResetToken = null;
		u.PasswordResetExpiry = null;
		await _db.SaveChangesAsync();
		return true;
	}

	public async Task<bool> ActivateByTokenAsync(string token)
	{
		var u = await _db.Users.FirstOrDefaultAsync(x => x.ActivationToken == token && x.ActivationExpiry >= DateTime.UtcNow);
		if (u == null) return false;
		u.ActivationToken = null;
		u.ActivationExpiry = null;
		u.IsActive = true;
		await _db.SaveChangesAsync();
		return true;
	}

	public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? nameFilter, string? emailFilter)
	{
		var query = _db.Users.AsNoTracking().AsQueryable();

		if (!string.IsNullOrWhiteSpace(nameFilter))
		{
			query = query.Where(u => u.FullName.Contains(nameFilter));
		}
		if (!string.IsNullOrWhiteSpace(emailFilter))
		{
			query = query.Where(u => u.Email.Contains(emailFilter));
		}

		var total = await query.CountAsync();
		var items = await query
			.OrderBy(u => u.FullName)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(u => new UserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive))
			.ToListAsync();

		return new PagedResult<UserDto>(items, total, page, pageSize);
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
