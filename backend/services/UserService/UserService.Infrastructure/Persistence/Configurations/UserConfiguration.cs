using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");
		builder.HasKey(u => u.Id);

		builder.Property(u => u.FullName).HasMaxLength(200);
		builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
		builder.Property(u => u.IsActive).IsRequired();
		builder.Property(u => u.CreatedAt).IsRequired();
		builder.Property(u => u.UpdatedAt);

		// map Email value object
		builder.Property(typeof(Email), "Email").HasConversion(
			v => ((Email)v).ToString(),
			v => new Email(v));
	}
}
