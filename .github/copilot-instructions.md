# Propely - GitHub Copilot Instructions

## Priority
1. `.agent.md` (governance) > current task in `docs/tasks/` > `docs/roadmap.md` + `docs/backlog/`

## Project Overview
Multi-tenant, AI-powered real estate management SaaS. Monorepo with 7 services:
- `apps/web/` -- Next.js 16 frontend (port 3000)
- `services/ai-api/` -- .NET 10 AI capabilities (port 5010)
- `services/orgs-api/` -- .NET 10 auth, agencies, billing, permissions (port 5020)
- `services/properties-api/` -- .NET 10 core property domain (port 5030)
- `services/publishing-api/` -- .NET 10 portal feeds, webhooks (port 5040)
- `services/contacts-api/` -- .NET 10 contacts, leads (port 5050)
- `services/appointments-api/` -- .NET 10 scheduling, calendar sync (port 5060)

## Commands
| Command | Action |
|---------|--------|
| `next task` | Read `.agent/rules/autonomous-workflow.md`, execute |
| `code review` | Read `.agent/rules/code-review-workflow.md`, execute |
| `fast track: <X>` | Read `.agent/rules/fast-track-workflow.md`, execute |
| `pr` | Read `.agent/rules/pr-workflow.md`, execute |

## Architecture
All .NET services: Clean Architecture + CQRS (MediatR). Namespace: `Propely.<Service>.<Layer>`.
```
src/  Propely.<Service>.Domain / .Application / .Infrastructure / .Api / .Client
tests/  Propely.<Service>.UnitTests / .IntegrationTests
```
Inter-service: Refit NuGet SDK clients (`Propely.<Service>.Client`), Polly resilience, `X-Tenant-Id` header.

## Build & Test
```bash
dotnet build services/ai-api/Propely.AiApi.sln
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln
cd apps/web && npm run build

dotnet test services/ai-api/Propely.AiApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
dotnet test services/publishing-api/Propely.PublishingApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
dotnet test services/appointments-api/Propely.AppointmentsApi.sln
cd apps/web && npm test
```

## Conventions
- **Commits**: `feat|fix|test|chore|docs(scope): message`
- **Branches**: `feat/<description>`, `fix/<description>`
- **.NET**: PascalCase public, camelCase locals
- **TypeScript**: camelCase vars/functions, PascalCase components/types
- **TDD mandatory**: write tests first, Red-Green-Refactor
- **Use cases before UI**: define exhaustively before design or code
- Domain layer: zero framework dependencies, dependencies flow inward only

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
Single Postgres, 6 databases: `propely_aiapi`, `propely_orgsapi`, `propely_propertiesapi`, `propely_contactsapi`, `propely_appointmentsapi`, `propely_publishingapi`.

## Rules
- English only -- no secrets in repo -- every task needs matching walkthrough
- Prefer file changes over chat -- correctness > security > simplicity > consistency
- Use GitHub PR template at `.github/PULL_REQUEST_TEMPLATE.md`
- Design tokens in `apps/web/src/app/globals.css` -- use `primary-*`, never hardcode colors
- Stitch MCP: Project `11823747923864658899`, model `GEMINI_3_PRO`, pixel-perfect mandate
