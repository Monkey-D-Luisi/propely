# CR-0080: Audit Remediation PR Review

## PR Metadata
- **PR**: [#296](https://github.com/Monkey-D-Luisi/saas-template/pull/296)
- **Title**: fix: comprehensive audit remediation (14 fixes across security, architecture, UX, and DevOps)
- **Branch**: `fix/audit-remediation-2026-02` → `main`
- **CI Status**: 5 PASS, 2 FAIL (Orgs API integration tests, E2E smoke tests)

## Changed Files
92 files across security (SEC-1–SEC-8), architecture (ARCH-1/4/5), UX (UX-1/3/4/5/7/8), and DevOps (OPS-1/2, PERF-5).

## Review Sources
- Inline review comments: **9** (7 Copilot, 2 Gemini)
- General reviews: **2** (Copilot summary, Gemini summary)
- Issue comments: **2** (ChatGPT Codex usage limit, Gemini summary — neither actionable)

## Comment Resolution Plan

### MUST_FIX (CI-blocking)

- [x] **Missing outbox dead-letter EF migration** (Copilot #2807946223 on OutboxMessageConfiguration.cs:63, Gemini #2807946680 on OutboxMessage.cs:19)
  - ARCH-5 added `RetryCount` and `FailedAtUtc` to the model/config but no migration was generated
  - CI: `PendingModelChangesWarning` → 2 integration test failures + E2E unhealthy container
  - **Fix**: Generate EF Core migration `AddOutboxDeadLetterColumns`

- [x] **DevAuthenticationHandler missing sys_admin claim** (self-found via CI analysis)
  - SEC-3 tightened AdminOnly policy to require `sys_admin` claim
  - DevAuthenticationHandler doesn't include `sys_admin` → FeatureFlagEndpointTests.Toggle returns 403
  - **Fix**: Add `sys_admin` = `"true"` claim to DevAuthenticationHandler's synthetic identity

### MUST_FIX (runtime correctness)

- [x] **proxy.ts: Buffer in Edge runtime** (Copilot #2807946208 on proxy.ts:16)
  - `Buffer.from(crypto.randomUUID()).toString('base64')` crashes in Edge runtime
  - **Fix**: Use `btoa(crypto.randomUUID())` (web-compatible)

- [x] **proxy.ts: x-nonce on response headers** (Copilot #2807946212 on proxy.ts:41)
  - Server components read from request headers, not response headers
  - **Fix**: Clone request headers and set x-nonce on the request via `NextResponse.next()`

- [x] **format.ts: Invalid date handling** (Copilot #2807946217 on format.ts:17)
  - `new Date(invalid)` returns "Invalid Date" (doesn't throw); try/catch is no-op
  - **Fix**: Check `Number.isNaN(date.getTime())` and return `dateStr` fallback

- [x] **format.ts: formatDateLong uses toLocaleDateString** (Copilot #2807946218 on format.ts:35)
  - `toLocaleDateString` ignores `hour`/`minute` options in most runtimes
  - **Fix**: Use `toLocaleString` instead

### SHOULD_FIX

- [x] **notifications.ts: Polling without AbortSignal** (Copilot #2807946221 on notifications.ts:55)
  - Interval callback calls `fetcher()` without signal; requests won't be cancelled on unmount
  - **Fix**: Pass `controller.signal` into interval callback

- [x] **Hooks: State updates after abort in finally block** (Copilot suppressed, work-items.ts:52)
  - `finally { setLoading(false) }` executes even when signal is aborted → potential post-unmount state update
  - **Fix**: Guard state updates in finally with `signal?.aborted` check

### SUGGESTION

- [x] **AppHeader.tsx: Combine useEffect hooks** (Gemini #2807946681 on AppHeader.tsx:107)
  - Two useEffect hooks for outside-click and Escape can be merged into one
  - **Fix**: Combine into single useEffect

### NOT APPLICABLE

- **Copilot suggestion on next.config.ts:9** (#2807946228): Suggested restoring static CSP headers in next.config.ts
  - **Rationale**: The project uses `next-intl/plugin` which compiles `src/proxy.ts` as the middleware entry point. CSP headers ARE being set via middleware (proxy.ts). The `.next/server/middleware.js` build artifact confirms this. Restoring static headers would create duplicate/conflicting CSP. The comment in next.config.ts accurately describes the current setup.

## Parity Verification Checklist
- [x] Redirect parity checked (`next` propagation and sanitization) — N/A for this review pass
- [x] Locale source correctness checked — N/A for this review pass
- [x] API/UI contract parity checked — migration added for outbox columns
- [x] Test parity checked — integration tests now pass with sys_admin claim + migration
