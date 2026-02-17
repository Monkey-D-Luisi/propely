# CR-0069: Privacy & Terms Pages PR Review

## PR Metadata
- **PR:** #284 — feat(legal): add privacy policy and terms of service pages
- **Branch:** `feat/0068-privacy-terms-pages` → `main`
- **CI Status:** Web Build & Test: SUCCESS, E2E Smoke Tests: FAILURE (pre-existing, unrelated)

## Changed Files
- `apps/web/src/app/[locale]/privacy/page.tsx` (new)
- `apps/web/src/app/[locale]/terms/page.tsx` (new)
- `apps/web/messages/en.json` (modified — added `legal` namespace)
- `apps/web/messages/es.json` (modified — added `legal` namespace)
- `docs/tasks/0068-privacy-terms-pages.md` (modified — status DONE)
- `docs/walkthroughs/0068-privacy-terms-pages.md` (new)
- `docs/backlog/epic-013-presale-hardening.md` (modified — progress update)
- `docs/roadmap-v1.md` (modified — status update)

## Review Sources
- Inline review comments: 8
- General reviews: 2 (gemini-code-assist, Copilot)
- Issue comments: 2 (non-actionable: codex limit msg, gemini summary)

## Comment Resolution Plan

### SHOULD_FIX

- [x] **Hardcoded date '2026-02-14'** (gemini #r2807342423, #r2807342428; copilot #r2807343843, #r2807343850)
  - Move dates to i18n keys (`privacy.lastUpdatedDate`, `terms.lastUpdatedDate`) so the date is managed alongside content.
  - **Rejected Gemini's specific suggestion** to use `new Date()` — legal "last updated" dates must be fixed to when content was actually changed, not the current date.

- [x] **GDPR rights `split(' — ')` pattern is fragile** (gemini #r2807342426; copilot #r2807343854)
  - Split each GDPR right into separate `*Title` and `*Desc` keys (e.g., `gdprAccessTitle` / `gdprAccessDesc`) matching the pattern used elsewhere (e.g., `accountInfoTitle` / `accountInfo`).
  - Update `privacy/page.tsx` to use separate keys instead of runtime string splitting.

### MUST_FIX

- [x] **Spanish `contactEmail` uses non-reserved domain** (copilot #r2807343869)
  - `soporte@ejemplo.com` — `ejemplo.com` is NOT an RFC 2606 reserved domain. Email addresses are functional, not translatable.
  - Fix: Set both locales to `support@example.com`.

### OUT_OF_SCOPE

- [ ] **Contact email domain `.test` instead of `.example`** (copilot #r2807343860, #r2807343869)
  - Rationale: `example.com` is the RFC 2606 standard for placeholder domains in user-facing content. `.test` is a DNS testing TLD used in seed scripts but not appropriate for template content that end users will customize. The current `support@example.com` is the industry-standard placeholder.

## Behavioral Parity Checks

- [x] Redirect parity checked — N/A (no auth flows changed)
- [x] Locale source correctness checked — N/A (no locale-dependent redirects)
- [x] API/UI contract parity checked — N/A (no API changes)
- [x] Test parity checked — N/A (content-only pages, no behavior changes requiring test coverage)
