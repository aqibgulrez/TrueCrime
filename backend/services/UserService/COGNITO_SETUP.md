# Cognito Setup (Dev & Prod)

This file documents an enterprise-grade AWS Cognito setup for both local development and production.

1) Create User Pool (secure defaults)
  - Enable SRP-based authentication (App clients without secret for SPAs) and an App client for server-to-server flows.
  - Enforce password policy (min 12 chars), email verification, and account lockout.
  - Require MFA (optional for dev; enforce in prod for sensitive flows).

2) App Clients & Credentials
  - Create a server app client (no secret for browser clients; with secret for backend-to-backend). Store secrets in AWS Secrets Manager.
  - Note the `UserPoolId` and `ClientId`.

3) Token Validation
  - API uses JWT validation against issuer: `https://cognito-idp.{region}.amazonaws.com/{userPoolId}`
  - Configure `JwtBearer` authority and audience (see `Program.cs`).
  - Use RS256 and validate tokens against Cognito JWKS endpoint.

4) Dev workflow
  - Option A (recommended): create a dedicated dev/staging user pool in your AWS account for parity.
  - Option B: use the same prod pool with strict IAM access controls (not recommended).
  - Configure `appsettings.Development.json` with `Cognito:Region`, `Cognito:UserPoolId`, `Cognito:ClientId`.

5) Secrets & Rotation
  - Store app client secrets in AWS Secrets Manager; provide the ARN to CI/CD and runtime via IAM task role.
  - Rotate keys and secret values periodically.

6) Monitoring & Security
  - Enable CloudWatch logs for Cognito triggers and CloudTrail for admin operations.
  - Add WAF and API Gateway rate-limiting for endpoints that accept credentials.

7) Admin & Migration
  - For user import/migration use Cognito import or AdminCreateUser with secure temporary password flows.

8) Local testing
  - Use test users in the dev pool; CI can obtain tokens via AWS SDK for integration tests.

9) Config (example)
  Add to `appsettings.Development.json` in `UserService.API`:

```json
{
  "Cognito": {
    "Region": "us-east-1",
    "UserPoolId": "us-east-1_XXXXXXXXX",
    "ClientId": "xxxxxxxxxxxxxxxxxxxxxxxxxx"
  }
}
```

10) Next steps
  - Add IAM roles for ECS task to read secrets.
  - Add automated tests that exercise token validation and protected endpoints.
