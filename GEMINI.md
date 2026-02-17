# SaaS Starter Kit - Gemini Instructions

## Priority
1. `.agent.md` (governance)
2. Current task in `docs/tasks/`
3. `docs/backlog/` for next work

## Project Overview

Monorepo with three independent services orchestrated by Docker Compose:
- `apps/web/` - Next.js 16 + Tailwind CSS frontend (port 3000)
- `services/ai-api/` - .NET 10 microservice for AI capabilities (port 5010)
- `services/orgs-api/` - .NET 10 microservice for organizations, auth, teams (port 5020)

## Commands

| Command | Action |
|---------|--------|
| `next task` | Read `.agent/rules/autonomous-workflow.md`, execute |
| `code review` | Read `.agent/rules/code-review-workflow.md`, execute |
| `fast track: <X>` | Read `.agent/rules/fast-track-workflow.md`, execute |
| `next audit action` | Read `.agent/rules/audit-action-workflow.md`, execute |
| `pr` | Read `.agent/rules/pr-workflow.md`, execute |

## Architecture

Both .NET services follow Clean Architecture + CQRS (MediatR):
```
src/
  *.Domain/          Pure business logic, entities, events
  *.Application/     Use cases, commands, queries, interfaces
  *.Infrastructure/  EF Core, RabbitMQ, Redis, external services
  *.Api/             Controllers, middleware, configuration
```

## Key Commands

```bash
# Full stack (infra + all app services with hot reload) [default]
./scripts/dev-up.sh            # Linux/macOS
.\scripts\dev-up.ps1           # Windows

# Infrastructure only
./scripts/dev-up.sh --infra-only   # Linux/macOS
.\scripts\dev-up.ps1 -InfraOnly    # Windows

# Full stack (explicit, backward-compatible)
./scripts/dev-up.sh --apps     # Linux/macOS
.\scripts\dev-up.ps1 -Apps     # Windows

# Stop / Reset
./scripts/dev-down.sh           # Stop all containers
.\scripts\dev-down.ps1
.\scripts\dev-reset.ps1         # Stop + destroy all data volumes

# Run services individually (host-based, without Docker)
./scripts/run-ai-api.sh        # AI API on port 5010
.\scripts\run-ai-api.ps1
./scripts/run-orgs-api.sh      # Orgs API on port 5020
.\scripts\run-orgs-api.ps1
./scripts/run-web.sh           # Web on port 3000
.\scripts\run-web.ps1

# Build
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build

# Test
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm test

# Scaffold new module
cd services/ai-api && .\scripts\scaffold-module.ps1 -ModuleName <Name>
cd services/orgs-api && .\scripts\scaffold-module.ps1 -ModuleName <Name>

# EF Core migrations
dotnet ef migrations add <Name> --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
dotnet ef database update --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET naming**: PascalCase for public members, camelCase for locals
- **TypeScript naming**: camelCase for variables/functions, PascalCase for components/types
- **Architecture**: Domain layer has zero framework dependencies. Dependencies flow inward only.

## Rules (always)
- English only in repo
- Every task needs matching walkthrough (same filename)
- No secrets in repo
- Prefer file changes over chat explanations
- If conflict: correctness > security > simplicity > consistency
- When creating a PR, always use the GitHub PR template at `.github/PULL_REQUEST_TEMPLATE.md` and fill all sections.

## Port Allocation

| Service | Port |
|---------|------|
| web | 3000 |
| ai-api | 5010 |
| orgs-api | 5020 |
| PostgreSQL | 5432 |
| RabbitMQ | 5672 / 15672 |
| Redis | 6379 |
| Aspire | 18888 / 4317 |
| Mailhog | 10025 / 18025 |

## Database

Single Postgres instance, separate databases:
- `saastemplate_aiapi` - AI API data
- `saastemplate_orgsapi` - Orgs API data

## Environment Variables

Root `.env` contains all config. Service-prefixed vars use `__` as section separator (e.g., `AIAPI_RabbitMQ__Host`). .NET's `AddEnvironmentVariables("AIAPI_")` strips the prefix and maps `__` to `:` automatically.
- `AIAPI_*` vars are stripped by `builder.Configuration.AddEnvironmentVariables("AIAPI_")` in ai-api `Program.cs`
- `ORGSAPI_*` vars are stripped by `builder.Configuration.AddEnvironmentVariables("ORGSAPI_")` in orgs-api `Program.cs`
- `NEXT_PUBLIC_*` vars are used directly by Next.js
- Run scripts load `.env` into process env; .NET handles the rest (no manual mapping needed)
