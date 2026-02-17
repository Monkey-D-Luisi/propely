# Walkthrough: audit-0013-extract-dialog-overlay

## Task Reference
- Task: `docs/tasks/audit-0013-extract-dialog-overlay.md`
- Walkthrough: `docs/walkthroughs/audit-0013-extract-dialog-overlay.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Extracted the identical `DialogOverlay` component (backdrop + Escape key + Tab focus trap + auto-focus) from `LeaveOrgButton.tsx`, `DeleteOrgSection.tsx`, and `MembersManager.tsx` into a shared `@/components/ui/dialog-overlay.tsx`. All three consumers now import from the shared location.

## Context
- Background: The same 60-line modal overlay pattern was copy-pasted across three components during Epic 004 implementation.
- Problem statement: DRY violation — any bug fix or enhancement to the overlay pattern would need to be applied in three places.

## Decisions & Trade-offs
- **Decision: Keep same API surface (`dialogRef`, `isProcessing`, `onClose`, `children`)**
  - Why: The three implementations were identical, so no API changes were needed. The shared component is a drop-in replacement.

- **Decision: `MembersManager.tsx` refactored to wrap dialog content in `DialogOverlay`**
  - Why: The `RemoveConfirmDialog` had the overlay inlined rather than using a wrapper pattern. Refactored to match `LeaveOrgButton` and `DeleteOrgSection` which already used the wrapper pattern.

## Files Changed
- `apps/web/src/components/ui/dialog-overlay.tsx` (CREATED) — Shared DialogOverlay component
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` — Removed local DialogOverlay, imported from shared
- `apps/web/src/components/orgs/DeleteOrgSection.tsx` — Removed local DialogOverlay, imported from shared
- `apps/web/src/components/orgs/MembersManager.tsx` — Replaced inline overlay with shared DialogOverlay wrapper

## Tests
- All 289 frontend tests pass (27 files)
- No new tests needed — existing component tests already exercise the overlay behavior through the consumers

## Checklist
- [x] Task scope matches `docs/tasks/audit-0013-extract-dialog-overlay.md`
- [x] Tests updated and passing
- [x] No secrets committed
