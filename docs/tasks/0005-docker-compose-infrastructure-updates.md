# Task: 0005-docker-compose-infrastructure-updates

## Metadata
- ID: 0005
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/0005-docker-compose-infrastructure-updates.md`

## Goal
Update all Docker Compose configuration and infrastructure scripts to support the full 6-service Propely topology. Ensure `docker compose --profile apps up -d` starts all services, and each service has corresponding run scripts for host-based development.

## Context
Tasks 0.2 (rename) and 0.3 (scaffold) created the 4 new service solutions (properties-api, publishing-api, contacts-api, appointments-api) with Dockerfile.dev files. However, docker-compose.yml only defines ai-api, orgs-api, and web as application services. The PostgreSQL init script only creates 2 databases. The .env.example and .env.docker files lack configuration for the new services. Run scripts exist only for ai-api and orgs-api.

## Scope
### In scope
- Add properties-api, publishing-api, contacts-api, and appointments-api to docker-compose.yml (apps profile)
- Update PostgreSQL init script to create all 6 databases
- Create run scripts (PS1 + SH) for all 4 new services
- Update .env.example with configuration for all new services
- Update .env.docker with container-network hostnames for new services
- Update dev-up scripts to list all services in output

### Out of scope
- Kubernetes manifests or production deployment config
- Database migrations for new services
- CI pipeline changes (Task 0.6)

## Requirements
- R1: All 6 backend services must be defined in docker-compose.yml under the apps profile
- R2: PostgreSQL init script must create all 6 service databases
- R3: Each new service must have PowerShell and Bash run scripts
- R4: .env.example must document variables for all services
- R5: .env.docker must override hostnames for container networking

## Acceptance Criteria
- AC1: `docker compose config` validates successfully with all 6 app services defined
- AC2: `docker compose up -d` starts all infrastructure services
- AC3: `docker compose --profile apps up -d` starts all 6 app services
- AC4: PostgreSQL init script creates all 6 databases on fresh startup
- AC5: `scripts/run-properties-api.ps1` starts properties-api on port 5030
- AC6: `scripts/run-publishing-api.ps1` starts publishing-api on port 5040
- AC7: `scripts/run-contacts-api.ps1` starts contacts-api on port 5050
- AC8: `scripts/run-appointments-api.ps1` starts appointments-api on port 5060
- AC9: `.env.example` contains variables for all 6 services
- AC10: dev-up scripts list all 6 services in output

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
Follow the existing patterns from ai-api and orgs-api service definitions. Add 4 new service blocks to docker-compose.yml with identical structure (build context, profiles, depends_on, healthcheck). Extend the init SQL script with 4 new CREATE DATABASE statements. Create run scripts by adapting the ai-api template.

## Implementation Steps
1. Update docker-compose.yml: add properties-api, publishing-api, contacts-api, appointments-api service definitions
2. Add named volumes for new service obj directories
3. Update PostgreSQL init script with 4 new databases
4. Create run scripts: run-properties-api.ps1/.sh, run-publishing-api.ps1/.sh, run-contacts-api.ps1/.sh, run-appointments-api.ps1/.sh
5. Update .env.example with new service configuration blocks
6. Update .env.docker with container-network overrides for new services
7. Update dev-up.ps1/.sh output to list all services
8. Build all services to verify no breakage

## Files to Create / Modify
- `docker-compose.yml` (modify)
- `infra/postgres/init/00-init-databases.sql` (modify)
- `scripts/run-properties-api.ps1` (create)
- `scripts/run-properties-api.sh` (create)
- `scripts/run-publishing-api.ps1` (create)
- `scripts/run-publishing-api.sh` (create)
- `scripts/run-contacts-api.ps1` (create)
- `scripts/run-contacts-api.sh` (create)
- `scripts/run-appointments-api.ps1` (create)
- `scripts/run-appointments-api.sh` (create)
- `.env.example` (modify)
- `.env.docker` (modify)
- `scripts/dev-up.ps1` (modify)
- `scripts/dev-up.sh` (modify)

## Testing Plan
- Unit tests: N/A (infrastructure task)
- Integration tests: N/A
- Manual verification: `docker compose config` validates; all 6 solutions build; run scripts reference correct paths

## Security & Privacy
- All ports bind to 127.0.0.1 only (not 0.0.0.0)
- .env.example contains only placeholder/dev passwords
- Database init script grants minimum required privileges

## Observability
- Each new service will report to Aspire Dashboard via OpenTelemetry (configured in .env)

## Rollback Plan
Revert the commit and run `dev-reset` to destroy volumes. Re-run `dev-up` with old configuration.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
