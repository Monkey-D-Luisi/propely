# Propely - Agent Instructions (AGENTS.md)

> This file provides project context for AI coding assistants (GitHub Copilot Workspace, Codex, etc.)

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
# Build
dotnet build services/<service>/Propely.<Service>.sln
cd apps/web && npm run build

# Test
dotnet test services/<service>/Propely.<Service>.sln
cd apps/web && npm test

# Dev environment
.\scripts\dev-up.ps1           # Windows (Docker Compose)
./scripts/dev-up.sh            # Linux/macOS
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET naming**: PascalCase for public members, camelCase for locals
- **TypeScript naming**: camelCase for variables/functions, PascalCase for components/types
- **Architecture**: Domain layer has zero framework dependencies. Dependencies flow inward only.
- **English only** in all repository content
- **No secrets** in repo — use `.env` + `.env.example`

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

## Database

Single Postgres instance, separate databases per service:
- `propely_aiapi`, `propely_orgsapi`, `propely_propertiesapi`
- `propely_publishingapi`, `propely_contactsapi`, `propely_appointmentsapi`
