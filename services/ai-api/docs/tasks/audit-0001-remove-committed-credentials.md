# Audit Action: audit-0001-remove-committed-credentials

## Metadata
- ID: audit-0001
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-30
- Priority: P0
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Remove committed credentials; move to environment/user secrets; update .env.example and docs"
- Dependencies:
  - None (first action in security baseline)
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0001-remove-committed-credentials.md`
  - Security baseline: `docs/standards/security-baseline.md`
  - Security audit: `docs/audits/2026-01-30-security.md` (Finding F-03)

## Goal
Remove all committed credentials from configuration files and replace with secure placeholders per the security baseline.

## Context
The security audit (F-03) identified that development credentials are committed in `appsettings.Development.json` and `.env.example`. The security baseline mandates:
- No secrets in config files
- `.env.example` committed with placeholder values only (e.g., `CHANGEME`)
- Actual credentials managed via environment variables or user secrets

## Scope
### In scope
- Replace real passwords in `.env.example` with `CHANGEME` placeholders
- Remove hardcoded RabbitMQ password from `appsettings.Development.json`
- Update connection string examples to use placeholders
- Update documentation (README.md, QUICKSTART.md) to reflect new setup requirements
- Add fallback environment variables to run scripts for when `.env` is missing
- Verify `.env` is properly gitignored (already confirmed)

### Out of scope
- JWT authentication implementation (separate P0 action)
- HTTPS/HSTS enforcement (P1 action)
- Docker compose default overhaul (acceptable for dev convenience with env var support)

## Requirements
- R1: `.env.example` must use `CHANGEME` for all password fields
- R2: `appsettings.Development.json` must not contain any real or default passwords
- R3: Connection strings in examples must use placeholder passwords
- R4: Documentation must explain how to set up credentials locally

## Acceptance Criteria
- [x] All passwords in `.env.example` replaced with `CHANGEME`
- [x] RabbitMQ password removed from `appsettings.Development.json`
- [x] Connection strings use `CHANGEME` placeholder
- [x] Build passes
- [x] Tests pass

## Constraints
- C1: Must not break local development workflow for existing users
- C2: Must align with security baseline in `docs/standards/security-baseline.md`

## Implementation Steps
1. Update `.env.example` to use `CHANGEME` placeholders for all passwords
2. Update `appsettings.Development.json` to remove hardcoded RabbitMQ password
3. Run build and tests to verify no breakage
4. Update walkthrough with summary

## Testing Plan
- Unit tests: No new tests required (configuration change only)
- Integration tests: Verify tests still pass (they use Testcontainers, not config files)
- Manual checks: Verify `.env.example` has placeholders, no passwords in tracked files

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (79 tests)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
