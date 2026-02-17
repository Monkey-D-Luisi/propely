# Walkthrough: audit-0008-csrf-token-ttl

## Task Reference
- Task: `docs/tasks/audit-0008-csrf-token-ttl.md`
- Walkthrough: `docs/walkthroughs/audit-0008-csrf-token-ttl.md`
- Branch/PR: `fix/audit-0006-session-invalidation`
- Date: `2026-02-09`

## Summary
Added a timestamp to CSRF tokens and added TTL validation (1 hour) to prevent stale token replay. The token format changed from `{random_hex}` to `{unix_timestamp_hex}.{random_hex}`. Tokens older than 1 hour are now rejected with 403.

## Context
- Background: CSRF tokens had no expiration — once issued, they remained valid indefinitely as long as the cookie persisted.
- Problem statement: A stolen CSRF token cookie could be used indefinitely.
- Constraints: Must not break existing test infrastructure or active user sessions.

## Decisions & Trade-offs
- **Decision: Embedded timestamp in token vs server-side storage**
  - Options considered: (1) Embed timestamp in token string, (2) Store token issuance time in Redis
  - Why this choice: Embedded timestamp keeps the CSRF system stateless (no Redis dependency). The timestamp is hex-encoded and prepended to the random bytes separated by a dot.
  - Consequences: Old-format tokens (without dots) are rejected, which is acceptable since they only exist in the current browser session's cookie.

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` — Token generation now includes unix timestamp; validation parses timestamp and rejects tokens > 1 hour old

## Tests
### Integration
- All existing CSRF tests pass with the new token format (the test helpers call `GetCsrfTokenAsync` which fetches fresh tokens)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0008-csrf-token-ttl.md`
- [x] Tests updated and passing
- [x] No secrets committed
