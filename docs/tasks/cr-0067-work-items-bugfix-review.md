# Task: cr-0067-work-items-bugfix-review

## PR Metadata
- **PR**: #266
- **Branch**: `feat/0049-work-items-frontend`
- **Target**: `main`
- **Date**: 2026-02-13
- **Previous CR**: cr-0066 (addressed 17 review comments)

## Context
Second review pass on PR #266. No new inline reviewer comments; this pass addresses CI failure and two runtime bugs discovered during manual testing.

## CI Status
- Detect Changes: SUCCESS
- AI API - Build & Test: SUCCESS
- Orgs API - Build & Test: SKIPPED
- **Web - Build & Test: FAILURE** — TypeScript error in `useParseWorkItem` (missing `return null` in catch block)
- E2E Smoke Tests: SUCCESS

## New Review Comments
No new inline comments from reviewers. Three new general review summaries (Gemini, Codex, Copilot) with no new actionable inline comments.

## Comment Resolution Plan

### MUST_FIX
- [x] **CI TypeScript error**: `hooks/work-items.ts:124` — `useParseWorkItem` catch block lacks `return null`, causing "Function lacks ending return statement" error during `next build`
- [x] **Zod v4 UUID validation**: `schemas.ts:313-314` — `z.string().uuid()` rejects dev auth handler's `00000000-0000-0000-0000-000000000001` (not RFC 4122 compliant). Changed to `z.string().guid()`.

### SHOULD_FIX
- [x] **CQRS eventual consistency on create**: After creating a work item, `router.push('/work-items')` navigates to list before the read model projection completes. Changed to navigate to detail page (`/work-items/${created.id}`) which uses `GetByIdAsync` with write-model fallback.

## Parity Verification Checklist
- [x] Redirect parity checked — `next` propagation not applicable (no auth redirects changed); create-to-detail redirect is correct
- [x] Locale source correctness checked — no locale changes in this pass
- [x] API/UI contract parity checked — no new DTO fields; `.guid()` validation still validates GUID format
- [x] Test parity checked — test updated to expect `null` (not `undefined`) from `useParseWorkItem` on error; 391 tests pass; `next build` succeeds
