# Quick Start Guide

Get the SaaS Starter Kit running locally.

## Prerequisites

- **Docker Desktop** (or Docker Engine + Compose)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 22+** - [Download](https://nodejs.org/)

## Setup

### 1. Clone and Configure

**Linux/macOS:**
```bash
git clone https://github.com/Monkey-D-Luisi/saas-template.git
cd saas-template
cp .env.example .env
```

**Windows PowerShell:**
```powershell
git clone https://github.com/Monkey-D-Luisi/saas-template.git
cd saas-template
Copy-Item .env.example .env
```

> The defaults in `.env.example` work out-of-the-box for local development. No changes needed unless you have port conflicts.

### 2. Start Full Stack (Docker)

**Linux/macOS:**
```bash
./scripts/dev-up.sh
```

**Windows PowerShell:**
```powershell
.\scripts\dev-up.ps1
```

This starts PostgreSQL, RabbitMQ, Redis, Aspire Dashboard, and Mailhog.
It also starts `ai-api`, `orgs-api`, and `web` in Docker.

> Infrastructure-only mode is still available:
> - Linux/macOS: `./scripts/dev-up.sh --infra-only`
> - Windows: `.\scripts\dev-up.ps1 -InfraOnly`

### 3. Install Frontend Dependencies (Optional for host-based runs)

```bash
cd apps/web && npm install
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
dotnet ef database update --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
```

### 5. Optional: Run Services on Host (instead of Docker)

If you prefer running services directly on your machine (for fast local debugging), use separate terminals:

**Linux/macOS:**
```bash
./scripts/run-ai-api.sh       # Terminal 1 — http://localhost:5010
./scripts/run-orgs-api.sh     # Terminal 2 — http://localhost:5020
./scripts/run-web.sh          # Terminal 3 — http://localhost:3000
```

**Windows PowerShell:**
```powershell
.\scripts\run-ai-api.ps1      # Terminal 1 — http://localhost:5010
.\scripts\run-orgs-api.ps1    # Terminal 2 — http://localhost:5020
.\scripts\run-web.ps1         # Terminal 3 — http://localhost:3000
```

### 6. Seed Development Data (Optional)

Seeds a test organization and sends an invitation email via Mailhog:

**Linux/macOS:**
```bash
./scripts/seed-dev.sh
```

**Windows PowerShell:** *(no `.ps1` equivalent yet — use Git Bash or WSL)*

Then open [Mailhog UI](http://localhost:18025) to find the invitation email.

## Dashboards

| Dashboard | URL |
|-----------|-----|
| Web frontend | [http://localhost:3000](http://localhost:3000) |
| Aspire (traces, metrics, logs) | [http://localhost:18888](http://localhost:18888) |
| RabbitMQ Management | [http://localhost:15672](http://localhost:15672) |
| Mailhog (captured emails) | [http://localhost:18025](http://localhost:18025) |

## Daily Workflow

**Linux/macOS:**
```bash
./scripts/dev-up.sh          # Start full stack in Docker
dotnet test services/ai-api/SaasTemplate.AiApi.sln      # Tests
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm test                                   # Frontend tests
./scripts/dev-down.sh        # Stop all containers
```

**Windows PowerShell:**
```powershell
.\scripts\dev-up.ps1         # Start full stack in Docker
dotnet test services\ai-api\SaasTemplate.AiApi.sln
dotnet test services\orgs-api\SaasTemplate.OrgsApi.sln
cd apps\web; npm test                                     # Frontend tests
.\scripts\dev-down.ps1       # Stop all containers
```

## Scaffolding New Modules

Each .NET service includes a scaffold script to generate Clean Architecture vertical slices:

```powershell
cd services\ai-api
.\scripts\scaffold-module.ps1 -ModuleName Conversations

cd services\orgs-api
.\scripts\scaffold-module.ps1 -ModuleName Teams
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Database connection string not configured" | Run via `./scripts/run-*.sh` or `.\scripts\run-*.ps1` (loads `.env` automatically) |
| "relation does not exist" | Run EF migrations (see step 4 above) |
| Port 5432 in use | Stop other PostgreSQL services or change port in `docker-compose.yml` |
| Port 3000 in use | Stop other Node processes on port 3000 |
| Containers won't start | Run `./scripts/dev-reset.sh` or `.\scripts\dev-reset.ps1`, then `dev-up` |
| Can't receive emails | Verify Mailhog is running: [http://localhost:18025](http://localhost:18025) |

## Next Steps

- [Architecture Standards](.agent/rules/architecture-standards.md)
- [Coding Standards](.agent/rules/coding-standards.md)
- [Testing Standards](.agent/rules/testing-standards.md)
