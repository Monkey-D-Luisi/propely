# Task: 0052-stitch-design-refinement-pass

## Metadata
- ID: 0052
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-08
- Related docs:
  - Walkthrough: `docs/walkthroughs/0052-stitch-design-refinement-pass.md`
  - Epic: `docs/backlog/epic-P7-polish.md` (Task 7.4)

## Goal
Full design audit and consistency pass across all screens. Fix border radius, focus rings, max-widths, and accessibility deviations from the design system.

## Context
After completing Tasks 7.1-7.3, a comprehensive code-level audit revealed inconsistencies accumulated over multiple implementation phases. The design system rules in CLAUDE.md specify exact patterns for border radius, focus rings, max-widths, and accessibility — but 16 files had deviations.

## Scope
### In scope
- Code-level CSS class audit across all components
- Border radius standardization (cards=`rounded-xl`, buttons=`rounded-lg`)
- Focus ring standardization (`focus:ring-2 focus:ring-primary-600 focus:ring-offset-2`)
- Max-width standardization per page type
- ARIA label fix for delete confirmation input
- Regression test verification

### Out of scope
- Visual browser comparison against Stitch HTML (requires running app)
- Responsive spot-check at 375px/768px (requires running app)
- Stitch design regeneration
- New feature development

## Changes (16 files)

### Border radius fixes
| File | Fix |
|------|-----|
| `AgencySettingsForm.tsx` | 3x `rounded-2xl` → `rounded-xl` (card containers) |
| `NotificationBell.tsx` | `rounded-md` → `rounded-lg` (button) |
| `VerificationBanner.tsx` | `rounded-md` → `rounded-lg` (button) |
| `ViewToggle.tsx` | 2x `rounded-md` → `rounded-lg` (toggle buttons) |
| `LeaveOrgButton.tsx` | 2x `rounded-md` → `rounded-lg` (dialog buttons) + `rounded-lg` → `rounded-xl` (dialog card) |
| `LeadListView.tsx` | `rounded-md` → `rounded-lg` (select) |
| `LeadKanbanCard.tsx` | `rounded-md` → `rounded-lg` (select) |

### Focus ring additions
| File | Elements |
|------|----------|
| `LeaveOrgButton.tsx` | Cancel + confirm dialog buttons |
| `DeleteOrgSection.tsx` | Cancel + confirm dialog buttons |
| `MembersManager.tsx` | Cancel + confirm dialog buttons |
| `DeleteAccountSection.tsx` | Cancel + confirm dialog buttons |
| `AgencySettingsForm.tsx` | Discard + save buttons |
| `OrgSettingsForm.tsx` | Save button |
| `AppointmentForm.tsx` | Cancel + submit buttons |
| `not-found.tsx` | Standardized `focus:ring-4` → `focus:ring-2` |

### Max-width fixes
| File | From | To |
|------|------|----|
| `(dashboard)/page.tsx` | `max-w-5xl` (2x) | `max-w-4xl` |
| `verify-email/page.tsx` | `max-w-lg` | `max-w-md` |
| `accept-invite/loading.tsx` | `max-w-lg` | `max-w-md` |

### Accessibility
| File | Fix |
|------|-----|
| `AgencySettingsForm.tsx` | Added `aria-label` to delete confirmation input |

## Verification
```bash
cd apps/web && npx tsc --noEmit    # Clean
cd apps/web && npm test            # 842 tests pass (0 regressions)
```
