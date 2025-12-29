namespace UserService.Infrastructure.Auth;

public class CognitoOptions
{
    public string? Region { get; set; }
    public string? UserPoolId { get; set; }
    public string? ClientId { get; set; }
}
