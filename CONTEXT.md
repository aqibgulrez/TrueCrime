Project: TrueCrime — high-level context

I am building a true crime chatbot app.

Goal
- Enterprise-grade application hosted on AWS with testing, security, performance, and clean architecture.

Tech stack & infra
- Backend: .NET Web API (C#)
- Frontend: React (served from S3/CloudFront)
- Frontend: React (served from S3/CloudFront)
- API Gateway: frontend will call backend APIs through AWS API Gateway (edge-optimized or regional as appropriate)

API Gateway security
- The API Gateway will enforce enterprise-level security controls:
	- Authentication & Authorization: use AWS Cognito JWT authorizers (or Lambda authorizers) to enforce user and admin roles.
	- Transport security: custom domain with TLS, enforce HTTPS, support mutual TLS for service-to-service where required.
	- Threat protection: AWS WAF for OWASP rules, IP reputation, and geo-blocking; AWS Shield for DDoS protection.
	- Rate limiting & throttling: per-user and per-api throttles to prevent abuse; integrate with usage plans and API keys if needed.
	- Request validation & size limits: validate request schemas, body size, and reject malformed or oversized requests.
	- Logging & tracing: enable full request/response logging, integrate with CloudWatch and OpenTelemetry tracing for observability.
	- Webhook & replay protection: validate signatures on webhook endpoints and use idempotency keys for critical operations.
	- Least-privilege IAM: restrict API Gateway integrations via execution roles and limit access to backend resources.

- Auth: AWS Cognito
- Compute: ECS Fargate
- Database: PostgreSQL (RDS) — single database shared across apps for cost reasons
- Hosting: React in S3, backend in ECS Fargate
- Logging: Centralized logs (CloudWatch / Fluentd / OpenTelemetry)

Local development
- Use Docker for local dev (compose to replicate services: API, DB, local IAM mocks as needed)

Architecture expectations
- Clean architecture: Domain, Application, Infrastructure, API layers (separation of concerns)
- Automated tests: unit, integration, end-to-end, and contract tests
- CI/CD: pipelines for build, test, security scans, and deploy (GitHub Actions / CodePipeline)
- Security: static analysis, dependency scanning, secrets management (Secrets Manager / Parameter Store), least-privilege IAM roles
- Performance: load testing, caching, connection pooling, metrics and tracing (OpenTelemetry)
- Observability: logs, metrics, traces, error reporting, alerts
- Infra as code: Terraform or CloudFormation for Cognito, ECS, RDS, S3, log resources

Operational notes
- Use a single PostgreSQL RDS instance with database schema and role separation as necessary
- Design services to be stateless where possible; keep session/state in durable stores

Build & config notes
- Use MSBuild `.props` files to centralize package/version references and shared build settings so updating references is easy across projects.
- PostgreSQL local data files: configure a host folder (for example `./pgdata`) mounted into the Postgres Docker container so DB files live outside the repository and can be managed per-developer.
	- Do NOT commit Postgres `pgdata` directories or binary data files into git.
	- To bootstrap local environments, include SQL migration scripts or a sanitized SQL dump in the repo that developers can import. If a binary snapshot is needed, keep it outside the repo and share via secure storage; avoid committing it to git.

Repository notes (from quick scan)
- Project already uses solution with `UserService` split into API, Application, Domain, Infrastructure projects — aligns with clean architecture.
- Several build artifacts (`bin/`, `obj/`) are present in the repo history — add/update `.gitignore` and remove committed artifacts.
- `UserRepository.cs` and `UserDbContext.cs` files exist but are currently empty; they should be implemented and wired into DI.
- No `tests/` project found yet — add unit and integration test projects.

Next immediate steps I can take for you
- Add a `.gitignore` to exclude `bin/`, `obj/`, secrets, and IDE files, then remove committed build artifacts from history (or from the working tree) and commit the removal.
- Implement basic `UserRepository` and `UserDbContext` scaffolding with Entity Framework Core mappings.
- Add a `tests/` project and a sample unit test.
- Continue creating infra IaC skeleton (Terraform modules) for Cognito, ECS, RDS, S3.

Product & business rules
- The chatbot is a paid service: users pay a monthly subscription to access the chatbot page.
 - The chatbot is a paid service: users pay for a subscription to access the chatbot page.
 - Support recurring weekly and monthly plans; plan definitions (cadence, price, Stripe price ID) are stored in a settings table so offerings can be changed without code deployments.
- The chatbot will call the OpenAI API to generate stories and responses; the app will provide both pre-generated stories and allow users to generate their own.
- For cost and privacy, store only the user's most-recently-generated story for now; design the DB schema to be flexible to store multiple stories later.
- Payments will be handled through Stripe; integrate webhook handling for subscription lifecycle (create/cancel/payment failures) and reconcile with user records.
- Admin panel: provide an admin interface where admin users can view users, subscription status, payments, and basic usage metrics.

Data model notes
- Store minimal user-generated content initially: `Users` table, `LatestUserStory` column or `UserStories` table with a flag for `IsLatest`.
- Keep schema extensible (timestamps, story metadata, token costs, content moderation flags).
 - Add a `BillingPlans` (or `Settings`) table to model available plans and pricing: fields like `PlanId`, `Name`, `Cadence` (weekly|monthly), `StripePriceId`, `Price`, `Currency`, `TrialDays`, `IsActive`.
 - PaymentService maps users to `BillingPlans` and stores subscription metadata (Stripe customer ID, Stripe subscription ID, current status, period_end).

Security & compliance notes
- Ensure payment and PII data are handled securely; do not store raw payment card data (use Stripe tokens).
- Implement rate-limiting, content moderation (OpenAI safety), and logging of critical events for audits.

Secrets & configuration
- All secrets and connection strings must be provided via environment variables in runtime.
- Development: services may load values from `appsettings.Development.json` for convenience, but developers should prefer environment variables or a local `.env` file and never commit secrets to git.
- Production: do NOT use file-based secrets. Production services must read secrets (DB connection strings, API keys, Stripe secrets, OpenAI keys, etc.) from AWS Secrets Manager or SSM Parameter Store using IAM roles assigned to ECS tasks.
- ECS tasks should use IAM task roles with least privilege to retrieve secrets; avoid embedding long-lived credentials in images or code.
- Rotate secrets regularly and ensure that secret values are never logged or exposed in error messages.

Validation & testing (enterprise level)
- Apply rigorous validation at every layer:
	- API layer: request schema validation (OpenAPI), input sanitization, size limits, and authentication/authorization checks.
	- Application/service layer: enforce business rules, rate limits, and token accounting before external API calls.
	- Domain layer: enforce invariants and strong typing/value objects for critical data (emails, roles, IDs).
- Testing requirements:
	- Unit tests for domain logic and validation rules.
	- Integration tests for service interactions (DB, external APIs, message queues).
	- End-to-end tests covering common user flows (signup, subscription, generate story, payment failure handling).
	- Contract tests to ensure frontend-backend and service-service contracts remain stable.
	- Security and fuzz testing for input validation, plus dependency vulnerability scanning.
- All layers should have automated validation and tests run in CI; failures must block merges.

Usage-control & billing policy
- Enforce per-user usage controls to prevent abuse: apply per-user story limits and token limits (e.g., stories/day, tokens/month).
- Store limit settings in a config table (admin-updatable) or environment-backed feature flags so limits can be tuned without a deployment.
- Enforcement: `StoryService` should validate limits before calling OpenAI; track usage counters in DB (or a fast cache with periodic persistence) and reject requests that exceed limits.
- Billing enforcement: if a user's subscription is expired or in a delinquent state, block story-generation endpoints and redirect the frontend to the payment/checkout flow.
- Recurring billing: subscriptions are monthly and managed through Stripe Subscriptions; handle webhook events (`invoice.paid`, `invoice.payment_failed`, `customer.subscription.deleted`) to update application state.
- Admin override: admin users should be able to temporarily whitelist or adjust limits for specific users via the `AdminService` UI/API.
- Monitoring & alerts: generate alerts for unusual spikes in token usage to detect compromised accounts or abuse.

Operational enforcement details
- Use a short-lived cache (Redis or in-memory with distributed coordination) to keep per-user counters for performance; persist authoritative counters to Postgres regularly.
- For critical enforcement (blocking on expired payment), read authoritative subscription state from `PaymentService` backed store (cached for performance with short TTL).
- Ensure webhooks are idempotent and securely verified (use `STRIPE_WEBHOOK_SECRET`) so subscription state remains consistent.


File created by automated assistant.
