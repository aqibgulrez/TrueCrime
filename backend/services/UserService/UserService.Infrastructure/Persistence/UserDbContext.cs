using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Entities;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
	public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
	{
	}

	public DbSet<UserEntity> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<UserEntity>(builder =>
		{
			builder.ToTable("Users");
			builder.HasKey(u => u.Id);

			builder.Property(u => u.Email).HasMaxLength(320).HasColumnName("Email");
			builder.Property(u => u.FullName).HasMaxLength(200);
			builder.Property(u => u.Role).HasMaxLength(50);
			builder.Property(u => u.IsActive).IsRequired();
			builder.Property(u => u.CreatedAt).IsRequired();
			builder.Property(u => u.UpdatedAt);
		});
	}
}
