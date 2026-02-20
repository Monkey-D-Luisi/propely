# Walkthrough: 0005-docker-compose-infrastructure-updates

## Task Reference
- Task: `docs/tasks/0005-docker-compose-infrastructure-updates.md`
- Walkthrough: `docs/walkthroughs/0005-docker-compose-infrastructure-updates.md`
- Branch/PR: `feat/0005-docker-compose-infrastructure-updates`
- Date: `2026-02-20`

## Summary
Extended the Docker Compose configuration and all supporting infrastructure scripts to support the full 6-service Propely topology. Added docker-compose service definitions for properties-api, publishing-api, contacts-api, and appointments-api. Updated the PostgreSQL init script to create all 6 databases. Created 8 new run scripts (PS1 + SH) for host-based development. Updated `.env.example` and `.env.docker` with configuration for all new services. Updated dev-up scripts to list all services in their output.

## Context
- Background: Tasks 0.2 and 0.3 created the 4 new service solutions with Dockerfile.dev files, but docker-compose.yml only had ai-api, orgs-api, and web defined.
- Problem statement: Developers could not start the full stack with Docker Compose or use run scripts for the new services.
- Constraints: Must follow existing patterns exactly (port binding to 127.0.0.1, apps profile, healthcheck structure, env_file layering).

## Decisions & Trade-offs
- **Decision:** Service definitions follow the exact same pattern as ai-api/orgs-api
  - Options considered: Shared base service with `extends`, individual definitions
  - Why this choice: Individual definitions are explicit, easy to understand, and consistent with existing services
  - Consequences / risks: Some duplication, but easily maintainable

- **Decision:** All new services depend on postgres, rabbitmq, and redis
  - Options considered: Only depending on postgres (the strict minimum today)
  - Why this choice: All services will need messaging and caching eventually; this avoids future docker-compose changes
  - Consequences / risks: Slightly longer startup if a service doesn't immediately need RabbitMQ/Redis

## Implementation Notes
- Key changes:
  - 4 new service blocks in docker-compose.yml with named obj volumes for hot-reload build caching
  - PostgreSQL init script creates 6 databases with pgcrypto and uuid-ossp extensions
  - 8 new run scripts (4 PS1 + 4 SH) following the ai-api template pattern
  - .env.example expanded with DB, messaging, caching, JWT, observability, and CORS vars for each new service
  - .env.docker expanded with container-network hostname overrides for each new service
  - dev-up.ps1/.sh output updated to show all 6 API services + web
- Edge cases handled: Shell scripts marked executable via git
- Known limitations: Docker Compose `--profile apps` must be used to start app services (this is existing behavior, not new)

## Data / Schema / Migrations
- DB changes: 4 new databases created by init script (propely_propertiesapi, propely_publishingapi, propely_contactsapi, propely_appointmentsapi)
- Migration strategy: Init script runs on first PostgreSQL startup only
- Backward compatibility: Existing databases (propely_aiapi, propely_orgsapi) are unaffected; requires `dev-reset` to pick up new databases on existing volumes

## Commands Run
```bash
# Build all 6 services
dotnet build services/ai-api/Propely.AiApi.sln           # 0 errors
dotnet build services/orgs-api/Propely.OrgsApi.sln         # 0 errors
dotnet build services/properties-api/Propely.PropertiesApi.sln  # 0 errors
dotnet build services/publishing-api/Propely.PublishingApi.sln  # 0 errors
dotnet build services/contacts-api/Propely.ContactsApi.sln      # 0 errors
dotnet build services/appointments-api/Propely.AppointmentsApi.sln  # 0 errors

# Test all 6 services
dotnet test services/ai-api/Propely.AiApi.sln --no-build           # 169 passed
dotnet test services/orgs-api/Propely.OrgsApi.sln --no-build       # 616 passed
dotnet test services/properties-api/Propely.PropertiesApi.sln --no-build  # 6 passed
dotnet test services/publishing-api/Propely.PublishingApi.sln --no-build  # 6 passed
dotnet test services/contacts-api/Propely.ContactsApi.sln --no-build     # 6 passed
dotnet test services/appointments-api/Propely.AppointmentsApi.sln --no-build  # 6 passed

# Validate docker compose
docker compose --profile apps config --services  # Lists all 12 services (5 infra + 6 app + web)
```

## Files Changed
- `docker-compose.yml` — Added 4 new service definitions (properties-api, publishing-api, contacts-api, appointments-api) with named obj volumes
- `infra/postgres/init/00-init-databases.sql` — Added CREATE DATABASE + extensions for 4 new databases
- `scripts/run-properties-api.ps1` — New: runs properties-api on port 5030
- `scripts/run-properties-api.sh` — New: runs properties-api on port 5030
- `scripts/run-publishing-api.ps1` — New: runs publishing-api on port 5040
- `scripts/run-publishing-api.sh` — New: runs publishing-api on port 5040
- `scripts/run-contacts-api.ps1` — New: runs contacts-api on port 5050
- `scripts/run-contacts-api.sh` — New: runs contacts-api on port 5050
- `scripts/run-appointments-api.ps1` — New: runs appointments-api on port 5060
- `scripts/run-appointments-api.sh` — New: runs appointments-api on port 5060
- `.env.example` — Added configuration blocks for all 4 new services
- `.env.docker` — Added container-network hostname overrides for all 4 new services
- `scripts/dev-up.ps1` — Updated output to list all 6 API services and all 6 databases
- `scripts/dev-up.sh` — Updated output to list all 6 API services and all 6 databases

## Tests
### Unit
- What was added/updated: N/A (infrastructure task, no new code tests)
- How to run: `dotnet test` for each service

### Integration
- What was added/updated: N/A
- How to run: `docker compose --profile apps config --services` validates all services are defined

### Manual
- What you verified: `docker compose --profile apps config --services` lists all 12 services, all 6 .NET builds succeed with 0 errors, all 6 test suites pass

## Observability
- Logs added/updated: N/A
- Traces/metrics added/updated: OpenTelemetry endpoint configured for all new services in .env.example and .env.docker

## Security
- Validation: All ports bind to 127.0.0.1 only
- AuthN/AuthZ impact: JWT configuration placeholders added (commented out) for all new services
- Sensitive data handling: .env.example contains only dev placeholder passwords; no real secrets

## Follow-ups / Backlog
- [ ] Task 0.6: CI Pipeline Updates (depends on this task)
- [ ] After fresh `dev-reset`, verify all 6 databases are created and services start healthily

## Checklist
- [x] Task scope matches `docs/tasks/0005-docker-compose-infrastructure-updates.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
