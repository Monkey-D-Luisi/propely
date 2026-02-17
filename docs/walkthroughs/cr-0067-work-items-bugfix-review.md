# Walkthrough: cr-0067-work-items-bugfix-review

## Task Reference
- Task: `docs/tasks/cr-0067-work-items-bugfix-review.md`
- Walkthrough: `docs/walkthroughs/cr-0067-work-items-bugfix-review.md`
- PR: #266
- Branch: `feat/0049-work-items-frontend`
- Date: `2026-02-13`

## Summary
Fixed CI TypeScript build failure and two runtime bugs: Zod v4 strict UUID validation rejecting dev auth GUID, and CQRS eventual consistency causing newly created items to not appear in list.

## Changes Made

### MUST_FIX
1. **CI: TypeScript error** — Added `return null;` to `useParseWorkItem` catch block in `hooks/work-items.ts`. The return type `Promise<ParseWorkItemResponse | null>` requires an explicit `null` return on error paths. Updated corresponding test assertion from `toBeUndefined()` to `toBeNull()`.

2. **Zod v4 GUID validation** — Changed `id` and `orgId` in `WorkItemSchema` from `z.string().uuid()` to `z.string().guid()` in `lib/schemas.ts`. Zod v4's `.uuid()` enforces strict RFC 4122 version/variant bits, rejecting the dev auth handler's `00000000-0000-0000-0000-000000000001`. The `.guid()` method accepts any GUID format (8-4-4-4-12 hex with dashes).

### SHOULD_FIX
3. **Create-to-detail redirect** — Changed `new/page.tsx` to navigate to `/work-items/${created.id}` after creating, instead of `/work-items`. The `GetByIdAsync` endpoint has a built-in fallback to the write model, avoiding the CQRS read-model projection delay.

## Commands Run
```bash
cd apps/web && npx next build                  # 0 TypeScript errors
cd apps/web && npx vitest run                  # 391 passed (39 files)
```

## Validation Results
- Next.js build: 0 TypeScript errors, compiled successfully
- Frontend tests: 391 passed across 39 test files
