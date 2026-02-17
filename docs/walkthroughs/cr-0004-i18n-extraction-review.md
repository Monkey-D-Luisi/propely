# cr-0004: Code Review - i18n String Extraction

## Goal

Address all review feedback from PR #142 to ensure proper i18n implementation and code quality.

## Changes Made

### MUST_FIX

1. **MembersManager.tsx** - Fixed role badge rendering using `t.rich()`
2. **en.json / es.json** - Updated `yourRole` key to use tag format
3. **Walkthrough** - Moved from `docs/walkthrough/` to `docs/walkthroughs/`

### SHOULD_FIX

4. **Walkthrough links** - Converted file:// URIs to relative paths
5. **Dependency arrays** - Removed stable `t` function from:
   - RegisterForm.tsx useEffect
   - AppHeader.tsx useCallback
   - AcceptInvite.tsx useEffect
   - MembersManager.tsx useCallback
6. **MembersManager.tsx** - Used translated role in success toast
7. **InviteForm.tsx** - Memoized roleOptions array

## Files Changed

- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/components/auth/RegisterForm.tsx`
- `apps/web/src/components/layout/AppHeader.tsx`
- `apps/web/src/components/orgs/AcceptInvite.tsx`
- `apps/web/src/components/orgs/InviteForm.tsx`
- `apps/web/src/components/orgs/MembersManager.tsx`
- `docs/walkthroughs/0005-extract-strings-en.md` (moved from docs/walkthrough/)

## Verification

- [ ] npm run build passes
- [ ] npm run test passes
- [ ] Role badge renders correctly in members view
