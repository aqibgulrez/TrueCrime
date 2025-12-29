using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
	public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<User>(builder =>
		{
			builder.ToTable("Users");
			builder.HasKey(u => u.Id);

			// Map Email value object to a string column
			var emailConverter = new ValueConverter<Email, string>(
				v => v.Value,
				v => new Email(v));

			builder.Property(nameof(User.Email))
				   .HasConversion(emailConverter)
				   .HasColumnName("Email");

			builder.Property(u => u.FullName).HasMaxLength(200);
			builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
			builder.Property(u => u.IsActive).IsRequired();
			builder.Property(u => u.CreatedAt).IsRequired();
			builder.Property(u => u.UpdatedAt);
		});
	}
}
