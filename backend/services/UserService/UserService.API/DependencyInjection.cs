using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UserService.Application;
using UserService.Infrastructure;

namespace UserService.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddControllers();
        services.AddHttpContextAccessor();

        // Application + Infrastructure registrations
        services.AddApplication();
        services.AddInfrastructure(configuration);

        // Cognito/JWT configuration (bind options for testability)
        var cognitoSection = configuration.GetSection("Cognito");
        services.Configure<UserService.Infrastructure.Auth.CognitoOptions>(cognitoSection);

        var cognitoRegion = cognitoSection["Region"];
        var cognitoUserPoolId = cognitoSection["UserPoolId"];
        var cognitoClientId = cognitoSection["ClientId"];
        if (!string.IsNullOrEmpty(cognitoRegion) && !string.IsNullOrEmpty(cognitoUserPoolId))
        {
            var issuer = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{cognitoUserPoolId}";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = issuer;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = cognitoClientId,
                        ValidateLifetime = true
                    };
                    options.RequireHttpsMetadata = true;
                });
        }

        return services;
    }
}
