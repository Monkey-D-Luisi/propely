# Code Review: cr-0039 — PR #236 epic-003 audit remediations

## PR Metadata
- PR: #236 (`fix/audit-0013-epic-003-remediations`)
- Target branch: `main`
- CI Status: All checks pass (Detect Changes: SUCCESS, AI API: SUCCESS, Web: SUCCESS, Orgs API: SKIPPED)

## Changed Files
- docs/audits/epic-003-executive-summary.md
- docs/tasks/0032-tenant-id-entities.md
- docs/walkthroughs/audit-0013-* through audit-0017-*
- services/ai-api/src/.../Domain/Common/Exceptions/TenantMismatchException.cs (new)
- services/ai-api/src/.../Infrastructure/Persistence/AppDbContext.cs
- services/ai-api/src/.../Infrastructure/Persistence/Migrations/20260209105314_AddOrgIdToWorkItems.cs
- services/ai-api/tests/.../IntegrationTests/Persistence/TenantQueryFilterTests.cs
- apps/web/src/components/orgs/__tests__/OrgSettingsForm.test.tsx
- apps/web/src/app/[locale]/orgs/mine/__tests__/page.test.tsx (new)

## Review Comments (7 inline, 2 reviews, 2 issue comments)

### Inline Comments

| # | Source | File | Classification | Summary |
|---|--------|------|---------------|---------|
| 1 | gemini-code-assist | TenantQueryFilterTests.cs:164 | MUST_FIX | Test name says ShouldThrowInvalidOperationException but asserts TenantMismatchException |
| 2 | Copilot | epic-003-executive-summary.md:126 | SHOULD_FIX | Test Coverage section still lists tests as missing but PR adds them |
| 3 | Copilot | OrgSettingsForm.test.tsx:134 | SHOULD_FIX | Comment says "toast" but assertion checks FormError alert |
| 4 | Copilot | TenantQueryFilterTests.cs:164 | MUST_FIX | Same as #1 — test name inconsistency |
| 5 | Copilot | audit-0013 walkthrough:11 | SHOULD_FIX | Summary still says InvalidOperationException |
| 6 | Copilot | audit-0014 walkthrough:29 | SHOULD_FIX | Test reference uses old name |
| 7 | Copilot | epic-003-executive-summary.md:48 | SHOULD_FIX | Findings describe old behavior in present tense |

### Review Bodies
- gemini-code-assist: One suggestion for test name consistency. Overall positive.
- copilot: Summary of 6 inline comments.

### Issue Comments
- chatgpt-codex-connector: Usage limit notice (not actionable)
- gemini-code-assist: PR summary (not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] **#1/#4 — Test name inconsistency**: Rename `SaveChanges_WithMismatchedOrgId_ShouldThrowInvalidOperationException` to `SaveChanges_WithMismatchedOrgId_ShouldThrowTenantMismatchException`

### SHOULD_FIX
- [x] **#2 — Test coverage section**: Update table to reflect added tests, update Missing Tests section to note they are now covered
- [x] **#3 — Comment in OrgSettingsForm test**: Fix comment from "toast" to "form error alert"
- [x] **#5 — audit-0013 walkthrough**: Update exception references from InvalidOperationException to TenantMismatchException
- [x] **#6 — audit-0014 walkthrough**: Update test name reference
- [x] **#7 — Executive summary findings wording**: Update F1 and F2 to past tense noting remediation

## Parity Verification Checklist
- [x] Redirect parity checked — no redirect changes in this PR
- [x] Locale source correctness checked — no locale changes in this PR
- [x] API/UI contract parity checked — TenantMismatchException returns 403 (was 500); no frontend impact (edge case)
- [x] Test parity checked — cross-tenant test and frontend 409 tests added
