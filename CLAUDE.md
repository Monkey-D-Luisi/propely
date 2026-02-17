# CLAUDE.md - Propely Agent Instructions

## Priority
1. `.agent.md` (governance)
2. Current task in `docs/tasks/`
3. `docs/roadmap.md` for execution order

## Project Overview

**Propely** is a multi-tenant, AI-powered real estate management SaaS. Monorepo with microservices orchestrated by Docker Compose:

- `apps/web/` - Next.js 16 + Tailwind CSS frontend (port 3000)
- `services/ai-api/` - .NET 10 internal AI utility (port 5010) — consumed via NuGet SDK only
- `services/orgs-api/` - .NET 10 auth, tenancy, teams (port 5020)
- `services/properties-api/` - .NET 10 property management core (port 5030)
- `services/publishing-api/` - .NET 10 portal publication + worker (port 5040)
- `services/contacts-api/` - .NET 10 leads/contacts (port 5050)
- `services/appointments-api/` - .NET 10 appointments + calendar sync (port 5060)

## Commands

| Command | Action |
|---------|--------|
| `next task` | Read `.agent/rules/autonomous-workflow.md`, execute |
| `code review` | Read `.agent/rules/code-review-workflow.md`, execute |
| `fast track: <X>` | Read `.agent/rules/fast-track-workflow.md`, execute |
| `pr` | Read `.agent/rules/pr-workflow.md`, execute |

## Architecture

All .NET services follow Clean Architecture + CQRS (MediatR):
```
src/
  Propely.<Service>.Domain/          Pure business logic, entities, events
  Propely.<Service>.Application/     Use cases, commands, queries, interfaces
  Propely.<Service>.Infrastructure/  EF Core, RabbitMQ, Redis, external services
  Propely.<Service>.Api/             Controllers, middleware, configuration
  Propely.<Service>.Client/          NuGet SDK (Refit interface, DTOs, DI extension)
```

### NuGet SDK Pattern
- Internal services expose `Propely.<Service>.Client` NuGet packages
- Clients use **Refit** + `IHttpClientFactory` with resilience (Polly)
- Tenant context (`X-Tenant-Id`) propagated via `TenantDelegatingHandler`

## Key Commands

```bash
# Full stack
.\scripts\dev-up.ps1           # Windows
./scripts/dev-up.sh            # Linux/macOS

# Infrastructure only
.\scripts\dev-up.ps1 -InfraOnly

# Stop / Reset
.\scripts\dev-down.ps1
.\scripts\dev-reset.ps1

# Build
dotnet build services/ai-api/Propely.AiApi.sln
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln
cd apps/web && npm run build

# Test
dotnet test services/<service>/Propely.<Service>.sln
cd apps/web && npm test

# EF Core migrations
dotnet ef migrations add <Name> --project services/<service>/src/Propely.<Service>.Infrastructure --startup-project services/<service>/src/Propely.<Service>.Api
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET naming**: PascalCase for public members, camelCase for locals
- **TypeScript naming**: camelCase for variables/functions, PascalCase for components/types
- **Architecture**: Domain layer has zero framework dependencies. Dependencies flow inward only.

## Frontend Design System

- **Forms**: JSON Forms (`@jsonforms/react`) — JSON Schema-driven, i18n via `translate`
- **Calendar**: FullCalendar Standard (MIT)
- **UI Design**: Google Stitch (Pro model) → pixel-perfect implementation

## Rules (always)
- English only in repo
- Every task needs matching walkthrough (same filename)
- No secrets in repo
- Prefer file changes over chat explanations
- If conflict: correctness > security > simplicity > consistency

## Port Allocation

| Service | Port |
|---------|------|
| web | 3000 |
| ai-api | 5010 |
| orgs-api | 5020 |
| properties-api | 5030 |
| publishing-api | 5040 |
| contacts-api | 5050 |
| appointments-api | 5060 |
| PostgreSQL | 5432 |
| RabbitMQ | 5672 / 15672 |
| Redis | 6379 |
| Aspire | 18888 / 4317 |
| Mailhog | 10025 / 18025 |

## Database

Single Postgres instance, separate databases:
- `propely_aiapi`, `propely_orgsapi`, `propely_propertiesapi`
- `propely_publishingapi`, `propely_contactsapi`, `propely_appointmentsapi`

## Environment Variables

Root `.env` contains all config. Service-prefixed vars use `__` as section separator.
- `AIAPI_*`, `ORGSAPI_*`, `PROPERTIESAPI_*`, `PUBLISHINGAPI_*`, `CONTACTSAPI_*`, `APPOINTMENTSAPI_*`
- `NEXT_PUBLIC_*` → Next.js frontend
