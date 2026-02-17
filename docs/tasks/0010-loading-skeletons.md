# Task: 0010 - Standardize Loading States and Skeleton Components

## Metadata
- ID: 0010
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0010-loading-skeletons.md`

## Goal
Create reusable skeleton primitives and add `loading.tsx` files for each route segment to provide a consistent, polished loading experience across the entire application.

## Context
Some pages have ad-hoc loading states (e.g., MembersManager has skeleton divs), but there's no consistency. Some pages show nothing during loading. Next.js App Router supports `loading.tsx` files that automatically show while the page content loads via Suspense.

## Scope
### In scope
- Create Skeleton primitive components (SkeletonText, SkeletonBlock, SkeletonAvatar, SkeletonTable)
- Create `loading.tsx` for each route: login, register, orgs/mine, orgs/[orgId]/members, orgs/accept-invite
- Replace ad-hoc skeleton markup in MembersManager with Skeleton primitives
- Ensure skeletons match the layout of the actual content (same widths, heights, spacing)

### Out of scope
- Data fetching changes
- Suspense boundaries for individual components (just page-level loading)

## Requirements
- R1: Skeleton components use Tailwind `animate-pulse` with consistent `bg-slate-200` color
- R2: Each route has a `loading.tsx` that visually matches the page structure
- R3: Skeleton components accept size props (width, height, className)
- R4: SkeletonTable accepts row count and column count

## Acceptance Criteria
- AC1: Navigating between pages shows skeleton loading states
- AC2: Skeleton layout matches actual page layout (no jarring shift when content loads)
- AC3: All Skeleton components are reusable primitives in `components/ui/skeleton/`
- AC4: MembersManager uses Skeleton primitives instead of raw divs
- AC5: `npm run build` succeeds
- AC6: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `apps/web/src/components/ui/skeleton/skeleton.tsx` with primitives:
   - `Skeleton` - base: renders a pulsing div with configurable dimensions
   - `SkeletonText` - text line skeleton with configurable width
   - `SkeletonTable` - table skeleton with configurable rows/cols
2. Create `loading.tsx` for each route segment
3. Replace ad-hoc skeleton markup in MembersManager
4. Run build and verify

## Files to Create / Modify
- `apps/web/src/components/ui/skeleton/skeleton.tsx` (create)
- `apps/web/src/components/ui/skeleton/index.ts` (create)
- `apps/web/src/app/[locale]/login/loading.tsx` (create)
- `apps/web/src/app/[locale]/register/loading.tsx` (create)
- `apps/web/src/app/[locale]/orgs/mine/loading.tsx` (create)
- `apps/web/src/app/[locale]/orgs/[orgId]/members/loading.tsx` (create)
- `apps/web/src/app/[locale]/orgs/accept-invite/loading.tsx` (create)
- `apps/web/src/components/orgs/MembersManager.tsx` (modify - use Skeleton primitives)

## Testing Plan
- Unit tests: Skeleton component tests in task 0014
- Manual verification: Navigate between pages and observe loading states

## Security & Privacy
- No security impact

## Observability
- No changes

## Rollback Plan
Remove loading.tsx files and skeleton components. MembersManager reverts to inline divs.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
