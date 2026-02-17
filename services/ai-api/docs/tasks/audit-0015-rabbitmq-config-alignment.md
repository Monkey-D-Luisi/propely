# Audit Action: audit-0015-rabbitmq-config-alignment

## Metadata
- ID: audit-0015
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-01
- Priority: P1
- Source:
  - Executive summary: `docs/audits/2026-02-01-executive-summary.md`
  - Action plan item: "Replace unused RabbitMQ connection string variables with `RabbitMQ__*` settings."
- Dependencies:
  - `docs/tasks/audit-0011-documentation-stabilization.md`
  - `docs/tasks/audit-0012-cr-walkthroughs.md`
  - `docs/tasks/audit-0013-english-only-docs.md`
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0015-rabbitmq-config-alignment.md`

## Goal
Align RabbitMQ configuration variables with the application settings by replacing unused connection string variables with the `RabbitMQ__*` configuration keys.

## Context
The DevOps Readiness audit identified that the RabbitMQ connection string variable in `.env.example` is unused and misleading. This action ensures environment variables, Docker Compose, and documentation match the configuration actually consumed by the application.

## Scope
### In scope
- Replace unused RabbitMQ connection string variables with `RabbitMQ__*` settings in `.env.example` and Docker Compose.
- Align application configuration bindings to the documented `RabbitMQ__*` keys.
- Update documentation references to RabbitMQ configuration variables.

### Out of scope
- Changes to RabbitMQ infrastructure or broker topology.
- Non-RabbitMQ configuration refactors.

## Requirements
- R1: RabbitMQ settings use `RabbitMQ__*` configuration keys across environment files and Docker Compose.
- R2: Documentation references the updated RabbitMQ configuration keys.
- R3: The application reads the new configuration values without breaking existing development workflows.

## Acceptance Criteria
- [x] AC1: `.env.example` and Docker Compose reference `RabbitMQ__*` settings only.
- [x] AC2: Application configuration binding matches `RabbitMQ__*` settings.
- [x] AC3: Documentation referencing RabbitMQ config is updated.
- [x] AC4: Walkthrough documents decisions, commands, and files changed.

## Constraints
- C1: Keep changes limited to RabbitMQ configuration alignment.
- C2: Do not introduce secrets into the repository.

## Implementation Steps
1. Review current RabbitMQ configuration usage in code and configuration files.
2. Update `.env.example`, Docker Compose, and appsettings to use `RabbitMQ__*` keys.
3. Update documentation references to match the new settings.
4. Validate configuration usage in local development workflows.

## Testing Plan
- Unit tests: Add or update tests only if configuration binding changes require coverage.
- Integration tests: `dotnet test` (or targeted integration tests if added).
- Manual checks: Verify RabbitMQ configuration variables are consumed as expected in development.

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes (N/A - config/docs change, CI verified)
- [x] Tests pass (N/A - config/docs change, CI verified)
- [x] Formatting/analyzers pass (N/A - config/docs change)
- [x] No secrets committed
- [x] Walkthrough updated
