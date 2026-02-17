# Task: 0005 - Extract All Frontend Strings to EN Locale

## Metadata
- ID: 0005
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0005-extract-strings-en.md`

## Goal
Extract every user-facing string from all frontend components into English message files organized by feature namespace, and replace hardcoded strings with `useTranslations()` calls.

## Context
Task 0003 set up the next-intl infrastructure with empty message files. Now we need to systematically go through every component, extract all user-facing strings (labels, messages, errors, button text, placeholders, etc.) into the EN message files, and use `useTranslations()` everywhere.

## Scope
### In scope
- Extract strings from ALL components in `apps/web/src/`
- Organize into namespaces: `common`, `auth`, `orgs`, `members`, `errors`, `navigation`
- Replace hardcoded strings with `t('key')` calls
- Handle interpolation for dynamic strings (e.g., `${member.email} is now ${nextRole}`)
- Handle pluralization if applicable
- Ensure error messages from API responses are still shown as-is (don't translate API errors)

### Out of scope
- Spanish translations (task 0006)
- Form refactoring (tasks 0007-0008)

## Requirements
- R1: Every user-facing string uses `useTranslations()` or `getTranslations()`
- R2: Message keys follow dot notation with feature namespace: `auth.signIn`, `orgs.members.roleUpdated`
- R3: No string literal in JSX that is visible to users (except for purely technical content like HTML attributes)
- R4: Dynamic values use next-intl's interpolation: `t('greeting', { name })`
- R5: Message file is well-organized and keys are alphabetically sorted within each namespace

## Acceptance Criteria
- AC1: `messages/en.json` contains all extracted strings organized by namespace
- AC2: Zero hardcoded user-facing strings remain in any component
- AC3: App renders identically to before (strings are the same, just sourced from message files)
- AC4: `npm run build` succeeds
- AC5: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Do NOT change component behavior or layout.

## Proposed Approach (high-level)
1. Audit all components and list every user-facing string
2. Design namespace structure in messages/en.json
3. Component by component: add `useTranslations`, replace strings, add to message file
4. Verify rendering is identical

## Implementation Steps
1. Create comprehensive namespace structure in `messages/en.json`:
   ```json
   {
     "common": { "appName", "signIn", "signOut", "close", "cancel", "confirm", "tryAgain", "loading", "refreshList" },
     "auth": { "login.title", "login.description", "login.email", "login.password", "login.submit", "login.submitting", "login.noAccount", "login.createAccount", "login.errors.*", "register.*", "csrf.*" },
     "orgs": { "mine.*", "create.*", "settings.*" },
     "members": { "title", "table.*", "invite.*", "leave.*", "roles.*" },
     "errors": { "notFound.*", "generic.*", "permissions.*" },
     "navigation": { "backToOrgs" }
   }
   ```
2. For each component file, add `const t = useTranslations('namespace')` and replace all strings
3. For server components, use `getTranslations()` instead
4. For toast messages, use the translation in the caller component
5. Run build and verify

## Files to Create / Modify
- `apps/web/messages/en.json` (modify - add all strings)
- Every `.tsx` file in `apps/web/src/components/` (modify)
- Every `.tsx` file in `apps/web/src/app/[locale]/` (modify)

## Testing Plan
- Unit tests: N/A (will be covered in tasks 0012-0014)
- Manual verification: Visual comparison of all pages before/after

## Security & Privacy
- No security impact (string extraction only)

## Observability
- No changes to observability

## Rollback Plan
Revert to hardcoded strings. Remove message file content.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
