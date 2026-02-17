# Audit Action: audit-0006-docker-cli-v2

## Metadata
- ID: audit-0006
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-31
- Priority: P2
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Standardize dev scripts to a single Docker CLI (v2)"
- Dependencies: None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0006-docker-cli-v2.md`
  - DevOps audit: `docs/audits/2026-01-30-devops-readiness.md` (Finding F-02)

## Goal
Standardize all dev scripts to use `docker compose` (v2) instead of mixed `docker-compose` (v1) and `docker compose` commands.

## Context
The DevOps audit (F-02) identified that PowerShell scripts use `docker-compose` (v1) while Bash scripts use `docker compose` (v2), creating inconsistent prerequisites across platforms. Docker Compose v1 is deprecated and v2 is the current standard.

## Scope
### In scope
- Update PowerShell scripts to use `docker compose` (v2)
- Ensure consistent command syntax across all scripts

### Out of scope
- Changing script logic or functionality
- Docker/Compose version requirements documentation

## Requirements
- R1: All scripts must use `docker compose` (v2 syntax)
- R2: Scripts must continue to work on Windows (PowerShell) and Unix (Bash)

## Acceptance Criteria
- [x] `dev-up.ps1` uses `docker compose up -d`
- [x] `dev-down.ps1` uses `docker compose down`
- [x] `dev-reset.ps1` uses `docker compose down -v --remove-orphans`
- [x] All 79 tests pass

## Constraints
- C1: Must not break existing script functionality

## Implementation Steps
1. Update `dev-up.ps1` to use `docker compose`
2. Update `dev-down.ps1` to use `docker compose`
3. Update `dev-reset.ps1` to use `docker compose`
4. Test scripts work correctly

## Definition of Done
- [x] Acceptance criteria met
- [x] Scripts work on local machine
- [x] Walkthrough updated
