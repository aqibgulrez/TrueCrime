TrueCrime — developer README

Developer setup

1) Prerequisites
- Docker & Docker Compose
- .NET SDK (matching project target)
- Node.js (for frontend)

2) Local Postgres data directory
- Create a host folder for Postgres data (example):

```powershell
mkdir D:\Projects\TrueCrime\pgdata
```

- This folder will be mounted into the Postgres Docker container so database files live outside the repository.
- DO NOT commit `pgdata` to git. A `.gitignore` entry already prevents this.

3) Bootstrap the database (two options)

- Option A — import the SQL template (recommended):

```powershell
# Start Postgres container (example)
docker run --name truecrime-postgres -e POSTGRES_PASSWORD=postgres -v D:\Projects\TrueCrime\pgdata:/var/lib/postgresql/data -p 5432:5432 -d postgres:15

# Import template SQL into the running container
docker exec -i truecrime-postgres psql -U postgres -f /tmp/dev_template.sql
```

- Option B — run migrations:

```powershell
# Use EF Core migrations from the API project
dotnet ef database update --project backend\services\UserService\UserService.Infrastructure\UserService.Infrastructure.csproj --startup-project backend\services\UserService\UserService.API\UserService.API.csproj
```

4) dev_template.sql
- A minimal SQL template `dev_template.sql` is included in the repo. Developers can copy it into the Postgres container or import it as shown above. It is intentionally small and sanitized.

5) Environment variables
- For local development, create a `.env` or `appsettings.Development.json` with connection strings and keys (do not commit secrets). Example env keys: `DATABASE_URL`, `OPENAI_API_KEY`, `STRIPE_API_KEY`.

6) Start services
- Use Docker Compose (if provided) or run each service locally. Example:

```powershell
# from repo root
docker compose up --build
```

If you want, I can add a `docker-compose.yml` and a startup script next.