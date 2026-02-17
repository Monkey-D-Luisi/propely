# CLAUDE.md - Agent Instructions for SaaS Starter Kit

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
| `audit epic <NNN>` | Read `.agent/rules/audit-epic-workflow.md`, execute |
| `next audit action` | Read `.agent/rules/audit-action-workflow.md`, execute (auto-selects epic) |
| `next audit action <NNN>` | Same, but targets a specific epic's audit |
| `batch audit actions <NNN>` | Read `.agent/rules/batch-audit-actions-workflow.md`, execute |
| `audit services` | Read `.agent/rules/audit-services-workflow.md`, execute |
| `fix service audits` | Read `.agent/rules/fix-service-audits-workflow.md`, execute |
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
./scripts/dev-reset.sh          # Stop + destroy all data volumes
.\scripts\dev-reset.ps1

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

## Frontend Design System (mandatory for all UI work)

### Tokens
Design tokens are defined in `apps/web/src/app/globals.css` via Tailwind v4 `@theme`. Use semantic `primary-*` classes — never hardcode `indigo-*` for brand/action colors.

### Color usage
| Purpose | Classes |
|---------|---------|
| Primary button | `bg-primary-600 hover:bg-primary-600/90 text-white shadow-sm active:scale-[0.98]` |
| Secondary button | `border-slate-200 bg-white text-slate-700 hover:bg-slate-50` |
| Focus ring (buttons) | `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2` |
| Focus ring (inputs) | `focus:ring-2 focus:ring-primary-600 focus:border-transparent` |
| Links (auth context) | `text-primary-700 underline hover:text-primary-800` |
| Page background | `bg-surface` (#f6f6f8) — semantic token, do NOT use `bg-slate-50` |
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

### Stitch MCP (UI design tool — pixel-perfect mandate)
- **Stitch Project ID**: `16786124142182555397`
- **Always use `modelId: GEMINI_3_PRO`** when calling `generate_screen_from_text` or `edit_screens`
- Every new screen MUST have a corresponding Stitch design in the project
- **Pixel-perfect implementation is mandatory.** The Stitch design is the single source of truth for all UI work. Implementation must match the Stitch HTML output exactly — same spacing, colors, typography, layout, and component structure.
- **Workflow for UI tasks:**
  1. Generate (or retrieve existing) the Stitch screen design
  2. Download the Stitch HTML to `.stitch-html/<screen-name>.html` for reference
  3. Extract exact CSS classes, spacing, colors, and typography from the Stitch HTML
  4. Implement the page/component matching the Stitch design pixel-for-pixel
  5. Verify visually (Puppeteer screenshot or manual check) against the Stitch design
- If a Stitch design doesn't exist yet for a screen, generate one before writing any UI code.
- Reference: `docs/walkthroughs/0065-design-system.md`

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
| Aspire | 18888 / 4317 / 4318 |
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
