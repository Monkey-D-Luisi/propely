# Task: 0002-docker-compose-dev-scripts

## Metadata
- ID: 0002
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-28
- Related docs:
  - Walkthrough: `docs/walkthroughs/0002-docker-compose-dev-scripts.md`
  - Epic: `docs/backlog/work-item-management-epic.md`

## Goal
Set up the local development environment with Docker Compose for all infrastructure services (PostgreSQL, RabbitMQ, Redis).

## Context
The project requires local infrastructure services for development. Currently, the dev scripts exist but are placeholders waiting for docker-compose.yml to be created.

## Scope
### In scope
- Docker Compose configuration for PostgreSQL, RabbitMQ, Redis
- Environment variable template (.env.example)
- Working dev scripts (dev-up, dev-down, dev-reset)

### Out of scope
- Production deployment configuration
- Kubernetes manifests
- CI/CD pipeline integration

## Requirements
- R1: PostgreSQL accessible on port 5432
- R2: RabbitMQ accessible on port 5672 (AMQP) and 15672 (Management UI)
- R3: Redis accessible on port 6379
- R4: .env.example with CHANGEME placeholders (no real secrets)
- R5: dev-up.sh starts services with `docker compose up -d`
- R6: dev-down.sh stops services with `docker compose down`
- R7: dev-reset.sh destroys volumes with user confirmation

## Acceptance Criteria
- AC1: `./scripts/dev-up.sh` starts all services successfully
- AC2: `./scripts/dev-down.sh` stops services without data loss
- AC3: `./scripts/dev-reset.sh` destroys volumes (with user confirmation)
- AC4: `.env.example` committed with `CHANGEME` placeholders
- AC5: `.env` is in `.gitignore`
- AC6: Services are accessible at documented ports

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
Create standard Docker Compose setup with named volumes for data persistence. Use environment variables for all configurable values with secure defaults.

## Implementation Steps
1. Create docker-compose.yml with all three services
2. Create .env.example with placeholder credentials
3. Update dev-up.sh to run docker compose up -d
4. Update dev-down.sh to run docker compose down
5. Update dev-reset.sh to run docker compose down -v
6. Verify .env is in .gitignore (already done)

## Files to Create / Modify
- `docker-compose.yml` (create)
- `.env.example` (create)
- `scripts/dev-up.sh` (modify)
- `scripts/dev-down.sh` (modify)
- `scripts/dev-reset.sh` (modify)

## Testing Plan
- Unit tests: N/A (infrastructure only)
- Integration tests: N/A (infrastructure only)
- Manual verification:
  - Run dev-up.sh, verify services with `docker ps`
  - Connect to Postgres via psql or GUI
  - Access RabbitMQ management at http://localhost:15672
  - Connect to Redis via redis-cli

## Security & Privacy
- All credentials in .env.example use CHANGEME placeholders
- Real .env file is gitignored
- RabbitMQ management UI is local-only (127.0.0.1)

## Observability
- Logs: Docker compose logs available via `docker compose logs`
- Metrics: N/A for this task
- Traces: N/A for this task

## Rollback Plan
Delete docker-compose.yml and revert scripts to placeholder versions.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
