# Task: 0007 - Refactor Auth Forms with react-hook-form

## Metadata
- ID: 0007
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0007-refactor-auth-forms.md`

## Goal
Rewrite LoginForm and RegisterForm using react-hook-form + Zod resolver + form primitives from task 0004. Integrate i18n for all labels and error messages.

## Context
LoginForm and RegisterForm currently use multiple `useState` hooks for form state and manual validation. Task 0004 created reusable form primitives and task 0005 externalized strings. Now we combine both to create clean, maintainable auth forms.

## Scope
### In scope
- Rewrite LoginForm using react-hook-form + FormProvider + form primitives
- Rewrite RegisterForm using react-hook-form + FormProvider + form primitives
- Use existing Zod schemas (or create auth-specific ones) as form validation schemas
- All form labels, placeholders, and error messages from i18n
- Preserve all existing behavior (CSRF handling, redirect logic, error states)
- Remove manual useState for form fields

### Out of scope
- Changing auth flow or API endpoints
- Adding new form fields
- Org-related forms (task 0008)

## Requirements
- R1: LoginForm uses react-hook-form with Zod resolver for email + password validation
- R2: RegisterForm uses react-hook-form with Zod resolver for name + email + password validation
- R3: All visible text comes from i18n (`useTranslations`)
- R4: Form-level errors (API errors like "Invalid credentials") display via FormError
- R5: Submit button shows loading text during submission
- R6: Existing redirect logic (next param, reauth) remains unchanged
- R7: CSRF token handling remains unchanged

## Acceptance Criteria
- AC1: LoginForm renders and submits correctly with valid data
- AC2: LoginForm shows validation errors for empty/invalid fields
- AC3: RegisterForm renders and submits correctly with valid data
- AC4: RegisterForm shows validation errors for empty/invalid fields
- AC5: API error messages (401, 403) display correctly
- AC6: Redirect after login/register works as before
- AC7: `npm run build` succeeds
- AC8: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Visual appearance must remain the same.

## Proposed Approach (high-level)
1. Create Zod schemas for login and register forms (may already exist or extend existing ones)
2. Rewrite LoginForm: FormProvider + zodResolver + form primitives
3. Rewrite RegisterForm: same approach
4. Keep CSRF and redirect logic in submit handlers
5. Test both forms manually

## Implementation Steps
1. Create/verify Zod schemas for login form (`loginFormSchema`) and register form (`registerFormSchema`)
2. Rewrite `LoginForm.tsx`:
   - `useForm({ resolver: zodResolver(loginFormSchema) })`
   - Wrap in `FormProvider`
   - Replace `<input>` elements with `<FormField>` primitives
   - Replace submit button with `<FormSubmitButton>`
   - Keep `handleSubmit` logic for CSRF + API call + redirect
   - Use `t()` for all strings
3. Rewrite `RegisterForm.tsx`: same approach
4. Remove unused `useState` imports
5. Run build and lint

## Files to Create / Modify
- `apps/web/src/components/auth/LoginForm.tsx` (modify - rewrite)
- `apps/web/src/components/auth/RegisterForm.tsx` (modify - rewrite)
- `apps/web/src/lib/schemas.ts` (modify - add login/register form schemas if needed)

## Testing Plan
- Unit tests: Will be covered in task 0014
- Manual verification: Submit login/register forms, verify validation, error states, redirects

## Security & Privacy
- Zod validation runs client-side (defense in depth, server still validates)
- Password field uses type="password"
- CSRF protection unchanged

## Observability
- No changes to observability

## Rollback Plan
Revert form components to useState-based versions from git history.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
