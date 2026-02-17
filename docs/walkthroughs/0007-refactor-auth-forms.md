# Walkthrough: 0007 - Refactor Auth Forms with react-hook-form

## Task Reference
- Task: `docs/tasks/0007-refactor-auth-forms.md`
- Walkthrough: `docs/walkthroughs/0007-refactor-auth-forms.md`
- Branch/PR: `main`
- Date: `2026-02-06`

## Summary
Rewrote LoginForm and RegisterForm to use react-hook-form + Zod resolver + form primitives (FormField, FormSubmitButton, FormError). Added translated Zod validation messages via schema factory functions. All existing behavior (CSRF handling, redirect logic, error states) preserved.

## Context
- Background: LoginForm and RegisterForm used manual useState for each field and no form library. Task 0004 created form primitives integrating with react-hook-form. Task 0005 extracted all strings to i18n.
- Problem statement: Forms needed to use the project's form primitives and validation infrastructure instead of manual state management.
- Constraints: Visual appearance must remain the same. Existing auth flow unchanged.

## Decisions & Trade-offs
- **Decision: Schema factory functions for i18n validation messages**
  - Options considered: Static schemas with English-only messages, custom Zod error map, schema factory functions
  - Why this choice: Factory functions (`createLoginFormSchema`, `createRegisterFormSchema`) accept translated message strings and return a Zod schema. This keeps schemas type-safe while supporting i18n. Schemas are memoized in the component via `useMemo` keyed on the translation function.
  - Consequences: Schema is recreated when locale changes. Negligible performance impact.

- **Decision: Keep CSRF as separate useState**
  - Options considered: Add CSRF to form schema, keep as separate state
  - Why this choice: CSRF token is fetched asynchronously on mount and is not a user-editable field. It doesn't belong in the form schema. Kept as separate `useState` with `disabled={!csrfToken}` on the submit button.

## Implementation Notes
- Key changes:
  - Added `auth.validation` keys to both EN and ES locale files (emailRequired, emailInvalid, passwordRequired, passwordMinLength)
  - Added `createLoginFormSchema` and `createRegisterFormSchema` factory functions to `lib/schemas.ts`
  - LoginForm: Replaced manual email/password/isSubmitting useState with `useForm` + `FormProvider`. Uses `FormField`, `FormSubmitButton`, `FormError`.
  - RegisterForm: Same approach with name/email/password fields.
  - Both forms use `methods.handleSubmit(onSubmit)` which validates via Zod before calling the submit handler. react-hook-form manages `isSubmitting` state automatically.
- Edge cases handled: CSRF token still pre-fetched on mount and checked before submit
- Known limitations: None

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npm run build
```

## Files Changed
- `apps/web/messages/en.json` — Added `auth.validation` keys (emailRequired, emailInvalid, passwordRequired, passwordMinLength)
- `apps/web/messages/es.json` — Added matching Spanish validation keys
- `apps/web/src/lib/schemas.ts` — Added `createLoginFormSchema`, `createRegisterFormSchema`, and types
- `apps/web/src/components/auth/LoginForm.tsx` — Rewritten with react-hook-form + form primitives
- `apps/web/src/components/auth/RegisterForm.tsx` — Rewritten with react-hook-form + form primitives
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0007 status and progress tracker
- `docs/tasks/0007-refactor-auth-forms.md` — Marked DoD checklist complete

## Tests
### Unit
- What was added/updated: No unit tests in this task (deferred to task 0014)
- How to run: `cd apps/web && npm test`

### Manual
- Verified: `npm run build` succeeds with all routes generating correctly

## Observability
- No changes

## Security
- Validation: Zod validation runs client-side (defense in depth). Server still validates.
- AuthN/AuthZ impact: None. Same CSRF + cookie-based auth flow.
- Sensitive data handling: Password field uses `type="password"` and `autoComplete="new-password"/"current-password"`

## Follow-ups / Backlog
- [ ] Unit tests for LoginForm and RegisterForm (task 0014)

## Checklist
- [x] Task scope matches `docs/tasks/0007-refactor-auth-forms.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
