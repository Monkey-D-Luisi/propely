# Walkthrough - Task 0005: Extract All Frontend Strings to EN Locale

## Task Reference
- **Task file:** `docs/tasks/0005-extract-strings-en.md`
- **Epic:** `docs/backlog/epic-001-professional-saas-refinement.md`

## Summary
Extracted all hardcoded user-facing strings from 11 frontend components/pages into English message files using next-intl's `useTranslations` hook. Created matching ES placeholder structure for future translation work (Task 0006).

## Key Changes

### Message Files
| File | Description |
|------|-------------|
| `apps/web/messages/en.json` | ~100 strings organized in namespaces: common, auth, orgs, errors |
| `apps/web/messages/es.json` | Matching structure with empty values (placeholder) |

### Updated Components

**Auth:**
- `apps/web/src/components/auth/LoginForm.tsx` - Login form labels, errors, buttons
- `apps/web/src/components/auth/RegisterForm.tsx` - Registration form labels, errors, buttons

**Layout:**
- `apps/web/src/components/layout/AppHeader.tsx` - App name, sign in/out buttons

**Organizations:**
- `apps/web/src/components/orgs/AcceptInvite.tsx` - Invite acceptance statuses, errors
- `apps/web/src/components/orgs/InviteForm.tsx` - Invite form labels, toasts
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` - Leave dialog, confirmations
- `apps/web/src/components/orgs/MembersManager.tsx` - Members page headers, role changes
- `apps/web/src/components/orgs/MembersTable.tsx` - Table headers, role options
- `apps/web/src/components/orgs/RoleBadge.tsx` - Role labels

**Pages:**
- `apps/web/src/app/[locale]/page.tsx` (Home) - Welcome message, app description
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` (My Orgs) - Organization list, create form

## Verification

### Build
```
✓ Compiled successfully in 3.2s
✓ Finished TypeScript in 4.5s
✓ Generating static pages (13/13)
```

### Tests
```
Test Files  1 passed (1)
     Tests  5 passed (5)
```

## Decisions
1. **Namespace organization**: Grouped strings by feature area (common, auth, orgs, errors) for maintainability
2. **Dynamic interpolation**: Used next-intl's interpolation syntax for role-specific messages (e.g., `{role}`, `{email}`)
3. **Empty ES values**: Left ES translation values empty as placeholders - Task 0006 will add actual translations

## Follow-ups
- Task 0006: Add Spanish translations to es.json
