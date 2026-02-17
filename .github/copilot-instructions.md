# Propely - GitHub Copilot Instructions

## Priority
1. `.agent.md` (governance)
2. Current task in `docs/tasks/`
3. `docs/roadmap.md` for execution order

## Project Overview

**Propely** is a multi-tenant, AI-powered real estate management SaaS. Monorepo with microservices orchestrated by Docker Compose.

## Services

| Service | Port | Purpose |
|---------|------|---------|
| `apps/web` | 3000 | Next.js 16 + Tailwind CSS frontend |
| `services/ai-api` | 5010 | Internal AI utility (NuGet SDK only) |
| `services/orgs-api` | 5020 | Auth, tenancy, teams |
| `services/properties-api` | 5030 | Property management core domain |
| `services/publishing-api` | 5040 | Portal publication + background worker |
| `services/contacts-api` | 5050 | Leads/contacts |
| `services/appointments-api` | 5060 | Appointments + Google/Outlook calendar sync |

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
.\scripts\dev-up.ps1           # Full stack (Docker Compose)
.\scripts\dev-up.ps1 -InfraOnly  # Infrastructure only
.\scripts\dev-down.ps1         # Stop
.\scripts\dev-reset.ps1        # Stop + destroy data

# EF Core migrations
dotnet ef migrations add <Name> --project services/<service>/src/Propely.<Service>.Infrastructure --startup-project services/<service>/src/Propely.<Service>.Api
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **.NET naming**: PascalCase public members, camelCase locals
- **TypeScript**: camelCase variables/functions, PascalCase components/types
- **English only** in all repo content
- **No secrets** in repo — `.env` + `.env.example`
- Domain layer has zero framework dependencies

## Key Libraries

- **Forms**: JSON Forms (`@jsonforms/react`) — JSON Schema-driven, i18n via `translate`
- **Calendar**: FullCalendar Standard (MIT)
- **UI Design**: Google Stitch (Pro model) → pixel-perfect implementation

## Frontend Design System

### Stitch MCP (pixel-perfect mandate)
- **Always use `modelId: GEMINI_3_PRO`**
- Every new screen MUST have a Stitch design first
- Workflow: Generate Stitch → save HTML to `.stitch-html/` → extract styles → implement pixel-for-pixel

## Database

Single Postgres instance, separate databases per service:
- `propely_aiapi`, `propely_orgsapi`, `propely_propertiesapi`
- `propely_publishingapi`, `propely_contactsapi`, `propely_appointmentsapi`

## Clean Architecture Layer Order
1. **Domain** — Entities, value objects, domain events
2. **Application** — Commands, queries, handlers, interfaces
3. **Infrastructure** — Repositories, external services
4. **Presentation** — Controllers, DTOs, validation

## Quality Gates
```bash
dotnet build services/<service>/Propely.<Service>.sln  # Must pass
dotnet test services/<service>/Propely.<Service>.sln   # Must pass
cd apps/web && npm run build                           # Must pass
cd apps/web && npm test                                # Must pass
```
