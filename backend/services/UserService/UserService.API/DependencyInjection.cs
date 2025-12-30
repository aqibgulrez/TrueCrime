using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
                        ValidateAudience = !string.IsNullOrEmpty(cognitoClientId),
                        ValidAudience = cognitoClientId ?? string.Empty,
                        ValidateLifetime = true,
                        // Use standard role claim type; we'll map Cognito groups into role claims on token validation
                        RoleClaimType = ClaimTypes.Role
                    };
                    options.RequireHttpsMetadata = true;
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = ctx =>
                            {
                                var identity = ctx.Principal?.Identity as ClaimsIdentity;
                                if (identity != null)
                                {
                                    // Map Cognito groups (cognito:groups) into role claims so [Authorize(Roles="...") ] works
                                    var groupClaims = identity.FindAll("cognito:groups");
                                    foreach (var gc in groupClaims)
                                    {
                                        identity.AddClaim(new Claim(identity.RoleClaimType, gc.Value));
                                    }
                                    // Some Cognito setups may emit groups as a single JSON array claim - handle comma-separated fallback
                                    var single = identity.FindFirst("cognito:groups");
                                    if (single != null && single.Value.Contains(",") && !groupClaims.Any())
                                    {
                                        foreach (var part in single.Value.Split(','))
                                        {
                                            var v = part.Trim(' ', '"');
                                            if (!string.IsNullOrEmpty(v)) identity.AddClaim(new Claim(identity.RoleClaimType, v));
                                        }
                                    }
                                }
                                return System.Threading.Tasks.Task.CompletedTask;
                            }
                        };
                });
        }

        return services;
    }
}
