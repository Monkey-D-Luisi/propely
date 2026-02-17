# Task: 0011 - Accessibility Audit and Improvements

## Metadata
- ID: 0011
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0011-accessibility.md`

## Goal
Conduct an accessibility audit of all pages and fix issues. Add skip-to-content link, focus management, ARIA live regions for toasts, proper keyboard navigation for modals, and correct heading hierarchy.

## Context
The app has basic accessibility (semantic HTML, some ARIA labels) but lacks professional-grade a11y features. Modals don't trap focus, toasts aren't announced to screen readers, and there's no skip navigation link.

## Scope
### In scope
- Add skip-to-content link in layout
- Focus trap in modals (LeaveOrgButton dialog)
- ARIA live region for toast notifications
- Focus management on route transitions
- Correct heading hierarchy (h1 > h2 > h3) on all pages
- Keyboard navigation: Escape to close modals, Enter to submit forms
- Color contrast verification (Tailwind slate palette)
- Proper `role` attributes where needed

### Out of scope
- WCAG AAA compliance (target AA)
- Screen reader testing (manual)
- Automated a11y testing (could be a future task)

## Requirements
- R1: All modals trap focus and release on close
- R2: Toast notifications use `role="status"` or `aria-live="polite"`
- R3: Skip-to-content link is visible on focus
- R4: Every page has exactly one `<h1>`
- R5: All interactive elements are keyboard accessible
- R6: Focus returns to trigger element when modal closes

## Acceptance Criteria
- AC1: Tab key navigates through all interactive elements in logical order
- AC2: Escape key closes open modals
- AC3: Skip-to-content link is visible on first Tab press
- AC4: Toast notifications are announced by screen readers
- AC5: Heading hierarchy is correct on every page
- AC6: `npm run build` succeeds
- AC7: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Add skip-to-content link in root layout, target `<main id="main-content">`
2. Add `id="main-content"` to main content area of each page
3. Add focus trap to LeaveOrgButton modal (use a small `useFocusTrap` hook or inline logic)
4. Add `role="status"` and `aria-live="polite"` to toast container
5. Add `aria-modal="true"` and `role="dialog"` to modal overlays
6. Verify heading hierarchy on all pages (fix any h2 that should be h1, etc.)
7. Add `onKeyDown` handlers for Escape on modals
8. Ensure focus returns to trigger button on modal close
9. Verify color contrast with Tailwind slate palette

## Files to Create / Modify
- `apps/web/src/app/[locale]/layout.tsx` (modify - skip link)
- `apps/web/src/components/ui/toast.tsx` (modify - ARIA live region)
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` (modify - focus trap, keyboard)
- Various page files (modify - heading hierarchy fixes)

## Testing Plan
- Manual verification: Tab through all pages, test with keyboard only
- Automated: Consider adding eslint-plugin-jsx-a11y rules

## Security & Privacy
- No security impact

## Rollback Plan
Revert accessibility additions. No behavioral impact.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
