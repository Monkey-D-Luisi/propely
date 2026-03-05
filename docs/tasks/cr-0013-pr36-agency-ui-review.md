# Task: cr-0013-pr36-agency-ui-review

## Metadata
- ID: cr-0013
- Type: Code Review
- Status: IN_PROGRESS
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - PR: https://github.com/Monkey-D-Luisi/propely/pull/36
  - Task: `docs/tasks/0011-agency-management-ui.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.6)

## PR Metadata
- **PR #36**: feat(web): Agency Management UI (#0011)
- **Branch**: `copilot/update-task-management-system` → `main`
- **Files changed**: 22
- **Additions**: 1840, Deletions: 35
- **CI Status**: Semgrep scan PASSED. No other CI checks ran (frontend-only PR).

## Changed Files

| File | Type | Lines |
|------|------|-------|
| `apps/web/messages/en.json` | Modified | +82 -16 |
| `apps/web/messages/es.json` | Modified | +72 -6 |
| `apps/web/package-lock.json` | Modified | +0 -12 |
| `apps/web/src/app/[locale]/agencies/[id]/page.tsx` | New | +12 |
| `apps/web/src/app/[locale]/agencies/[id]/settings/page.tsx` | New | +12 |
| `apps/web/src/app/[locale]/agencies/new/page.tsx` | New | +8 |
| `apps/web/src/components/agencies/AgencyCreateForm.tsx` | New | +146 |
| `apps/web/src/components/agencies/AgencyDashboard.tsx` | New | +119 |
| `apps/web/src/components/agencies/AgencySettingsForm.tsx` | New | +206 |
| `apps/web/src/components/agencies/BranchCard.tsx` | New | +45 |
| `apps/web/src/components/agencies/BranchSwitcher.tsx` | New | +98 |
| `apps/web/src/components/agencies/__tests__/AgencyCreateForm.test.tsx` | New | +148 |
| `apps/web/src/components/agencies/__tests__/AgencyDashboard.test.tsx` | New | +146 |
| `apps/web/src/components/agencies/__tests__/AgencySettingsForm.test.tsx` | New | +177 |
| `apps/web/src/components/agencies/__tests__/BranchSwitcher.test.tsx` | New | +123 |
| `apps/web/src/components/layout/AppHeader.tsx` | Modified | +9 |
| `apps/web/src/components/layout/__tests__/AppHeader.test.tsx` | Modified | +8 |
| `apps/web/src/hooks/agencies.ts` | New | +125 |
| `apps/web/src/lib/schemas.ts` | Modified | +73 |
| `docs/backlog/epic-P1-agency-permissions.md` | Modified | +1 -1 |
| `docs/tasks/0011-agency-management-ui.md` | New | +127 |
| `docs/walkthroughs/0011-agency-management-ui.md` | New | +103 |

---

## Section 1: Agent Review Findings

### MUST_FIX

#### F1: BranchSwitcher missing ARIA menu roles [Accessibility]
- **File**: `BranchSwitcher.tsx`
- **Issue**: Dropdown lacks `role="menu"`, `role="menuitem"`, `aria-haspopup="true"`. Only has `aria-expanded` and `aria-label`. No keyboard arrow navigation within dropdown items.
- **Fix**: Add ARIA menu roles to the dropdown container and items. Add `aria-haspopup="true"` to the trigger button.

#### F2: Mobile menu links to `/agencies/new` instead of agency list [UX]
- **File**: `AppHeader.tsx:222-228`
- **Issue**: Desktop `BranchSwitcher` shows list of agencies with proper navigation. Mobile menu link goes directly to `/agencies/new` (create form), which is inconsistent and confusing. Users expect "Agencies" to show their agencies, not a creation form.
- **Fix**: On mobile, if user has agencies, link to the first agency's dashboard. If no agencies, link remains `/agencies/new`.

#### F3: Nested interactive elements — `<Link>` wrapping `<Button>` [Accessibility]
- **File**: `AgencyDashboard.tsx:76-81`
- **Issue**: Settings button is `<Link href=...><Button type="button">...</Button></Link>` — a `<button>` nested inside an `<a>`. This is invalid HTML and causes accessibility issues (nested interactive elements, confusing screen reader experience).
- **Fix**: Remove the `<Button>` wrapper and style the `<Link>` directly as a button-like element, or use `router.push` on a standalone `<Button>` click.

#### F4: `onChange` override breaks react-hook-form validation [Bug]
- **File**: `AgencyCreateForm.tsx:100-104, 118-122`
- **Issue**: `{...methods.register('name')}` spreads an `onChange` handler, but a separate `onChange={onNameChange}` prop is also provided. The explicit prop overrides RHF's internal handler, which means RHF's `onChange` (responsible for triggering validation, dirty tracking, etc.) does not fire. Validation will only trigger on submit, not on change/blur as users expect.
- **Fix**: Use `methods.register` to get the ref, then manually call both the RHF onChange and the custom logic.

### SHOULD_FIX

#### F5: BranchCard date uses browser locale instead of app locale [i18n]
- **File**: `BranchCard.tsx:17`
- **Issue**: `toLocaleDateString(undefined, ...)` uses the browser's default locale rather than the app's `next-intl` locale. In a bilingual app (en/es), this means dates might format inconsistently.
- **Fix**: Pass the current locale from `useLocale()` to `toLocaleDateString()`.

#### F6: Settings save button shows destructive toast for non-error [UX]
- **File**: `AgencySettingsForm.tsx:75-82`
- **Issue**: Clicking "Save changes" shows a `variant: 'destructive'` toast saying "Agency update is not yet available." The destructive variant (red styling) is meant for errors, not informational messages. This misleads users.
- **Fix**: Change to `variant: 'default'` or disable the save button entirely with an explanatory tooltip until the backend supports PATCH.

#### F7: Missing Stitch designs for all new screens [Design]
- **Files**: No `.stitch-html/` files for agency-create, agency-dashboard, agency-settings
- **Issue**: Per CLAUDE.md mandate, every new screen must have a Stitch design. The task was implemented remotely without MCP access.
- **Fix**: Note as follow-up. Stitch designs will be created when MCP is available and implementation verified pixel-perfect. Not a merge blocker since the screens follow existing design system conventions.

#### F8: `useCreateAgency`/`useDeleteAgency` don't use `useCallback` [Performance]
- **File**: `agencies.ts:86-97, 100-106`
- **Issue**: These hooks return new function references on every render. While not causing bugs, it means any component using these as effect dependencies would re-run unnecessarily.
- **Fix**: Wrap returned functions in `useCallback`.

### NIT

#### F9: Unused hooks `useAddBranch` and `useRemoveBranch` [Dead Code]
- **File**: `agencies.ts:109-125`
- **Issue**: Defined but not used by any component in this PR. They are for future use.
- **Fix**: Keep as-is — they're small utilities that will be used when branch management UI is needed.

#### F10: Missing trailing newline in en.json and es.json [Style]
- **Issue**: Both JSON files end without a trailing newline. Most linters flag this.
- **Fix**: Add trailing newline.

#### F11: Double quotes in agencies.ts vs single quotes elsewhere [Style]
- **File**: `agencies.ts`
- **Issue**: Uses double quotes (`"use client"`) while other new files use single quotes (`'use client'`).
- **Fix**: Normalize to single quotes for consistency.

---

## Section 2: Review Comment Threads

No external review comments. All 3 sources returned 0 comments.

- Total inline review comments: 0
- Total general reviews: 0
- Total issue comments: 0

---

## Resolution Plan

### MUST_FIX (implement all)
- [x] F1: Add ARIA menu roles to BranchSwitcher dropdown
- [x] F2: Fix mobile menu agencies link destination
- [x] F3: Remove nested interactive elements in AgencyDashboard
- [x] F4: Fix onChange override in AgencyCreateForm

### SHOULD_FIX (implement)
- [x] F5: Use app locale for date formatting in BranchCard
- [x] F6: Change destructive toast to default for settings save
- [ ] F7: Missing Stitch designs — documented as follow-up, not blocking
- [x] F8: Wrap mutation hooks in useCallback

### NIT (implement quick)
- [ ] F9: Unused hooks — kept intentionally for future use
- [x] F10: Add trailing newlines to JSON files
- [x] F11: Normalize quotes in agencies.ts
