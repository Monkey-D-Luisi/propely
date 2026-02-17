# Propely - Gemini Instructions

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

### NuGet SDK Pattern (inter-service communication)
- Services never call each other directly from the frontend
- Internal services expose `Propely.<Service>.Client` NuGet packages
- Clients use **Refit** (declarative interface-based HTTP clients) + `IHttpClientFactory`
- Tenant context (`X-Tenant-Id`) propagated automatically via `TenantDelegatingHandler`

## Key Commands

```bash
# Full stack (infra + all app services with hot reload)
.\scripts\dev-up.ps1           # Windows
./scripts/dev-up.sh            # Linux/macOS

# Infrastructure only
.\scripts\dev-up.ps1 -InfraOnly

# Stop / Reset
.\scripts\dev-down.ps1
.\scripts\dev-reset.ps1         # Stop + destroy all data volumes

# Run services individually
.\scripts\run-ai-api.ps1        # port 5010
.\scripts\run-orgs-api.ps1      # port 5020
.\scripts\run-properties-api.ps1 # port 5030
.\scripts\run-publishing-api.ps1 # port 5040
.\scripts\run-contacts-api.ps1   # port 5050
.\scripts\run-appointments-api.ps1 # port 5060
.\scripts\run-web.ps1           # port 3000

# Build
dotnet build services/ai-api/Propely.AiApi.sln
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln
cd apps/web && npm run build

# Test
dotnet test services/ai-api/Propely.AiApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
cd apps/web && npm test

# EF Core migrations (example for properties-api)
dotnet ef migrations add <Name> --project services/properties-api/src/Propely.PropertiesApi.Infrastructure --startup-project services/properties-api/src/Propely.PropertiesApi.Api
dotnet ef database update --project services/properties-api/src/Propely.PropertiesApi.Infrastructure --startup-project services/properties-api/src/Propely.PropertiesApi.Api
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET naming**: PascalCase for public members, camelCase for locals
- **TypeScript naming**: camelCase for variables/functions, PascalCase for components/types
- **Architecture**: Domain layer has zero framework dependencies. Dependencies flow inward only.

## Frontend Design System (mandatory for all UI work)

### Stitch MCP (UI design tool — pixel-perfect mandate)
- **Always use `modelId: GEMINI_3_PRO`** when generating screens
- Every new screen MUST have a corresponding Stitch design
- **Pixel-perfect implementation is mandatory.** The Stitch design is the single source of truth.
- **Workflow:**
  1. Generate Stitch screen design
  2. Download HTML to `.stitch-html/<screen-name>.html`
  3. Extract exact CSS, spacing, colors, typography
  4. Implement matching pixel-for-pixel
  5. Verify visually

### Key Libraries
- **Forms**: JSON Forms (`@jsonforms/react` + `@jsonforms/material-renderers`) — JSON Schema-driven, i18n via `translate`
- **Calendar**: FullCalendar Standard (MIT) — month/week/day views, drag-and-drop

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
- `propely_aiapi` - AI API data
- `propely_orgsapi` - Orgs API data
- `propely_propertiesapi` - Properties data
- `propely_publishingapi` - Publication data
- `propely_contactsapi` - Contacts/Leads data
- `propely_appointmentsapi` - Appointments data

## Environment Variables

Root `.env` contains all config. Service-prefixed vars use `__` as section separator (e.g., `AIAPI_RabbitMQ__Host`). .NET's `AddEnvironmentVariables("AIAPI_")` strips the prefix and maps `__` to `:` automatically.
- `AIAPI_*` → ai-api
- `ORGSAPI_*` → orgs-api
- `PROPERTIESAPI_*` → properties-api
- `PUBLISHINGAPI_*` → publishing-api
- `CONTACTSAPI_*` → contacts-api
- `APPOINTMENTSAPI_*` → appointments-api
- `NEXT_PUBLIC_*` → Next.js frontend
- Run scripts load `.env` into process env; .NET handles the rest (no manual mapping needed)
