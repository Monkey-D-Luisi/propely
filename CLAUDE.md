# Propely - Agent Instructions (CLAUDE.md)

## Priority
1. `.agent.md` (governance)
2. Current task in `docs/tasks/`
3. `docs/roadmap.md` for execution order, then `docs/backlog/` for details

## Project Overview

Propely is a multi-tenant, AI-powered real estate management SaaS. The platform's core differentiator is **natural language process automation**: agents interact with the system through text and voice commands to create properties, manage leads, schedule appointments, close operations, and perform any system action. The repository is a monorepo with 7 services orchestrated by Docker Compose:

- `apps/web/` - Next.js 16 + Tailwind CSS frontend (port 3000)
- `services/ai-api/` - .NET 10 AI API -- AI action engine, NL intent classification, voice transcription (gpt-4o-mini-transcription), content generation (port 5010)
- `services/orgs-api/` - .NET 10 Orgs API -- auth, agencies, branches, billing, permissions (port 5020)
- `services/properties-api/` - .NET 10 Properties API -- core property domain (port 5030)
- `services/publishing-api/` - .NET 10 Publishing API -- **deprioritized**, scaffolded only (port 5040)
- `services/contacts-api/` - .NET 10 Contacts API -- contacts, leads (port 5050)
- `services/appointments-api/` - .NET 10 Appointments API -- scheduling, calendar sync (port 5060)

## Commands

| Command | Action |
|---------|--------|
| `next task` | Read `.agent/rules/autonomous-workflow.md`, execute |
| `code review` | Read `.agent/rules/code-review-workflow.md`, execute |
| `fast track: <X>` | Read `.agent/rules/fast-track-workflow.md`, execute |
| `pr` | Read `.agent/rules/pr-workflow.md`, execute |

## Architecture

All 6 .NET services follow Clean Architecture + CQRS (MediatR):
```
src/
  Propely.<Service>.Domain/          Pure business logic, entities, events
  Propely.<Service>.Application/     Use cases, commands, queries, interfaces
  Propely.<Service>.Infrastructure/  EF Core, RabbitMQ, Redis, external services
  Propely.<Service>.Api/             Controllers, middleware, configuration
  Propely.<Service>.Client/          NuGet SDK client (Refit) for inter-service communication
tests/
  Propely.<Service>.UnitTests/
  Propely.<Service>.IntegrationTests/
```

Namespace convention: `Propely.<Service>.<Layer>` (e.g., `Propely.PropertiesApi.Domain`).

### NuGet SDK Client Pattern
Inter-service communication uses Refit NuGet SDK clients:
- Each service publishes a `Propely.<Service>.Client` project
- Client contains: Refit interface, request/response DTOs, `IServiceCollection` extension
- Tenant context propagated via `TenantDelegatingHandler` (adds `X-Tenant-Id` header)
- Resilience via Polly (retry + circuit breaker)

| SDK | Published by | Consumed by |
|-----|-------------|-------------|
| `Propely.OrgsApi.Client` | orgs-api | All backend services |
| `Propely.AiApi.Client` | ai-api | web (via HTTP), properties-api |
| `Propely.PropertiesApi.Client` | properties-api | ai-api, contacts-api, appointments-api |
| `Propely.ContactsApi.Client` | contacts-api | ai-api, appointments-api |
| `Propely.AppointmentsApi.Client` | appointments-api | ai-api |

### AI Action Engine (core differentiator)
The `ai-api` service acts as the AI orchestration layer. Architecture:
1. **Input** — Text (`POST /v1/actions/execute`) or voice audio (`POST /v1/voice/execute`)
2. **Voice STT** — `gpt-4o-mini-transcription` via OpenAI Audio API (if voice)
3. **Intent classification** — OpenAI function calling with tool definitions for each action type
4. **Action routing** — `IActionHandler<T>` pattern (MediatR) dispatches to typed handlers
5. **Execution** — Handlers call other service SDK clients (PropertiesApi, ContactsApi, AppointmentsApi)
6. **Response** — Structured result + NL confirmation text

Key: `ai-api` **consumes** other service SDK clients to execute actions on behalf of the user.

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
./scripts/dev-reset.sh          # Stop + destroy all data volumes
.\scripts\dev-reset.ps1

# Run services individually (host-based, without Docker)
./scripts/run-ai-api.sh        # AI API on port 5010
.\scripts\run-ai-api.ps1
./scripts/run-orgs-api.sh      # Orgs API on port 5020
.\scripts\run-orgs-api.ps1
./scripts/run-properties-api.sh    # Properties API on port 5030
.\scripts\run-properties-api.ps1
./scripts/run-publishing-api.sh    # Publishing API on port 5040
.\scripts\run-publishing-api.ps1
./scripts/run-contacts-api.sh      # Contacts API on port 5050
.\scripts\run-contacts-api.ps1
./scripts/run-appointments-api.sh  # Appointments API on port 5060
.\scripts\run-appointments-api.ps1
./scripts/run-web.sh           # Web on port 3000
.\scripts\run-web.ps1

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
dotnet test services/publishing-api/Propely.PublishingApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
dotnet test services/appointments-api/Propely.AppointmentsApi.sln
cd apps/web && npm test

# Scaffold new module (any .NET service)
cd services/<service> && .\scripts\scaffold-module.ps1 -ModuleName <Name>

