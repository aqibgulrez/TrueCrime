using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Infrastructure.Entities;

namespace UserService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
	public void Configure(EntityTypeBuilder<UserEntity> builder)
	{
		builder.ToTable("Users");
		builder.HasKey(u => u.Id);

		builder.Property(u => u.FullName).HasMaxLength(200);
		builder.Property(u => u.Role).HasMaxLength(50);
		builder.Property(u => u.IsActive).IsRequired();
		builder.Property(u => u.CreatedAt).IsRequired();
		builder.Property(u => u.UpdatedAt);
	}
}
