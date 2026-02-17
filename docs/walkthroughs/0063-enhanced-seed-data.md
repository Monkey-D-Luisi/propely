# Walkthrough: 0063-enhanced-seed-data

## Task Reference
- Task: `docs/tasks/0063-enhanced-seed-data.md`
- Walkthrough: `docs/walkthroughs/0063-enhanced-seed-data.md`
- Branch/PR: `feat/0063-enhanced-seed-data`
- Date: `2026-02-14`

## Summary
Created a dedicated `scripts/seed-demo.sh` (and `.ps1`) that populates a rich demo environment with 5 users, 3 organizations, 13 work items, feature flag toggles, and pending invitations. The script uses the REST APIs (Orgs API + AI API) rather than direct DB inserts, making it portable and self-documenting.

## Context
- Background: The existing `seed-dev.sh` creates minimal data (one admin user, one org) for development. For demos and screenshots, a richer dataset is needed.
- Problem statement: Need realistic multi-tenant data showcasing all features: multiple orgs with different roles, work items in various statuses, feature flags, and pending invitations.
- Constraints: No direct database access — must use the public REST APIs. Notifications cannot be created directly (side-effect only). Rate limiting on login endpoint (5/minute).

## Decisions & Trade-offs
- **Decision: Separate `seed-demo.sh` vs extending `seed-dev.sh`**
  - Options considered: (a) Extend seed-dev.sh, (b) Create separate seed-demo.sh
  - Why this choice: Separate scripts keep concerns clean. `seed-dev.sh` remains minimal for quick dev setup; `seed-demo.sh` creates a comprehensive showcase dataset.
  - Consequences / risks: Two scripts to maintain. Mitigated by shared patterns and conventions.

- **Decision: REST API calls vs direct DB inserts**
  - Options considered: (a) Use curl to call APIs, (b) Write SQL inserts, (c) Use EF Core seeding
  - Why this choice: API calls exercise the full stack (validation, events, side-effects), are self-documenting, and work regardless of schema changes.
  - Consequences / risks: Slower than direct inserts, subject to rate limiting. Mitigated by retry logic for 429 responses.

- **Decision: Work items not deduplicated on re-run**
  - Options considered: (a) Check for existing work items by title before creating, (b) Accept duplicates
  - Why this choice: The AI API has no "list by title" filter, and work items don't have a unique constraint on title. Querying all items to check for duplicates adds complexity.
  - Consequences / risks: Running the script twice creates duplicate work items. Documented in the script header. For a demo, running against a fresh DB (after `dev-reset.sh`) is recommended.

## Implementation Notes
- Key changes:
  - `scripts/seed-demo.sh` — Bash script (~300 lines) with helper functions for CSRF, register, login, create-org, invite, accept-invite, create-work-item, update-status, toggle-feature-flag. Includes `curl_retry` for 429 handling.
  - `scripts/seed-demo.ps1` — PowerShell equivalent with `Invoke-WithRetry` for rate limiting.
  - Both scripts create: 5 users, 3 orgs (Acme Corp, Startup Labs, Solo Project), invite/accept members, 10+3 work items, toggle 2 feature flags.
- Edge cases handled:
  - 409 Conflict on register → fall back to login + fetch user ID
  - 409 on org create → look up existing org by name in user's org list
  - 429 Rate Limited → retry after 15-second wait (up to 3 attempts)
  - Org owned by different user → skip gracefully (set ORG_ID empty)
- Known limitations:
  - Work items and invitations are created fresh each run (not idempotent)
  - Notifications cannot be created directly — they appear as side effects of invitations and role changes
  - Rate limiter (5 login calls/minute) can slow down idempotent re-runs

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
# Reset databases for clean test
docker exec saastemplate-postgres psql -U saastemplate -c "DROP DATABASE IF EXISTS saastemplate_aiapi;"
docker exec saastemplate-postgres psql -U saastemplate -c "DROP DATABASE IF EXISTS saastemplate_orgsapi;"
docker exec saastemplate-postgres psql -U saastemplate -c "CREATE DATABASE saastemplate_aiapi;"
docker exec saastemplate-postgres psql -U saastemplate -c "CREATE DATABASE saastemplate_orgsapi;"
docker restart saastemplate-ai-api saastemplate-orgs-api

# First run (clean DB)
bash scripts/seed-demo.sh

# Second run (idempotency test)
bash scripts/seed-demo.sh

# License header verification
bash scripts/verify-license-headers.sh
```

## Files Changed
- `scripts/seed-demo.sh` — New: rich demo seed script (bash), ~300 lines
- `scripts/seed-demo.ps1` — New: rich demo seed script (PowerShell), ~350 lines
- `docs/tasks/0063-enhanced-seed-data.md` — Status: PENDING → DONE
- `docs/backlog/epic-012-demo-marketing.md` — Task 0063: IN_PROGRESS → DONE, progress 1/2
- `docs/roadmap-v1.md` — Task 0063: PENDING → DONE

## Tests
### Unit
- N/A (shell scripts)

### Integration
- N/A

### Manual
- Ran `seed-demo.sh` on fresh database: all 5 users registered, 3 orgs created, members invited + accepted, 13 work items created, 4 statuses updated, 2 feature flags toggled. All green.
- Ran `seed-demo.sh` again (idempotency): all users detected as existing and logged in (yellow), orgs detected as existing (yellow), invitations/work items created fresh (green). Script completed successfully.
- Verified database counts: users (5), orgs (3), work items (13 first run, 26 after second run).

## Observability
- N/A

## Security
- Validation: Scripts use API-level validation (no direct DB access)
- AuthN/AuthZ impact: None
- Sensitive data handling: Demo password (`Demo123!`) is hardcoded — for local dev only

## Follow-ups / Backlog
- [ ] Task 0064: Demo video / walkthrough (depends on this task)

## Checklist
- [x] Task scope matches `docs/tasks/0063-enhanced-seed-data.md`
- [x] Tests updated and passing (manual verification)
- [x] Docs updated where relevant
- [x] No secrets committed
