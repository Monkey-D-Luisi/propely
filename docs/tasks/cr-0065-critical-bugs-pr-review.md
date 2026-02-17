# cr-0065 — PR #265 Code Review (ft-0005 Critical Bugs)

## PR Metadata

- **PR**: [#265](https://github.com/Monkey-D-Luisi/saas-template/pull/265)
- **Branch**: `fix/ft-0005-critical-bugs` → `main`
- **CI Status**: Web - Build & Test FAILED (csrf.test.ts), Orgs API passed, E2E passed

## Changed Files

1. `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Services/CsrfValidator.cs`
2. `apps/web/src/lib/csrf.ts`
3. `apps/web/src/lib/__tests__/csrf.test.ts` (needs update)
4. `apps/web/src/components/orgs/AcceptInvite.tsx`
5. `apps/web/src/components/layout/AppHeader.tsx`
6. `apps/web/src/app/[locale]/[...rest]/page.tsx`
7. `apps/web/src/app/not-found.tsx`
8. `apps/web/src/app/[locale]/page.tsx`
9. `apps/web/src/components/billing/PricingContent.tsx`
10. `apps/web/src/components/billing/PlanCard.tsx`
11. `apps/web/messages/en.json`
12. `apps/web/messages/es.json`
13. `docs/walkthroughs/ft-0005-critical-bugs.md`

## Review Threads (19 inline + 3 reviews + 1 issue comment)

### Comment Resolution Plan

#### MUST_FIX

- [x] **CI: csrf.test.ts failing** — Test uses `'cached-token'` which doesn't match `isTokenFresh()` format (`{hex-timestamp}.{random}`). Update test to use valid token format.
- [x] **not-found.tsx: nested `<html>/<body>` tags** (chatgpt-codex #2802950346) — Root not-found renders inside layout tree; remove document shell.
- [x] **not-found.tsx: hardcoded `#f6f6f8`** (Copilot #2802983608) — Use `bg-surface` semantic token.
- [x] **not-found.tsx: hardcoded `#5048e5`** (Copilot #2802983515) — Use `bg-primary-600` semantic token.
- [x] **Walkthrough inaccuracy** (Copilot #2802983541) — Docs say "cookie optional, must match if present" but code ignores cookie entirely. Correct the description.

#### SHOULD_FIX

- [x] **Footer links: use `<Link>` for internal** (gemini #2802940253) — Privacy/Terms → `<Link>`, GitHub/Twitter → `<a target="_blank">`.
- [x] **Enterprise CTA behavior** (chatgpt-codex #2802950350) — Enterprise "Contact Sales" goes through `startCheckout`; branch to mailto or placeholder.
- [x] **Localize fallback feature strings** (Copilot #2802983450) — Move hardcoded English feature strings to translation files.
- [x] **Trusted-by semantic markup** (Copilot #2802983464) — Use `<ul>/<li>` instead of `<div>` for logo list.
- [x] **Remove non-functional billing toggle** (Copilot #2802983479) — Toggle with `aria-checked="false"` is a11y violation; remove until backend supports billing periods.

#### SUGGESTION (implement trivial)

- [x] **DRY starter/free planMeta** (Copilot #2802983578) — Share config between starter and free keys.
- [x] **Extract TrustedBy shared component** (Copilot #2802983494) — Duplicated between landing + pricing; extract to shared component.

#### SUGGESTION (defer)

- [ ] **PlanCard button duplication** (Copilot #2802983428) — Loading `"..."` text is intentionally simple. Minor duplication between 2 ternary branches. Not worth extracting. **DEFERRED**.
- [ ] **Large inline SVG** (Copilot #2802983663) — Dashboard mockup SVG is self-contained and only used once. Extracting to a separate file adds indirection without meaningful benefit. **DEFERRED**.
- [ ] **`t` in useEffect deps** (Copilot #2802983642) — Harmless to include; follows React best practice. **NO CHANGE**.
- [ ] **GitHub link placeholder** (Copilot #2802983590) — Placeholder by design. Will be set when repo goes public. **NO CHANGE**.

#### QUESTION (respond)

- [x] **CSRF security: removing cookie check** (Copilot #2802983554 + #2802983561) — CORS + custom header IS sufficient CSRF protection. The browser enforces that only allowed origins can send cross-origin requests with credentials. `AllowAnyHeader()` means the server ACCEPTS headers, but CORS preflight still restricts which origins can make the request. XSS on allowed origin makes all CSRF defenses moot. The double-submit cookie was failing due to cross-origin cookie blocking.
- [x] **Cookie mirroring concern** (Copilot #2802983624) — The client-side cookie is intentionally a local cache on the frontend domain. The backend only checks the `x-csrf-token` header, not the cookie. This is working as designed.
- [x] **Footer links "#" UX** (Copilot #2802983527) — Addressed by converting to `<Link>` for internal / proper `<a>` for external. Overlaps with gemini comment.

## Parity Verification Checklist

- [x] Redirect parity checked — `next` propagation in AcceptInvite.tsx is consistent (encodeURIComponent → login redirect)
- [x] Locale source correctness checked — locale from `useLocale()` passed to `startCheckout`; translations use `useTranslations()`
- [x] API/UI contract parity checked — no new API fields; fallback plans are frontend-only static data
- [x] Test parity checked — csrf.test.ts needs update for `isTokenFresh()` behavior change (addressed above)
