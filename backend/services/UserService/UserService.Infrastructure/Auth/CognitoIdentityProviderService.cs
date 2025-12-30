using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Auth
{
    public class CognitoIdentityProviderService : IIdentityProviderService
    {
        private readonly IAmazonCognitoIdentityProvider _client;
        private readonly CognitoOptions _options;

        public CognitoIdentityProviderService(IAmazonCognitoIdentityProvider client, IOptions<CognitoOptions> options)
        {
            _client = client;
            _options = options.Value ?? new CognitoOptions();
        }

        public async Task<bool> RegisterAsync(string email, string password, string fullName, string role = "User")
        {
            if (string.IsNullOrEmpty(_options.UserPoolId)) throw new InvalidOperationException("Cognito UserPoolId not configured");

            // Create the user (admin flow) and set a permanent password so the user can sign in immediately.
            var adminCreate = new AdminCreateUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = email,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = email },
                    new AttributeType { Name = "email_verified", Value = "true" },
                    new AttributeType { Name = "name", Value = fullName }
                },
                MessageAction = "SUPPRESS"
            };

            await _client.AdminCreateUserAsync(adminCreate).ConfigureAwait(false);

            // Set permanent password
            var setPwd = new AdminSetUserPasswordRequest
            {
                Username = email,
                UserPoolId = _options.UserPoolId,
                Password = password,
                Permanent = true
            };

            await _client.AdminSetUserPasswordAsync(setPwd).ConfigureAwait(false);

            // Optionally attach to groups for role (admin vs user) -- skip if default
            if (!string.IsNullOrEmpty(role) && !string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
            {
                var addToGroup = new AdminAddUserToGroupRequest
                {
                    UserPoolId = _options.UserPoolId,
                    Username = email,
                    GroupName = role
                };
                await _client.AdminAddUserToGroupAsync(addToGroup).ConfigureAwait(false);
            }

            // Disable the user until they activate via email link
            var disable = new AdminDisableUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = email
            };
            await _client.AdminDisableUserAsync(disable).ConfigureAwait(false);

            return true;
        }

        public async Task<AuthenticateResult?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(_options.UserPoolId) || string.IsNullOrEmpty(_options.ClientId)) throw new InvalidOperationException("Cognito not configured");

            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = _options.UserPoolId,
                ClientId = _options.ClientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password }
                }
            };

            var resp = await _client.AdminInitiateAuthAsync(authReq).ConfigureAwait(false);
            var result = resp.AuthenticationResult;
            if (result == null) return null;

            return new AuthenticateResult(result.AccessToken, result.IdToken, result.RefreshToken, result.ExpiresIn);
        }
        public async Task<bool> InitiateForgotPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(_options.ClientId)) throw new InvalidOperationException("Cognito ClientId not configured");

            var req = new ForgotPasswordRequest
            {
                ClientId = _options.ClientId,
                Username = email
            };

            var resp = await _client.ForgotPasswordAsync(req).ConfigureAwait(false);
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> ConfirmForgotPasswordAsync(string email, string confirmationCode, string newPassword)
        {
            if (string.IsNullOrEmpty(_options.ClientId)) throw new InvalidOperationException("Cognito ClientId not configured");

            var req = new ConfirmForgotPasswordRequest
            {
                ClientId = _options.ClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
                Password = newPassword
            };

            var resp = await _client.ConfirmForgotPasswordAsync(req).ConfigureAwait(false);
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> EnableUserAsync(string email)
        {
            if (string.IsNullOrEmpty(_options.UserPoolId)) throw new InvalidOperationException("Cognito UserPoolId not configured");
            var req = new AdminEnableUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = email
            };
            var resp = await _client.AdminEnableUserAsync(req).ConfigureAwait(false);
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
