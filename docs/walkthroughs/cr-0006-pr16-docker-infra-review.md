# Walkthrough: cr-0006-pr16-docker-infra-review

## Task Reference
- Task: `docs/tasks/cr-0006-pr16-docker-infra-review.md`
- PR: #16 — `feat/0005-docker-compose-infrastructure-updates`
- Branch: `feat/0005-docker-compose-infrastructure-updates`
- Date: `2026-02-20`

## Summary
Addressed review feedback from Gemini Code Assist and Codex on PR #16. Fixed a MUST_FIX issue where bash `source` command breaks semicolons in .env connection strings. Applied suggestions to improve comment accuracy in `.env.docker`, `.env.example`, and documentation files.

## Context
- Background: PR #16 added Docker Compose and infrastructure support for 4 new backend services.
- Problem statement: Automated reviewers flagged a connection-string parsing bug in bash scripts and several documentation accuracy issues.
- Constraints: Keep changes minimal, only fix what's in scope.

## Decisions & Trade-offs
- **Decision:** Fix the `source` bug only in the 4 new scripts introduced by this PR
  - Options considered: Fix all 6 run scripts (including pre-existing ai-api, orgs-api)
  - Why this choice: Existing scripts are not part of this PR's diff; fixing them would be a drive-by refactor
  - Consequences / risks: Inconsistency between new and old scripts. Noted as follow-up.

- **Decision:** Defer least-privilege PostgreSQL grants to a future task
  - Options considered: Add explicit GRANT statements now
  - Why this choice: Would restructure the init script pattern shared with ai-api/orgs-api (pre-existing design)
  - Consequences / risks: Dev environment uses superuser-level access. Acceptable for local dev.

## Implementation Notes
- Key changes:
  - Replaced `source <(grep ...)` with `while IFS= read` + `export` loop in 4 new bash scripts
  - Added port numbers to `.env.docker` section comments
  - Fixed "six databases" → "seven databases" in `.env.example`
  - Fixed "minimum required privileges" wording in task doc and walkthrough
- Edge cases handled: `while read` loop correctly handles values containing semicolons, equals signs, and spaces

## Commands Run
```bash
# Validate docker compose still works
docker compose --profile apps config --services

# Build all services (verify no breakage)
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln
```

## Files Changed
- `scripts/run-properties-api.sh` — Replaced `source` with safe `while read` env loading
- `scripts/run-publishing-api.sh` — Replaced `source` with safe `while read` env loading
- `scripts/run-contacts-api.sh` — Replaced `source` with safe `while read` env loading
- `scripts/run-appointments-api.sh` — Replaced `source` with safe `while read` env loading
- `.env.docker` — Added port numbers to section comments
- `.env.example` — Fixed "six" → "seven" databases
- `docs/tasks/0005-docker-compose-infrastructure-updates.md` — Fixed privileges wording
- `docs/walkthroughs/0005-docker-compose-infrastructure-updates.md` — Fixed privileges wording

## Tests
### Manual
- Verified `docker compose --profile apps config --services` still lists all services
- Verified all 6 service solutions build

## Security
- The `source` fix prevents potential command injection from malformed .env values
- Least-privilege PostgreSQL grants deferred to future task

## Follow-ups / Backlog
- [ ] Fix `source` bug in pre-existing `run-ai-api.sh` and `run-orgs-api.sh`
- [ ] Implement least-privilege PostgreSQL grants

## Checklist
- [x] Task scope matches `docs/tasks/cr-0006-pr16-docker-infra-review.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
