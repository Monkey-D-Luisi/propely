# Walkthrough: 0011-accessibility

## Task Reference
- Task: `docs/tasks/0011-accessibility.md`
- Walkthrough: `docs/walkthroughs/0011-accessibility.md`
- Branch/PR: `feat/0011-accessibility`
- Date: `2026-02-06`

## Summary
Accessibility audit and improvements across the entire frontend. Added skip-to-content navigation, ARIA live regions for toast notifications, focus trap and keyboard handling for the leave organization dialog, proper `scope` attributes on table headers, `aria-hidden` on decorative elements, and fixed heading hierarchy in error states.

## Context
- Background: The app had basic semantic HTML but lacked professional-grade a11y features required for WCAG AA compliance.
- Problem statement: Modals didn't trap focus, toasts weren't announced to screen readers, no skip navigation, decorative content exposed to assistive technology.
- Constraints: No automated a11y testing suite yet; target WCAG AA, not AAA.

## Decisions & Trade-offs
- **Focus trap approach:**
  - Options considered: (a) Custom inline logic, (b) Separate `useFocusTrap` hook, (c) Library like `focus-trap-react`
  - Why this choice: Inline `DialogOverlay` component with focus trap — only one dialog exists in the app, so a reusable hook would be premature. If more dialogs are added, the `DialogOverlay` pattern can be extracted into a shared component.
  - Consequences / risks: If more modals are added, the focus trap logic needs to be extracted or a library adopted.

- **Skip-to-content link:**
  - Uses `sr-only` + `focus:not-sr-only` to be invisible until focused. Target is the main content wrapper `#main-content` with `tabIndex={-1}` for programmatic focus.
  - String is i18n-aware via `getTranslations` in the server component layout.

- **Toast ARIA approach:**
  - `aria-live="polite"` on the container with `aria-relevant="additions"` — new toasts are announced, removals are not.
  - Individual toasts have `role="status"` for semantic clarity.

## Implementation Notes
- Key changes:
  1. Skip-to-content link in root layout (`layout.tsx`)
  2. `aria-live="polite"` + `role="status"` on toast container and items (`toast.tsx`)
  3. Dialog semantics (`role="dialog"`, `aria-modal`, `aria-labelledby`), focus trap, Escape key handling, backdrop click dismiss, focus return to trigger (`LeaveOrgButton.tsx`)
  4. `scope="col"` on all `<th>` elements (`MembersTable.tsx`)
  5. `aria-hidden="true"` on decorative "!" and "404" elements (`error.tsx`, `not-found.tsx`, `global-error.tsx`)
  6. Heading hierarchy fix: 403 error state uses `<h1>` instead of `<h2>` (`MembersManager.tsx`)
  7. i18n strings: `skipToContent` and `close` added to EN and ES message files

- Edge cases handled:
  - Focus trap handles Shift+Tab wrapping
  - Dialog Escape key blocked during processing state
  - Backdrop click blocked during processing state
  - Focus returned to trigger button when dialog closes (except on redirect after successful leave)

- Known limitations:
  - No automated a11y testing (axe, jest-axe) — could be a future task
  - Route transition focus management not implemented (would require client-side router integration)

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npx next build
cd apps/web && npx vitest run --reporter=verbose
```

## Files Changed
- `apps/web/src/app/[locale]/layout.tsx` — Added skip-to-content link and `id="main-content"` target
- `apps/web/src/components/ui/toast.tsx` — Added `aria-live="polite"`, `role="status"`, `aria-label` on close button
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` — Added dialog semantics, focus trap, Escape key, focus return, backdrop click
- `apps/web/src/components/orgs/MembersTable.tsx` — Added `scope="col"` to table headers
- `apps/web/src/components/orgs/MembersManager.tsx` — Fixed heading hierarchy in 403 error state (h2 → h1)
- `apps/web/src/app/[locale]/error.tsx` — Added `aria-hidden` on decorative "!"
- `apps/web/src/app/[locale]/not-found.tsx` — Added `aria-hidden` on decorative "404"
- `apps/web/src/app/global-error.tsx` — Added `aria-hidden` on decorative "!"
- `apps/web/messages/en.json` — Added `skipToContent` and `close` strings
- `apps/web/messages/es.json` — Added `skipToContent` and `close` strings

## Tests
### Unit
- Existing 78 tests pass (no regressions)
- No new tests added (accessibility changes are primarily HTML attribute additions)

### Manual
- Verified: Build succeeds, no TypeScript errors, no lint errors
- Tab navigation, Escape key, skip link, and focus trap should be verified manually

## Observability
- No logs or metrics changes

## Security
- No security impact

## Follow-ups / Backlog
- [ ] Add automated a11y testing with `jest-axe` or `@axe-core/react`
- [ ] Implement route transition focus management via client-side router hooks
- [ ] Extract `DialogOverlay` into shared component if more modals are added

## Checklist
- [x] Task scope matches `docs/tasks/0011-accessibility.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
