# cr-0004: Code Review - i18n String Extraction PR #142

## PR Metadata

- **PR:** [#142](https://github.com/Monkey-D-Luisi/saas-template/pull/142)
- **Title:** feat(i18n): extract frontend strings to EN locale files
- **Branch:** feature/0005-extract-strings-en → main
- **CI Status:** All checks passing

## Changed Files

| File | Changes |
|------|---------|
| apps/web/messages/en.json | +163/-5 |
| apps/web/messages/es.json | +166/-8 |
| apps/web/src/app/[locale]/orgs/mine/page.tsx | +13/-10 |
| apps/web/src/app/[locale]/page.tsx | +9/-4 |
| apps/web/src/components/auth/LoginForm.tsx | +13/-11 |
| apps/web/src/components/auth/RegisterForm.tsx | +18/-16 |
| apps/web/src/components/layout/AppHeader.tsx | +13/-10 |
| apps/web/src/components/orgs/AcceptInvite.tsx | +24/-21 |
| apps/web/src/components/orgs/InviteForm.tsx | +21/-19 |
| apps/web/src/components/orgs/LeaveOrgButton.tsx | +14/-11 |
| apps/web/src/components/orgs/MembersManager.tsx | +25/-21 |
| apps/web/src/components/orgs/MembersTable.tsx | +17/-22 |
| apps/web/src/components/orgs/RoleBadge.tsx | +4/-8 |
| docs/walkthrough/0005-extract-strings-en.md | +60/-0 |

## Review Comments Summary

- **Inline review comments:** 13
- **General reviews:** 3
- **Issue comments:** 2

## Comment Resolution Plan

### MUST_FIX (Blocking)

- [ ] **MembersManager.tsx:144** - Use `t.rich()` for embedding RoleBadge component
  - Reported by: gemini-code-assist, chatgpt-codex, Copilot
  - Current code renders "Your current role is . Owner" (malformed)
  - Solution: Use `t.rich('members.yourRole', { roleBadge: () => <RoleBadge ... /> })`

- [ ] **en.json:70** - Update translation key to use tag instead of variable
  - Reported by: gemini-code-assist
  - Change from: `"yourRole": "Your current role is {role}."`
  - Change to: `"yourRole": "Your current role is <roleBadge>."`

- [ ] **es.json** - Apply same tag format change for yourRole key

- [ ] **Walkthrough location** - Move from `docs/walkthrough/` to `docs/walkthroughs/`
  - Reported by: Copilot
  - Project standard requires plural `walkthroughs/` directory

### SHOULD_FIX (Maintainability)

- [ ] **Walkthrough links** - Convert `file:///` URIs to relative paths
  - Reported by: gemini-code-assist, Copilot
  - Use relative paths like `../tasks/0005-extract-strings-en.md`

- [ ] **RegisterForm.tsx:41** - Remove `t` from useEffect dependency array
  - Reported by: gemini-code-assist
  - `t` from useTranslations is stable, doesn't need to be in deps

- [ ] **AppHeader.tsx:65** - Remove `tAuth` from useCallback dependency array
  - Reported by: gemini-code-assist

- [ ] **AcceptInvite.tsx:116** - Remove `t` from useEffect dependency array
  - Reported by: gemini-code-assist

- [ ] **MembersManager.tsx:99** - Remove `t` from useCallback dependency array
  - Reported by: gemini-code-assist

- [ ] **MembersManager.tsx:63** - Use translated role in success toast
  - Reported by: gemini-code-assist
  - Change to: `t('roles.${nextRole}')` instead of raw role value

- [ ] **InviteForm.tsx:33** - Memoize roleOptions array
  - Reported by: Copilot
  - Use `useMemo` to avoid recreating array on every render

## Acceptance Criteria

- [x] All MUST_FIX items resolved
- [x] All SHOULD_FIX items resolved
- [x] Build passes
- [ ] Tests pass
- [ ] Reply to review comments on GitHub
