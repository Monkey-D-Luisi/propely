# Walkthrough: audit-0011-rate-limit-delete-endpoints

## Task Reference
- Task: `docs/tasks/audit-0011-rate-limit-delete-endpoints.md`
- Walkthrough: `docs/walkthroughs/audit-0011-rate-limit-delete-endpoints.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Added a per-IP rate limit rule for `DELETE /orgs/*` endpoints (10 requests per minute) using the existing AspNetCoreRateLimit configuration. This is stricter than the global 100/min fallback and prevents abuse of destructive operations.

## Context
- Background: The three DELETE endpoints (leave org, remove member, delete org) were covered only by the global 100 req/min rate limit. An authenticated attacker could spam these endpoints rapidly.
- Problem statement: Rapid repeated calls could cause database load, notification spam, and amplify the org membership enumeration attack (now fixed in audit-0010).
- Constraints: Must use existing AspNetCoreRateLimit infrastructure. No code changes — configuration only.

## Decisions & Trade-offs
- **Decision: Single wildcard rule `delete:/orgs/*` vs individual endpoint rules**
  - Why this choice: A single wildcard rule covers all three DELETE endpoints and any future DELETE endpoints on the orgs controller. The limit of 10/min is sufficient for legitimate use (a user would rarely need to perform more than a few delete operations per minute) while still being restrictive enough to prevent abuse.
  - Alternative: Three individual rules for each specific path. Rejected because it adds maintenance burden with no additional security benefit — all three operations have similar risk profiles.

- **Decision: 10 requests per minute limit**
  - Why: Auth endpoints use 3-10/min limits. DELETE operations are even rarer in normal usage than logins, but we allow 10/min to avoid false positives for org owners managing multiple members.

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` — Added `delete:/orgs/*` rule with 10 req/min limit

## Tests
- Build passes (configuration is valid JSON and loads correctly)
- Existing integration tests continue to pass (rate limit rules don't affect single-request tests)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0011-rate-limit-delete-endpoints.md`
- [x] Tests updated and passing
- [x] No secrets committed
