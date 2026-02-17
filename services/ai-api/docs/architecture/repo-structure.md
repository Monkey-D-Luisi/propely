# Repository Structure

> **Monorepo Note:** The ai-api service is part of the `saas-template` monorepo. This document describes the ai-api service layout within the monorepo.

## Overview
This document describes the ai-api service directory layout within the monorepo at `services/ai-api/`.

## Monorepo Root (relevant files)

| File | Purpose |
|------|---------|
| `QUICKSTART.md` | Quick start guide for the full monorepo |
| `.env.example` | Local development environment template |
| `docker-compose.yml` | Local infrastructure and services |
| `.agent.md` | Agent governance (highest priority) |
| `CLAUDE.md` | Claude Code instructions |
| `AGENTS.md` | GitHub Copilot instructions |
| `.editorconfig` | Editor formatting rules |
| `scripts/dev-up.sh` | Start full stack |
| `scripts/run-ai-api.sh` | Run ai-api service only |

## Service Directory Structure

```
services/ai-api/
├── SaasTemplate.AiApi.sln       # Solution file
├── .gitattributes                # Git line ending configuration
├── docs/                         # Service-specific documentation
│   ├── architecture/             # Architecture docs and ADRs
│   │   └── decisions/            # Architecture Decision Records
│   ├── standards/                # Coding and process standards
│   ├── runbooks/                 # Operational guides
│   ├── backlog/                  # Service-specific backlog
│   └── audits/                   # Audit reports
├── scripts/                      # Service-specific scripts
│   ├── scaffold-module.ps1       # Generate new Clean Architecture module
│   └── scaffold-module.sh
├── src/                          # Source code
│   ├── SaasTemplate.AiApi.Domain/         # Domain layer (pure logic)
│   ├── SaasTemplate.AiApi.Application/    # Application layer (CQRS)
│   ├── SaasTemplate.AiApi.Infrastructure/ # Infrastructure layer (EF, RabbitMQ, Redis)
│   └── SaasTemplate.AiApi.Api/            # Presentation layer (controllers, middleware)
└── tests/                        # Test projects
    ├── SaasTemplate.AiApi.UnitTests/
    ├── SaasTemplate.AiApi.IntegrationTests/
    └── SaasTemplate.AiApi.ArchitectureTests/
```

## Directory Details

### `docs/`
Service-specific documentation.

- `architecture/`: System design, vertical slices, AI client integration
- `architecture/decisions/`: Architecture Decision Records
- `standards/`: Naming conventions, versioning, security baseline
- `runbooks/`: Operational procedures (local development)
- `backlog/`: Service-specific task backlog and epics
- `audits/`: Audit reports and rollup summaries

### `scripts/`
Service-specific scripts (module scaffolding). Dev scripts (`dev-up.sh`, `dev-down.sh`, etc.) are at the monorepo root.

### `src/`
Application source code organized by Clean Architecture layers.

### `tests/`
Test projects mirroring source structure:
- `UnitTests/`: Pure unit tests (domain logic, handlers)
- `IntegrationTests/`: Tests with Testcontainers (database, API)
- `ArchitectureTests/`: Architecture conformance tests (layer dependencies)

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Tasks | `NNNN-<title>.md` | `0001-template-baseline.md` |
| Fast track | `ft-NNNN-<title>.md` | `ft-0001-hotfix.md` |
| Code review | `cr-NNNN-<title>.md` | `cr-0015-security-review.md` |
| Audit action | `audit-####-<title>.md` | `audit-0001-cookie-secure-flag.md` |
| ADRs | `NNNN-<title>.md` | `0001-use-postgres.md` |
| Standards | `<topic>.md` | `naming-conventions.md` |
| Runbooks | `<topic>.md` | `local-development.md` |
