using System.Threading.Tasks;
using UserService.Application.DTOs;

namespace UserService.Application.Interfaces
{
    public interface IIdentityProviderService
    {
        /// <summary>
        /// Register a new user in the external identity provider (e.g., Cognito).
        /// Returns true on success.
        /// </summary>
        Task<bool> RegisterAsync(string email, string password, string fullName, string role = "User");

        /// <summary>
        /// Authenticate the user against the identity provider and return authentication tokens (id/access/refresh).
        /// </summary>
        Task<AuthenticateResult?> AuthenticateAsync(string email, string password);
        Task<bool> InitiateForgotPasswordAsync(string email);
        Task<bool> ConfirmForgotPasswordAsync(string email, string confirmationCode, string newPassword);
        Task<bool> EnableUserAsync(string email);
    }

    public record AuthenticateResult(string AccessToken, string IdToken, string RefreshToken, int ExpiresInSeconds);
}
