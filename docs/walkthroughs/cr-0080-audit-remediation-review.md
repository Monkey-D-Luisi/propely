# CR-0080: Audit Remediation PR Review — Walkthrough

## Task Reference
- **Task**: `docs/tasks/cr-0080-audit-remediation-review.md`
- **PR**: [#296](https://github.com/Monkey-D-Luisi/saas-template/pull/296)

## What Changed

### 1. Added outbox dead-letter EF Core migration
- Generated migration `AddOutboxDeadLetterColumns` for `retry_count` (int, default 0) and `failed_at_utc` (datetime, nullable) on `outbox_messages`
- Updated partial index filter to include `AND failed_at_utc IS NULL`
- Resolves `PendingModelChangesWarning` → fixes 2 integration tests + E2E container health

### 2. Added sys_admin claim to DevAuthenticationHandler
- The SEC-3 fix tightened AdminOnly policy to require `sys_admin` claim
- DevAuthenticationHandler's synthetic identity was missing this claim
- Added `new Claim(AuthClaimTypes.SystemAdmin, "true")` to the claims array
- Fixes `FeatureFlagEndpointTests.Toggle_WhenAuthenticated_ShouldSucceed` (was 403, now 200)

### 3. Fixed proxy.ts CSP nonce generation
- Replaced `Buffer.from(crypto.randomUUID()).toString('base64')` with `btoa(crypto.randomUUID())` for Edge runtime
- Changed `response.headers.set('x-nonce', nonce)` to set the nonce on the request headers via `NextResponse.next()` pattern, so server components can read it via `headers()`

### 4. Fixed format.ts date validation
- Added `Number.isNaN(date.getTime())` check before formatting in all three functions
- Changed `formatDateLong` from `toLocaleDateString` to `toLocaleString` so hour/minute options are honored

### 5. Fixed notifications.ts polling AbortSignal
- Passed `controller.signal` to the interval callback so polling requests are cancelled on unmount

### 6. Added abort guards in hook finally blocks
- Added `if (!signal?.aborted)` guard around state updates in `finally` blocks for `useWorkItems`, `useWorkItem`, `useNotifications`, and other data-fetching hooks

### 7. Combined AppHeader.tsx useEffect hooks
- Merged outside-click and Escape key handlers into a single `useEffect` with early return when menu is closed

## Commands Run
```bash
# Generate migration
dotnet ef migrations add AddOutboxDeadLetterColumns --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api

# Backend build + test
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

# Frontend build + test
cd apps/web && npx tsc --noEmit
cd apps/web && npm test
```

## Process Deviations
None — all fixes were within scope of the PR review feedback.
