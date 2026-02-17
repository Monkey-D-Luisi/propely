# Task: 0008 - Refactor Org Forms with react-hook-form

## Metadata
- ID: 0008
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0008-refactor-org-forms.md`

## Goal
Rewrite InviteForm and the create org form using react-hook-form + Zod resolver + form primitives. Integrate i18n for all labels and error messages.

## Context
Similar to task 0007 but for organization-related forms. InviteForm has email + role fields. The create org flow (currently inline in the org list page) needs a proper form treatment.

## Scope
### In scope
- Rewrite InviteForm using react-hook-form + form primitives + i18n
- Rewrite CreateOrg form/dialog using react-hook-form + form primitives + i18n
- Use existing Zod schemas (InviteRequestSchema, etc.) as form resolvers
- All visible text from i18n
- Preserve existing behavior

### Out of scope
- LeaveOrgButton (it's a confirmation dialog, not a form - just needs i18n)
- Adding new form fields
- Auth forms (task 0007)

## Requirements
- R1: InviteForm uses react-hook-form with zodResolver(InviteRequestSchema)
- R2: CreateOrg form uses react-hook-form with a Zod schema for org name
- R3: All visible text from i18n
- R4: Form submission triggers toast notifications (success/error) with i18n text
- R5: Reset form after successful submission

## Acceptance Criteria
- AC1: InviteForm validates email format before submission
- AC2: InviteForm shows role selector with i18n labels
- AC3: CreateOrg form validates org name
- AC4: Toast messages appear after success/error
- AC5: `npm run build` succeeds
- AC6: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Rewrite InviteForm with react-hook-form + FormField + FormSelect + FormSubmitButton
2. Rewrite CreateOrg with react-hook-form
3. Ensure i18n for all strings
4. Test both forms

## Implementation Steps
1. Rewrite `InviteForm.tsx`:
   - `useForm({ resolver: zodResolver(InviteRequestSchema) })`
   - FormField for email, FormSelect for role
   - FormSubmitButton with loading text
   - i18n for all labels
2. Rewrite/create org creation form:
   - Zod schema for org name validation
   - FormField for name
   - FormSubmitButton
3. Ensure LeaveOrgButton dialog uses i18n strings (no form refactor needed)
4. Run build and lint

## Files to Create / Modify
- `apps/web/src/components/orgs/InviteForm.tsx` (modify)
- `apps/web/src/components/orgs/CreateOrgForm.tsx` (create or modify existing)
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` (modify if create org form is inline)
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` (modify - i18n only)

## Testing Plan
- Unit tests: Will be covered in task 0014
- Manual verification: Submit invite, create org, verify validation

## Security & Privacy
- Zod validation client-side (server validates too)
- Email field validated against injection

## Observability
- No changes

## Rollback Plan
Revert form components from git history.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
