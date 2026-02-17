# Service Audit: web

## Audit Metadata
- **Service:** `apps/web/`
- **Date:** 2026-02-13
- **Auditor:** Agent
- **Status:** In Progress
- **Technology:** Next.js 16 + Tailwind CSS
- **Source files analyzed:** ~89 (components, hooks, lib, pages)
- **Test files analyzed:** 39 (391 test cases, all passing)

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Clean Code | 82/100 | Strong i18n, good structure, minor type assertion and component size issues |
| Architecture | 88/100 | Excellent separation of concerns, clean hook architecture, Zod schemas |
| Security | 85/100 | CSRF well-implemented, no XSS vectors, one open redirect concern |
| Performance | 78/100 | Good skeleton loading, but no dynamic imports, schema recreation issue |
| Test Coverage | 72/100 | 391 tests passing, 65% module coverage, billing entirely untested |
| **Overall** | **81/100** | **Solid production-ready frontend, main gaps in billing tests and perf** |

---

## Security Findings

### CRITICAL

*None identified.*

### HIGH

*None identified.*

### MEDIUM

#### F1. Potential Open Redirect via Protocol-Relative URL
- **Severity:** MEDIUM
- **OWASP:** A01 Broken Access Control
- **Files:** `apps/web/src/components/auth/LoginForm.tsx:49-60,117`, `apps/web/src/components/auth/RegisterForm.tsx:25,104`
- **Problem:** The `nextParam` validation checks `decoded.startsWith('/')` which allows protocol-relative URLs like `//evil.com`. The value is then used with `window.location.assign(nextParam)`.
- **Impact:** An attacker can craft `/login?next=%2F%2Fevil.com` that redirects authenticated users to a phishing page after login.
- **Recommendation:** Add check to reject `//` prefix: `if (decoded.startsWith('/') && !decoded.startsWith('//'))`. Or use `new URL(decoded, window.location.origin)` and verify origin matches.

### LOW

#### F2. Console Error Information Leakage
- **Severity:** LOW
- **OWASP:** A09 Security Logging Failures
- **Files:** `apps/web/src/components/work-items/WorkItemDetail.tsx:49`, `apps/web/src/components/work-items/WorkItemEdit.tsx:36`
- **Problem:** `console.error` calls log error objects to browser console in production.
- **Recommendation:** Replace with dev-only logging utility or guard with `process.env.NODE_ENV`.

#### F3. No Content Security Policy Headers
- **Severity:** LOW
- **OWASP:** A05 Security Misconfiguration
- **Files:** `apps/web/src/app/layout.tsx`, `apps/web/next.config.ts`
- **Problem:** No CSP meta tags or headers configuration detected. The app loads external Google Fonts.
- **Recommendation:** Add CSP headers via `next.config.ts` `headers()` with `default-src 'self'`, whitelisting `fonts.googleapis.com`.

#### F4. Unsafe Type Assertion on Error Body
- **Severity:** LOW
- **OWASP:** A04 Insecure Design
- **Files:** `apps/web/src/components/orgs/AcceptInvite.tsx:89`
- **Problem:** `(error.body as { error?: string } | undefined)?.error` uses type assertion without runtime validation.
- **Recommendation:** Use Zod `.safeParse()` or type guard to validate `error.body` shape.

### Positive Security Observations
- No `dangerouslySetInnerHTML` anywhere in the codebase
- No `eval()`, `Function()`, or `.innerHTML` usage
- No hardcoded secrets or API keys
- CSRF tokens consistently applied via `ensureCsrfToken()` + `x-csrf-token` header on all mutations
- Cookie-based auth with `credentials: 'include'` -- no tokens in localStorage
- Zod validation on all form inputs
- OAuthButtons sanitizes non-relative `nextPath` to root `/`
- Rate limit handling (429 with `Retry-After`) in `apiFetch`

---

## Clean Code

### Architecture Compliance

**Score:** 88/100

Excellent component structure with:
- Custom hooks for data fetching (`useCurrentUser`, `useMyOrgs`, `useOrg`, `useMembers`)
- Zod schemas for form validation (`lib/schemas.ts`)
- Error boundaries at global and route levels
- Clean routing with locale-aware navigation

### Strengths
- Consistent use of `useTranslations` hook for i18n across all components
- Proper error boundaries: `global-error.tsx`, `error.tsx`, `not-found.tsx`, catch-all route
- Clean separation of hooks from components
- `apiFetch` wrapper with typed `ApiError`, `isApiError` type guard, Zod response validation

### Issues
- **MembersManager.tsx** (~358 lines) -- largest component, handles search, invite, role changes, member removal, pagination. Candidate for decomposition.
- **5 `as` type assertions** bypass type safety: `MembersTable.tsx:89`, `WorkItemEdit.tsx:87`, `SmartFill.tsx:27`, `AcceptInvite.tsx:89` (plus 3 acceptable ref-forwarding patterns)
- **Hardcoded English** `loadingText = 'Submitting...'` default in `form-submit-button.tsx:14`
- **StatusBadge and WorkItemsTable** render status strings without i18n translation

### Design System Compliance