# EF Core migrations (pattern for any service)
dotnet ef migrations add <Name> \
  --project services/<service>/src/Propely.<Service>.Infrastructure \
  --startup-project services/<service>/src/Propely.<Service>.Api
dotnet ef database update \
  --project services/<service>/src/Propely.<Service>.Infrastructure \
  --startup-project services/<service>/src/Propely.<Service>.Api
```

## Conventions

- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET naming**: PascalCase for public members, camelCase for locals
- **TypeScript naming**: camelCase for variables/functions, PascalCase for components/types
- **Architecture**: Domain layer has zero framework dependencies. Dependencies flow inward only.

## Frontend Design System (mandatory for all UI work)

### Tokens
Design tokens are defined in `apps/web/src/app/globals.css` via Tailwind v4 `@theme`. Use semantic `primary-*` classes -- never hardcode color values for brand/action colors.

### Key Libraries
| Library | Purpose |
|---------|---------|
| JSON Forms (`@jsonforms/react`) | Schema-driven property forms |
| FullCalendar Standard | Calendar views for appointments |
| cmdk | Command bar / command palette for NL input |
| Stitch MCP | Design generation (Project ID: `16786124142182555397`, model: `GEMINI_3_PRO`) |

### Color usage
| Purpose | Classes |
|---------|---------|
| Primary button | `bg-primary-600 hover:bg-primary-600/90 text-white shadow-sm active:scale-[0.98]` |
| Secondary button | `border-slate-200 bg-white text-slate-700 hover:bg-slate-50` |
| Focus ring (buttons) | `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2` |
| Focus ring (inputs) | `focus:ring-2 focus:ring-primary-600 focus:border-transparent` |
| Links (auth context) | `text-primary-700 underline hover:text-primary-800` |
| Page background | `bg-surface` (#f6f6f8) -- semantic token, do NOT use `bg-slate-50` |
| Neutral text | `text-slate-900` / `text-slate-600` / `text-slate-500` (keep slate) |
| Card borders | `border-slate-200` (keep slate) |
| Input borders | `border-slate-200` (keep slate) |

### Border radius
- Cards/containers: `rounded-xl`
- Inputs/buttons: `rounded-lg`
- Badges/pills: `rounded-full`

### Layout widths
- Auth pages: `max-w-md`
- Dashboard/org list: `max-w-4xl`
- Detail pages (members, settings, billing, profile, admin): `max-w-5xl`

### Stitch MCP (UI design tool -- pixel-perfect mandate)
- **Stitch Project ID**: `16786124142182555397`
- **Always use `modelId: GEMINI_3_PRO`** when calling `generate_screen_from_text` or `edit_screens`
- Every new screen MUST have a corresponding Stitch design in the project
- **Pixel-perfect implementation is mandatory.** The Stitch design is the single source of truth for all UI work. Implementation must match the Stitch HTML output exactly -- same spacing, colors, typography, layout, and component structure.
- **Workflow for UI tasks:**
  1. Define use cases exhaustively (actor, preconditions, flows, postconditions)
  2. Generate (or retrieve existing) the Stitch screen design
  3. Download the Stitch HTML to `.stitch-html/<screen-name>.html` for reference
  4. Extract exact CSS classes, spacing, colors, and typography from the Stitch HTML
  5. Implement with TDD (Red-Green-Refactor) matching the Stitch design pixel-for-pixel
  6. Verify visually (Puppeteer screenshot or manual check) against the Stitch design
- If a Stitch design doesn't exist yet for a screen, generate one before writing any UI code.

## Rules (always)
- English only in repo
- Every task needs matching walkthrough (same filename)
- No secrets in repo
- Prefer file changes over chat explanations
- If conflict: correctness > security > simplicity > consistency
- **TDD is mandatory** for all backend and frontend work (see `.agent.md` sections 3.2 and 3.3)
- **Use cases before UI**: define use cases exhaustively before generating Stitch designs or writing code
- When creating a PR, always use the GitHub PR template at `.github/PULL_REQUEST_TEMPLATE.md` and fill all sections.

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
| Aspire | 18888 / 4317 / 4318 |
| Mailhog | 10025 / 18025 |

## Database

Single Postgres instance, separate databases per service:
- `propely_aiapi` -- AI API data
- `propely_orgsapi` -- Orgs API data
- `propely_propertiesapi` -- Properties API data
- `propely_contactsapi` -- Contacts API data
- `propely_appointmentsapi` -- Appointments API data
- `propely_publishingapi` -- Publishing API data

## Environment Variables

Root `.env` contains all config. Service-prefixed vars use `__` as section separator (e.g., `AIAPI_RabbitMQ__Host`). .NET's `AddEnvironmentVariables("PREFIX_")` strips the prefix and maps `__` to `:` automatically.

- `AIAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("AIAPI_")` in ai-api `Program.cs`
- `ORGSAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("ORGSAPI_")` in orgs-api `Program.cs`
- `PROPERTIESAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("PROPERTIESAPI_")` in properties-api `Program.cs`
- `CONTACTSAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("CONTACTSAPI_")` in contacts-api `Program.cs`
- `APPOINTMENTSAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("APPOINTMENTSAPI_")` in appointments-api `Program.cs`
- `PUBLISHINGAPI_*` -- stripped by `builder.Configuration.AddEnvironmentVariables("PUBLISHINGAPI_")` in publishing-api `Program.cs`
- `NEXT_PUBLIC_*` -- used directly by Next.js
- Run scripts load `.env` into process env; .NET handles the rest (no manual mapping needed)
