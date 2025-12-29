CIN Context - Environments & Services

Environments (for now)
- `dev` — local Docker + ephemeral resources for development and integration testing
- `production` — AWS-hosted resources (Cognito, ECS Fargate, RDS Postgres, S3/CloudFront)

Services
- `UserService`
  - Responsibilities: user profile management, link to Cognito identities, basic user settings and roles
  - Env vars: `DATABASE_URL`, `COGNITO_USERPOOL_ID`, `COGNITO_CLIENT_ID`, `AWS_REGION`

- `StoryService`
  - Responsibilities: orchestrate story generation via OpenAI, store only the user's most-recent story (design DB to allow multiple later), provide content-moderation hooks
  - Env vars: `OPENAI_API_KEY`, `DATABASE_URL`, `CONTENT_MODERATOR_CONFIG`

- `PaymentService`
  - Responsibilities: integrate with Stripe for subscription billing, handle webhooks (subscription created/cancelled/payment_failed), map Stripe customers to app users
  - Env vars: `STRIPE_API_KEY`, `STRIPE_WEBHOOK_SECRET`, `DATABASE_URL`

- `AdminService`
  - Responsibilities: admin UI/API for viewing users, subscription status, payments, basic usage metrics and logs; callable only by admin roles (Cognito groups)
  - Env vars: `DATABASE_URL`, `ADMIN_AUDIT_LOG_CONFIG`

Database strategy
- Single PostgreSQL (RDS) instance used by all services to save cost. Options:
  - Shared schema with well-namespaced tables (e.g., `users`, `stories`, `payments`)
  - Or logical schemas per service within the same DB (e.g., `user`, `story`, `payment`) for separation
- Store only latest story initially (either `Users.LatestStoryId` or `UserStories` table with `IsLatest` flag). Include metadata: `created_at`, `token_cost`, `moderation_status`.

Local dev
- Use Docker Compose to run Postgres, local SMTP/mock services, and the API services. Use `.env.dev` for local env vars.

Production infra mapping (high-level)
- Auth: AWS Cognito (user pools & groups)
- Compute: ECS Fargate tasks per service behind ALB or API Gateway
- DB: Amazon RDS (PostgreSQL) single instance / cluster
- Frontend: React app hosted in S3 + CloudFront
- Logging & Observability: CloudWatch + OpenTelemetry traces shipped to a tracing backend
- Secrets: AWS Secrets Manager / SSM Parameter Store

CI/CD & deployment notes
- Create pipelines per service (build, test, security scan, container publish, deploy to ECS). Use GitHub Actions or AWS CodePipeline.
- Maintain IaC (Terraform/CloudFormation) for Cognito, RDS, ECS, S3, and IAM roles.

Security & compliance (quick notes)
- Never store raw card data; use Stripe tokens and webhooks.
- Use least-privilege IAM roles for ECS tasks.
- Ensure encryption at rest (RDS) and in transit (TLS).

Operational behavior
- Subscription gating: `PaymentService` authoritatively tracks subscription state; other services validate access by checking subscription status on requests (cached locally for performance).
- Story retention: only latest story saved; plan migrations to expand retention later.

File created by assistant.
