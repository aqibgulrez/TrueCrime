using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Persistence;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Auth;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["DATABASE_URL"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));
        }

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Auth helpers (implementation lives in Infrastructure)
        services.AddScoped<IAuthService, CognitoAuthService>();

        return services;
    }
}