| Rule | Status |
|------|--------|
| Semantic `primary-*` classes (no `indigo-*`) | PASS |
| `bg-surface` for page backgrounds | PASS |
| `rounded-xl` for cards | PASS |
| `rounded-lg` for inputs/buttons | PASS |
| `rounded-full` for badges/pills | PASS |
| `focus:ring-2 focus:ring-primary-600` for inputs | PARTIAL FAIL -- form components use `focus:ring-primary-100` |
| Dialog overlay color | MINOR FAIL -- uses `bg-slate-900/40` instead of semantic token |

### Dead Code / Duplication
- `form-demo.tsx` exists in production source tree -- should be removed or moved to dev directory

---

## Performance

### Frontend Performance
- **Re-renders:** `WorkItemForm.tsx:21-24` creates a new Zod schema on every render without `useMemo`. Other form components correctly memoize schemas.
- **Bundle size:** No `next/dynamic` or `React.lazy` usage detected. All components statically imported. Next.js App Router provides automatic route-level splitting, but large components like `MembersManager` (358 lines) could benefit from dynamic imports.
- **Image optimization:** No `next/image` usage detected. Images appear to be loaded via icon fonts.
- **Data fetching patterns:** No waterfall issues detected. Proper `loading.tsx` files for Suspense boundaries.
- **Layout shifts (CLS):** Good skeleton loading states used consistently across all data-dependent views.
- **Route-level code splitting:** Automatic via Next.js App Router.

### Additional Performance Notes
- External `material-symbols-outlined` font from Google Fonts adds render-blocking request
- Debounced search in `MembersManager.tsx` prevents excessive API calls
- Small dependency footprint: 6 production deps (next, react, react-dom, next-intl, react-hook-form, zod)

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Frontend (components) | 33 files | Billing module (4 components) entirely untested |
| Frontend (hooks) | 4 files | `billing.ts` and `notifications.ts` hooks untested |
| Frontend (lib) | 3 files | `roles.ts` and `auth-events.ts` untested |
| Frontend (pages) | 1 file | 28 pages untested (mostly thin wrappers) |

**Overall: 39 test files, 391 test cases, 100% passing, ~65% module coverage**

### Missing Tests

**High Priority:**
1. `src/components/billing/BillingManagement.tsx` -- Revenue-critical, zero tests
2. `src/components/billing/PricingContent.tsx` -- Checkout flow, zero tests
3. `src/components/billing/PaymentHistory.tsx` -- Financial data display, zero tests
4. `src/components/billing/PlanCard.tsx` -- Plan rendering, zero tests
5. `src/hooks/billing.ts` -- All billing hooks, zero tests
6. `src/components/orgs/AcceptInvite.tsx` -- Complex multi-state flow, zero tests
7. `src/hooks/notifications.ts` -- Notification hooks, zero tests

**Medium Priority:**
8. `src/lib/roles.ts` -- Role utility used in permission checks
9. `src/components/ui/dialog-overlay.tsx` -- Shared modal with focus trap
10. `src/components/ui/pagination.tsx` -- Shared pagination component
11. `src/components/layout/LanguageSwitcher.tsx` -- Locale switching

---

## What's Done Well

1. **CSRF implementation is exemplary.** Every mutation follows consistent `ensureCsrfToken()` + `x-csrf-token` header pattern across all 12+ form components. (`LoginForm.tsx`, `RegisterForm.tsx`, `ForgotPasswordForm.tsx`, etc.)
2. **Robust API error handling.** `apiFetch` wrapper with typed `ApiError`, `isApiError` type guard, `getDomainErrorCode`, Zod response validation, `Retry-After` parsing. (`lib/api.ts`)
3. **Design system compliance.** Zero `indigo-*` usages, semantic `primary-*` classes throughout, correct border radius tokens, `bg-surface` for backgrounds. (`globals.css`, all components)
4. **Strong i18n foundation.** `next-intl` properly integrated, `useTranslations` in every component, locale-aware routing, i18n-aware form validation messages, locale-aware currency formatting. (`i18n/routing.ts`, `i18n/navigation.ts`)
5. **Comprehensive error boundaries.** Global, route-level, 404, and catch-all route error handling. (`global-error.tsx`, `error.tsx`, `not-found.tsx`, `[...rest]/page.tsx`)
6. **Testing practices.** All 391 tests pass, consistent `renderWithProviders` helper, proper mock patterns with `vi.hoisted()`, excellent edge case coverage (loading, error, rate limiting states).
7. **Clean hook architecture.** Custom hooks cleanly separate data fetching from presentation. Consistent error/loading patterns. (`hooks/orgs.ts`, `hooks/billing.ts`, `hooks/work-items.ts`)

---

## Prioritized Action Plan

