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
        // AWS Cognito identity provider client and wrapper
        var cognitoSection = configuration.GetSection("Cognito");
        services.Configure<CognitoOptions>(opts =>
        {
            opts.Region = cognitoSection["Region"];
            opts.UserPoolId = cognitoSection["UserPoolId"];
            opts.ClientId = cognitoSection["ClientId"];
        });
        services.AddSingleton<Amazon.CognitoIdentityProvider.IAmazonCognitoIdentityProvider>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CognitoOptions>>().Value;
            var region = string.IsNullOrEmpty(opts.Region) ? Amazon.RegionEndpoint.USEast1 : Amazon.RegionEndpoint.GetBySystemName(opts.Region);
            return new Amazon.CognitoIdentityProvider.AmazonCognitoIdentityProviderClient(region);
        });

        // Use Cognito identity provider for both dev and prod (no local fallback)
        services.AddScoped<UserService.Application.Interfaces.IIdentityProviderService, CognitoIdentityProviderService>();
        // Email sender
        services.AddSingleton<UserService.Application.Interfaces.IEmailSender, UserService.Infrastructure.Email.SmtpEmailSender>();

        return services;
    }
}
