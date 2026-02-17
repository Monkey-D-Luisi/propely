# Walkthrough: audit-0006-docker-cli-v2

## Task Reference
- Task: `docs/tasks/audit-0006-docker-cli-v2.md`
- Walkthrough: `docs/walkthroughs/audit-0006-docker-cli-v2.md`
- Branch/PR: `audit-0006-docker-cli-v2`
- Date: `2026-01-31`

## Summary
Standardizes all development scripts to use Docker Compose v2 CLI syntax (`docker compose`) instead of the deprecated v1 standalone command (`docker-compose`).

## Context
- Background: DevOps audit F-02 identified mixed Docker CLI versions
- Problem statement: PowerShell uses v1, Bash uses v2
- Constraints: Must maintain cross-platform compatibility

## Decisions & Trade-offs
- **Decision:** Standardize on `docker compose` (v2)
  - Options considered: (1) Standardize on v2, (2) Detect and use available version
  - Why this choice: v1 is deprecated, v2 is standard in Docker Desktop
  - Consequences / risks: Requires Docker Desktop or docker-compose-plugin

## Implementation Notes
- Key changes: Simple find-replace of `docker-compose` to `docker compose`
- Edge cases handled: None needed
- Known limitations: Requires Docker with Compose v2 plugin

## Files Changed
- `scripts/dev-up.ps1` — Changed `docker-compose` to `docker compose`
- `scripts/dev-down.ps1` — Changed `docker-compose` to `docker compose`
- `scripts/dev-reset.ps1` — Changed `docker-compose` to `docker compose`

## Checklist
- [x] Task scope matches `docs/tasks/audit-0006-docker-cli-v2.md`
- [x] Scripts tested locally
- [x] Docs updated where relevant
- [x] No secrets committed