> This table is consumed by the `fix service audits` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Category | Title | Description | Files | Dependencies | Status |
|---|----------|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | MEDIUM | Security | Fix open redirect via protocol-relative URL | Add `!decoded.startsWith('//')` check in `nextParam` validation | `src/components/auth/LoginForm.tsx:49-60,117`, `src/components/auth/RegisterForm.tsx:25,104` | None | Done (PR #270 — check added in LoginForm, RegisterForm, OAuthButtons) |
| 2 | P1 | MEDIUM | Security | Add Content Security Policy headers | Configure CSP in `next.config.ts` or middleware, whitelist Google Fonts | `next.config.ts` | None | Done (PR #270 — CSP configured in next.config.ts headers) |
| 3 | P1 | HIGH | Test Coverage | Add billing module tests | 4 components + 1 hook with zero test coverage on revenue-critical path | `src/components/billing/*.tsx`, `src/hooks/billing.ts` | None | Done (PR #270 — 4 component tests + hook tests added) |
| 4 | P1 | MEDIUM | Test Coverage | Add AcceptInvite component tests | Complex multi-state component with no tests | `src/components/orgs/AcceptInvite.tsx` | None | Done (PR #270 — tests in `src/components/orgs/__tests__/AcceptInvite.test.tsx`) |
| 5 | P1 | MEDIUM | Clean Code | Fix hardcoded English in FormSubmitButton | Replace `loadingText = 'Submitting...'` default with i18n key | `src/components/ui/form/form-submit-button.tsx:14` | None | Done (PR #270 — now uses `t('submitting')` i18n key) |
| 6 | P1 | MEDIUM | Performance | Wrap WorkItemForm schema in useMemo | Schema created on every render without memoization | `src/components/work-items/WorkItemForm.tsx:21-24` | None | Done (PR #270 — schema wrapped in useMemo) |
| 7 | P1 | MEDIUM | Clean Code | Fix focus ring classes to match design system | Form components use `focus:ring-primary-100` instead of `focus:ring-primary-600` | `src/components/ui/form/form-field.tsx`, `form-select.tsx`, `form-textarea.tsx` | None | Done (PR #270 — updated to `focus:ring-primary-600`) |
| 8 | P2 | LOW | Security | Remove console.error from production | Replace with dev-only logging utility | `src/components/work-items/WorkItemDetail.tsx:49`, `WorkItemEdit.tsx:36` | None | Not started |
| 9 | P2 | LOW | Test Coverage | Add notifications hook tests | `useNotifications` and related hooks have no coverage | `src/hooks/notifications.ts` | None | Done (PR #270 — tests in `src/hooks/__tests__/notifications.test.ts`) |
| 10 | P2 | LOW | Test Coverage | Add roles utility tests | `isManager` used in permission checks, no tests | `src/lib/roles.ts` | None | Not started |
| 11 | P2 | LOW | Test Coverage | Add dialog-overlay tests | Shared modal with focus trapping, no tests | `src/components/ui/dialog-overlay.tsx` | None | Not started |
| 12 | P2 | LOW | Test Coverage | Add pagination and LanguageSwitcher tests | Shared components without direct tests | `src/components/ui/pagination.tsx`, `src/components/layout/LanguageSwitcher.tsx` | None | Not started |
| 13 | P2 | LOW | Clean Code | Replace type assertions with runtime validation | 5 `as` casts bypass type safety; use Zod `.safeParse()` or type guards | `MembersTable.tsx:89`, `WorkItemEdit.tsx:87`, `SmartFill.tsx:27`, `AcceptInvite.tsx:89` | None | Not started |
| 14 | P2 | LOW | Clean Code | Remove form-demo.tsx from production | Development artifact in production source tree | `src/components/ui/form/form-demo.tsx` | None | Done (file already removed from codebase) |
| 15 | P3 | LOW | Performance | Replace external font with self-hosted | Use `next/font` or inline SVGs to eliminate external request | `src/components/billing/PlanCard.tsx` | None | Not started |
| 16 | P3 | LOW | Performance | Add dynamic imports for heavy components | Use `next/dynamic` for `BillingManagement`, `PricingContent`, `AuditLogTable` | Page files in `src/app/[locale]/` | None | Not started |
| 17 | P3 | LOW | Clean Code | Decompose MembersManager | Extract `useMembersState` hook and split dialog into standalone file | `src/components/orgs/MembersManager.tsx` | None | Not started |
| 18 | P3 | LOW | Clean Code | Add i18n to StatusBadge and WorkItemsTable | Status strings and column headers render in English only | `src/components/work-items/StatusBadge.tsx`, `WorkItemsTable.tsx` | None | Not started |
| 19 | P3 | LOW | Clean Code | Standardize dialog overlay to semantic token | Replace `bg-slate-900/40` with CSS variable | `src/components/ui/dialog-overlay.tsx` | None | Not started |
| 20 | P3 | LOW | Test Coverage | Add auth-events and RoleBadge tests | Small utility and component with no coverage | `src/lib/auth-events.ts`, `src/components/orgs/RoleBadge.tsx` | None | Not started |
| 21 | P3 | LOW | Performance | Audit for next/image usage | Verify all raster images use `next/image` for optimization | All component files | None | Not started |
| 22 | P3 | LOW | Security | Add runtime validation for error.body | Replace `as` assertion with Zod safeParse | `src/components/orgs/AcceptInvite.tsx:89` | None | Not started |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
cd apps/web && npm run build && npm test
```
