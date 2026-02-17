# Walkthrough: 0010 - Standardize Loading States and Skeleton Components

## Task Reference
- Task: `docs/tasks/0010-loading-skeletons.md`
- Walkthrough: `docs/walkthroughs/0010-loading-skeletons.md`
- Branch/PR: `main`
- Date: `2026-02-06`

## Summary
Created reusable skeleton loading primitives (`Skeleton`, `SkeletonText`, `SkeletonTable`) and added `loading.tsx` files for every route segment. Replaced ad-hoc skeleton markup in `MembersManager` and `mine/page.tsx` with the new primitives. Skeletons visually match the actual page layouts to minimize content shift.

## Context
- Background: Some pages had ad-hoc skeleton loading states (inline `animate-pulse` divs), others showed nothing or simple text during loading. No consistency across the app.
- Problem statement: A professional SaaS needs consistent, polished loading states that match page layouts.
- Constraints: Must use Tailwind `animate-pulse` with `bg-slate-200` to match existing patterns.

## Decisions & Trade-offs
- **Decision: Three skeleton primitives instead of more granular components**
  - Options considered: Many primitives (SkeletonAvatar, SkeletonButton, SkeletonCard, etc.), fewer flexible primitives
  - Why this choice: `Skeleton` (base div), `SkeletonText` (multi-line text), and `SkeletonTable` (table with header/rows) cover all use cases in the app. Additional primitives can be added later. Less is more for maintainability.

- **Decision: Server component loading pages**
  - Options considered: Client components with `'use client'`, server components
  - Why this choice: `loading.tsx` files in Next.js App Router are server components by default. The skeleton primitives have no client-side interactivity, so server rendering is appropriate and faster.

- **Decision: Match actual page layout structure in skeletons**
  - Options considered: Simple placeholder blocks, layout-matching skeletons
  - Why this choice: Layout-matching skeletons minimize content layout shift (CLS) when the actual content loads, providing a smoother UX.

## Implementation Notes
- Key changes:
  - Created `Skeleton` (base pulsing div with configurable className), `SkeletonText` (multi-line text with configurable widths), `SkeletonTable` (table with header row and data rows)
  - Created `loading.tsx` for: login (card with 2 fields), register (card with 3 fields), orgs/mine (header + grid of 4 org cards), orgs/[orgId]/members (header + table + invite form + actions card), orgs/accept-invite (centered title + description)
  - Replaced ad-hoc skeleton divs in `MembersManager.tsx` with `Skeleton` and `SkeletonTable` primitives that match the actual component layout
  - Replaced ad-hoc skeleton divs in `mine/page.tsx` with `Skeleton` primitives that match the actual page layout
- Edge cases: `SkeletonText` automatically makes the last line shorter (w-2/3) when `lines > 1` for a natural text appearance
- Known limitations: Skeleton layouts are static approximations; if the actual page layout changes significantly, skeletons should be updated to match

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npm run build
cd apps/web && npx eslint "src/components/ui/skeleton/skeleton.tsx" "src/components/ui/skeleton/index.ts" "src/app/[locale]/login/loading.tsx" "src/app/[locale]/register/loading.tsx" "src/app/[locale]/orgs/mine/loading.tsx" "src/app/[locale]/orgs/mine/page.tsx" "src/app/[locale]/orgs/[orgId]/members/loading.tsx" "src/app/[locale]/orgs/accept-invite/loading.tsx" "src/components/orgs/MembersManager.tsx"
```

## Files Changed
- `apps/web/src/components/ui/skeleton/skeleton.tsx` — Created Skeleton, SkeletonText, SkeletonTable primitives
- `apps/web/src/components/ui/skeleton/index.ts` — Barrel export for skeleton components
- `apps/web/src/app/[locale]/login/loading.tsx` — Login page skeleton (card with 2 fields + button)
- `apps/web/src/app/[locale]/register/loading.tsx` — Register page skeleton (card with 3 fields + button)
- `apps/web/src/app/[locale]/orgs/mine/loading.tsx` — My Orgs page skeleton (header + 4 org cards grid)
- `apps/web/src/app/[locale]/orgs/[orgId]/members/loading.tsx` — Members page skeleton (header + table + invite/actions)
- `apps/web/src/app/[locale]/orgs/accept-invite/loading.tsx` — Accept invite skeleton (centered title + description)
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` — Replaced ad-hoc skeleton divs with Skeleton primitives
- `apps/web/src/components/orgs/MembersManager.tsx` — Replaced ad-hoc skeleton divs with Skeleton + SkeletonTable primitives
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0010 status
- `docs/tasks/0010-loading-skeletons.md` — Marked DoD checklist complete

## Tests
### Unit
- What was added/updated: No unit tests in this task (deferred to task 0014)
- How to run: `cd apps/web && npm test`

### Manual
- Verified: `npm run build` succeeds with all routes generating correctly
- Verified: ESLint passes with no new errors

## Observability
- No changes

## Security
- No security impact — skeleton components are purely presentational

## Follow-ups / Backlog
- [ ] Unit tests for Skeleton components (task 0014)

## Checklist
- [x] Task scope matches `docs/tasks/0010-loading-skeletons.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
