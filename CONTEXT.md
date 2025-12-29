Project: TrueCrime — high-level context

I am building a true crime chatbot app.

Goal
- Enterprise-grade application hosted on AWS with testing, security, performance, and clean architecture.

Tech stack & infra
- Backend: .NET Web API (C#)
- Frontend: React (served from S3/CloudFront)
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
- The chatbot will call the OpenAI API to generate stories and responses; the app will provide both pre-generated stories and allow users to generate their own.
- For cost and privacy, store only the user's most-recently-generated story for now; design the DB schema to be flexible to store multiple stories later.
- Payments will be handled through Stripe; integrate webhook handling for subscription lifecycle (create/cancel/payment failures) and reconcile with user records.
- Admin panel: provide an admin interface where admin users can view users, subscription status, payments, and basic usage metrics.

Data model notes
- Store minimal user-generated content initially: `Users` table, `LatestUserStory` column or `UserStories` table with a flag for `IsLatest`.
- Keep schema extensible (timestamps, story metadata, token costs, content moderation flags).

Security & compliance notes
- Ensure payment and PII data are handled securely; do not store raw payment card data (use Stripe tokens).
- Implement rate-limiting, content moderation (OpenAI safety), and logging of critical events for audits.


File created by automated assistant.
