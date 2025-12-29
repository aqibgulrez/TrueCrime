using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Domain.ValueObjects;
using Xunit;

namespace UserService.Tests;

public class RepositoryTests
{
    [Fact]
    public async Task AddAndGetUser_Works()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddAndGetUser")
            .Options;

        await using (var context = new UserDbContext(options))
        {
            var repo = new UserRepository(context);
            var dto = new UserService.Application.DTOs.UserDto(Guid.Empty, "Test User", "test@example.com");
            var id = await repo.AddAsync(dto);
            var fetched = await repo.GetByIdAsync(id);
            Assert.NotNull(fetched);
            Assert.Equal("test@example.com", fetched.Email);
        }
    }
}
