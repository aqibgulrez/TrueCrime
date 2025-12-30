using System;
using System.Threading.Tasks;
using UserService.Application.Interfaces;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Persistence;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure.Auth
{
    /// <summary>
    /// Local development identity provider that stores password hashes in the local DB and sends reset/activation emails via IEmailSender.
    /// Not for production.
    /// </summary>
    public class LocalIdentityProviderService : IIdentityProviderService
    {
        private readonly IUserRepository _repo;
        private readonly UserService.Application.Interfaces.IEmailSender _emails;

        public LocalIdentityProviderService(IUserRepository repo, UserService.Application.Interfaces.IEmailSender emails)
        {
            _repo = repo;
            _emails = emails;
        }

        public async Task<bool> RegisterAsync(string email, string password, string fullName, string role = "User")
        {
            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null) return false;
            // For local dev we do not create the local profile here; controller will create a profile and set activation token.
            // Just validate password rules and succeed.
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
            return true;
        }

        public async Task<AuthenticateResult?> AuthenticateAsync(string email, string password)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;
            if (!user.IsActive) return null;

            var hash = await _repo.GetPasswordHashByEmailAsync(email);
            if (hash == null) return null;
            if (!PasswordHasher.Verify(password, hash)) return null;

            // No tokens in local provider; return a minimal AuthenticateResult with empty tokens
            return new AuthenticateResult(string.Empty, string.Empty, string.Empty, 0);
        }

        public async Task<bool> InitiateForgotPasswordAsync(string email)
        {
            // generate code and send via email
            var codeBytes = new byte[6];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(codeBytes);
            var code = Convert.ToBase64String(codeBytes).TrimEnd('=');
            var expiry = DateTime.UtcNow.AddHours(1);
            await _repo.SetPasswordResetTokenAsync(email, code, expiry);
            await _emails.SendEmailAsync(email, "Password reset code", $"Your code: {code}");
            return true;
        }

        public async Task<bool> ConfirmForgotPasswordAsync(string email, string confirmationCode, string newPassword)
        {
            var user = await _repo.GetByResetTokenAsync(confirmationCode);
            if (user == null || !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)) return false;
            var newHash = PasswordHasher.HashPassword(newPassword);
            return await _repo.ResetPasswordByTokenAsync(confirmationCode, newHash);
        }

        public async Task<bool> EnableUserAsync(string email)
        {
            // Local enable: mark IsActive true via activation token flow in repository (controller calls ActivateByTokenAsync)
            // Nothing to do here for local provider.
            return true;
        }
    }
}
