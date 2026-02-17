# Walkthrough: 0008 - Refactor Org Forms with react-hook-form

## Task Reference
- Task: `docs/tasks/0008-refactor-org-forms.md`
- Walkthrough: `docs/walkthroughs/0008-refactor-org-forms.md`
- Branch/PR: `main`
- Date: `2026-02-06`

## Summary
Rewrote InviteForm and the create-org form (inline in MyOrgsPage) to use react-hook-form + Zod resolver + form primitives (FormField, FormSelect, FormSubmitButton, FormError). Added schema factory functions with translated Zod validation messages. All existing behavior preserved.

## Context
- Background: InviteForm used manual useState for email/role/isSubmitting and validated via `InviteRequestSchema.safeParse()`. The create-org form was inline in MyOrgsPage with manual state for orgName/creating/createError and only a basic `trim()` check.
- Problem statement: Forms needed to use the project's form primitives and validation infrastructure instead of manual state management (same rationale as task 0007 for auth forms).
- Constraints: Visual appearance must remain the same. Existing behavior unchanged.

## Decisions & Trade-offs
- **Decision: Schema factory functions (same pattern as auth forms)**
  - Options considered: Reuse InviteRequestSchema directly, create factory functions
  - Why this choice: Factory functions (`createInviteFormSchema`, `createOrgFormSchema`) accept translated message strings for consistency with the auth form pattern. This keeps Zod validation messages i18n-compatible.
  - Consequences: Consistent pattern across all forms in the app.

- **Decision: Keep create-org form inline in MyOrgsPage**
  - Options considered: Extract to separate component, keep inline
  - Why this choice: The form is simple (one field) and tightly coupled to the page's show/hide toggle and refetch logic. Extracting it would add complexity without benefit.

- **Decision: Use `methods.setError('root', ...)` for API errors**
  - Options considered: Separate error useState, root form error
  - Why this choice: react-hook-form's root error integrates with FormError naturally. No need for separate state.

## Implementation Notes
- Key changes:
  - Added `orgs.validation` keys to both EN and ES locale files (emailRequired, emailInvalid, orgNameRequired)
  - Added `createInviteFormSchema`, `createOrgFormSchema` factory functions to `lib/schemas.ts`
  - InviteForm: Replaced manual email/role/isSubmitting/error useState with `useForm` + `FormProvider`. Uses `FormField`, `FormSelect`, `FormSubmitButton`, `FormError`. Uses `methods.reset()` after success.
  - MyOrgsPage: Replaced manual orgName/creating/createError useState with `useForm` + `FormProvider`. Uses `FormField`, `FormSubmitButton`, `FormError`. Kept `showForm` useState for toggle.
  - Both forms use `noValidate` on `<form>` to let Zod handle validation.
- Edge cases handled: Form reset after successful submission, root error for API failures
- Known limitations: None

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npm run build
cd apps/web && npx eslint src/components/orgs/InviteForm.tsx "src/app/[locale]/orgs/mine/page.tsx" src/lib/schemas.ts
```

## Files Changed
- `apps/web/messages/en.json` — Added `orgs.validation` keys (emailRequired, emailInvalid, orgNameRequired)
- `apps/web/messages/es.json` — Added matching Spanish validation keys
- `apps/web/src/lib/schemas.ts` — Added `createInviteFormSchema`, `createOrgFormSchema`, and types
- `apps/web/src/components/orgs/InviteForm.tsx` — Rewritten with react-hook-form + form primitives
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` — Rewritten create-org form with react-hook-form
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0008 status and progress tracker
- `docs/tasks/0008-refactor-org-forms.md` — Marked DoD checklist complete

## Tests
### Unit
- What was added/updated: No unit tests in this task (deferred to task 0014)
- How to run: `cd apps/web && npm test`

### Manual
- Verified: `npm run build` succeeds with all routes generating correctly
- Verified: ESLint passes with no errors

## Observability
- No changes

## Security
- Validation: Zod validation runs client-side (defense in depth). Server still validates.
- AuthN/AuthZ impact: None. InviteForm still requires `canInvite` prop.

## Follow-ups / Backlog
- [ ] Unit tests for InviteForm and CreateOrg form (task 0014)

## Checklist
- [x] Task scope matches `docs/tasks/0008-refactor-org-forms.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
