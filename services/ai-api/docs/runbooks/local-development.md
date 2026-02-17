# Local Development Runbook

> **Note:** The ai-api service lives inside the `saas-template` monorepo at `services/ai-api/`. All infrastructure is managed from the monorepo root.

## Prerequisites

### Required
- Docker Desktop (or Docker Engine + Compose)
- .NET 10 SDK

### Recommended
- IDE: VS Code, Rider, or Visual Studio
- Database GUI: pgAdmin or DBeaver
- API testing: Postman, Bruno, or VS Code REST Client

## Quick Start

```bash
git clone https://github.com/Monkey-D-Luisi/saas-template.git
cd saas-template
cp .env.example .env          # defaults work out-of-the-box
./scripts/dev-up.sh           # start full stack (infra + all services)
```

API available at `http://localhost:5010`

## Scripts

All scripts are at the **monorepo root** (`saas-template/scripts/`):

| Script | Description |
|--------|-------------|
| `./scripts/dev-up.sh` / `.\scripts\dev-up.ps1` | Start full stack (PostgreSQL, RabbitMQ, Redis, Aspire, Mailhog + all services) |
| `./scripts/dev-up.sh --infra-only` | Start infrastructure only (no app services) |
| `./scripts/dev-down.sh` / `.\scripts\dev-down.ps1` | Stop all containers (preserves data) |
| `./scripts/dev-reset.sh` / `.\scripts\dev-reset.ps1` | Stop and remove all data volumes |
| `./scripts/run-ai-api.sh` / `.\scripts\run-ai-api.ps1` | Run ai-api only (loads `.env` automatically) |

The ai-api service also has its own scripts in `services/ai-api/scripts/`:

| Script | Description |
|--------|-------------|
| `scaffold-module.ps1 -ModuleName <Name>` | Scaffold a new Clean Architecture module |

## Infrastructure Services

| Service | Host | Port | Credentials |
|---------|------|------|-------------|
| PostgreSQL | localhost | 5432 | saastemplate / saastemplate_dev_password |
| RabbitMQ | localhost | 5672, 15672 (UI) | saastemplate / saastemplate_dev_password |
| Redis | localhost | 6379 | password: saastemplate_dev_password |
| Aspire Dashboard | localhost | 18888 | — |
| Mailhog | localhost | 18025 (UI) | — |

**Connection Strings** (from root `.env`, `AIAPI_` prefix stripped at startup):
```
AIAPI_ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=saastemplate_aiapi;Username=saastemplate;Password=saastemplate_dev_password
AIAPI_RabbitMQ__Host=localhost
AIAPI_RabbitMQ__Port=5672
AIAPI_RabbitMQ__Username=saastemplate
AIAPI_RabbitMQ__Password=saastemplate_dev_password
AIAPI_RabbitMQ__VirtualHost=/
AIAPI_Redis__ConnectionString=localhost:6379,password=saastemplate_dev_password
```

## Environment Variables

The root `.env.example` contains all configuration. Copy it to `.env`:

```bash
cp .env.example .env
```

The `run-ai-api.sh` script loads `.env` automatically. .NET's `AddEnvironmentVariables("AIAPI_")` strips the `AIAPI_` prefix and maps `__` to `:` for config binding.

> Never commit `.env` to version control—it's in `.gitignore`.

## Database Migrations

```bash
# Apply migrations
dotnet ef database update \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api

# Create new migration
dotnet ef migrations add <MigrationName> \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api

# View migration SQL (without applying)
dotnet ef migrations script \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
```

## API Testing

### Using cURL

```bash
# Create a work item (requires auth token in production; dev mode auto-authenticates)
curl -X POST http://localhost:5010/v1/work-items \
  -H "Content-Type: application/json" \
  -d '{"title":"My Task","description":"Task description"}'

# Get a work item
curl http://localhost:5010/v1/work-items/{id}
```

### Response Examples

**POST /v1/work-items (201 Created):**
```json
{
  "id": "e6a404e0-4dc9-4949-9130-e9ca9ecdc439",
  "title": "My Task",
  "description": "Task description",
  "status": "Pending",
  "createdAtUtc": "2026-01-28T22:09:31.289Z",
  "updatedAtUtc": "2026-01-28T22:09:31.289Z"
}
```

## Troubleshooting

### "Database connection string not configured"

Use `./scripts/run-ai-api.sh` which loads `.env` automatically. Or ensure `AIAPI_ConnectionStrings__DefaultConnection` is set.

### "relation does not exist"

Apply database migrations (see Database Migrations section above).

### Port already in use

```bash
# Find process using port (Linux/macOS)
lsof -i :5010

# Find process using port (Windows)
netstat -ano | findstr :5010
```

### Containers won't start

```bash
# Check logs
docker compose logs postgres
docker compose logs rabbitmq
docker compose logs redis

# Reset and restart (destructive)
./scripts/dev-reset.sh
./scripts/dev-up.sh
```

### Database connection failed

1. Verify PostgreSQL is running: `docker ps | grep saastemplate-postgres`
2. Check connection string matches `.env`
3. Check PostgreSQL logs: `docker compose logs postgres`

### Tests fail

Tests use Testcontainers and manage their own database. Ensure Docker is running:

```bash
docker ps                                              # Verify Docker is running
dotnet test services/ai-api/SaasTemplate.AiApi.sln     # Run from monorepo root
```
