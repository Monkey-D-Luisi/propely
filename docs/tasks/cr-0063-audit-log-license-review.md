# Code Review: cr-0063 — Audit Log + License Review

## PR Metadata
- PR: #263 (`feat(admin): add LICENSE/EULA and audit log viewer (#0059, #0048)`)
- Branch: `feat/0059-license-eula` → `main`
- CI Status: Orgs API Build & Test ✅, E2E Smoke ✅, Web Build & Test ❌ (coverage threshold)

## Changed Files
- `EULA.md` — Full EULA
- `LICENSE` — Short-form license
- `apps/web/messages/en.json`, `es.json` — audit log i18n
- `apps/web/src/app/[locale]/admin/audit-logs/page.tsx` — Admin page
- `apps/web/src/components/admin/AuditLogFilters.tsx` — Filter controls
- `apps/web/src/components/admin/AuditLogTable.tsx` — Table with expandable rows
- `apps/web/src/components/layout/AppHeader.tsx` — Nav link
- `apps/web/src/hooks/audit-logs.ts` — Data fetching hooks
- `apps/web/src/lib/schemas.ts` — Zod schemas
- `docs/backlog/epic-007-admin-dashboards.md` — Progress update
- `docs/backlog/epic-011-licensing.md` — Progress update
- `docs/roadmap-v1.md` — Status update
- `docs/tasks/0048-audit-log-ui.md`, `0059-license-eula.md` — DONE
- `docs/walkthroughs/0048-audit-log-ui.md`, `0059-license-eula.md` — New
- `services/orgs-api/src/.../Api/Controllers/AuditLogsController.cs` — REST endpoints
- `services/orgs-api/src/.../Application/AuditLogs/**` — DTOs, interfaces, queries
- `services/orgs-api/src/.../Infrastructure/Persistence/Repositories/AuditLogRepository.cs`
- `services/orgs-api/src/.../Infrastructure/DependencyInjection.cs`
- `tests/.../AuditLogs/Queries/*.cs` — 8 unit tests

## Review Threads

### Source 1: Inline Review Comments (5)
1. Copilot @ `docs/walkthroughs/0059-license-eula.md:15` — "Not a legal document" phrasing
2. Copilot @ `docs/walkthroughs/0059-license-eula.md:71` — Tests checklist inconsistency
3. Copilot @ `EULA.md:54` — Section 4 per-seat vs per-team ambiguity
4. Copilot @ `EULA.md:106` — Termination "destroy all copies" vs deployed End Products
5. Gemini @ `EULA.md:54` — Same as #3, team/org use terms clarity

### Source 2: General Reviews (2)
1. Copilot (COMMENTED) — Overview summary, no extra issues
2. Gemini (COMMENTED) — Summary + 1 suggestion (covered by inline #5)

### Source 3: Issue Comments (2)
1. ChatGPT Codex — Usage limit notice (not actionable)
2. Gemini — PR summary (not actionable)

## Comment Verification
- [x] Total inline review comments: 5
- [x] Total general reviews: 2
- [x] Total issue comments: 2
- [x] All comments read and analyzed

## Parity Checks
- [x] Redirect parity: N/A (no auth flows changed)
- [x] Locale source correctness: N/A (only i18n string additions)
- [x] API/UI contract parity: Verified — frontend query params match backend exactly
- [x] Test parity: FAIL — new frontend code has 0% coverage, pulling global below 60% threshold

## Comment Resolution Plan

### MUST_FIX
- [x] **MF-1**: CI coverage failure — add frontend tests for AuditLogTable, AuditLogFilters, audit-logs hook to restore coverage above 60%

### SHOULD_FIX
- [x] **SF-1**: EULA Section 4 — clarify per-seat licensing model (addresses Copilot #3 + Gemini #5)
- [x] **SF-2**: EULA Section 10 — tighten termination wording re: destroy vs retain for deployed products (Copilot #4)
- [x] **SF-3**: Walkthrough 0059 line 15 — change "Not a legal document" to "not legal advice" (Copilot #1)
- [x] **SF-4**: Walkthrough 0059 line 71 — change "Tests updated and passing" to "Docs verified" (Copilot #2)
