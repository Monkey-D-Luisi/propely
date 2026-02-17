# Task: 0004 - Setup react-hook-form and Create Form Primitives

## Metadata
- ID: 0004
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0004-react-hook-form-setup.md`

## Goal
Install react-hook-form with the Zod resolver and create reusable form primitives that integrate with the existing Tailwind design system. These primitives will be used in tasks 0007-0008 to refactor existing forms.

## Context
All forms currently use manual `useState` for each field plus manual validation. This works but doesn't scale well and leads to repetitive code. react-hook-form provides better performance (less re-rendering), built-in validation with our existing Zod schemas, and a cleaner API.

## Scope
### In scope
- Install react-hook-form and @hookform/resolvers
- Create form primitive components that wrap react-hook-form with Tailwind styles
- Primitives: FormField (label + input + error), FormSelect, FormSubmitButton, FormError
- Each primitive must accept a `name` prop that connects to the form context
- Primitives must match existing visual design (Tailwind classes from current forms)

### Out of scope
- Refactoring existing forms (tasks 0007-0008)
- i18n integration in primitives (primitives accept string props, i18n applied at usage)

## Requirements
- R1: FormField renders a label, input, and validation error message
- R2: FormSelect renders a label, select element, and validation error
- R3: FormSubmitButton shows loading state and disables during submission
- R4: All primitives use forwardRef for proper ref forwarding
- R5: Primitives accept className for style overrides
- R6: Types are fully generic - work with any Zod schema via react-hook-form's generic UseFormReturn

## Acceptance Criteria
- AC1: `npm install` succeeds with react-hook-form and @hookform/resolvers
- AC2: FormField component exists and renders label + input + error
- AC3: FormSelect component exists and renders label + select + error
- AC4: FormSubmitButton component exists with loading/disabled states
- AC5: A demo form exists (can be a test or storybook-like file) showing all primitives working together with a Zod schema
- AC6: `npm run build` succeeds
- AC7: ESLint passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Primitives must be generic and reusable, not tied to any specific form.

## Proposed Approach (high-level)
1. Install packages
2. Create form primitives in `components/ui/form/`
3. Use react-hook-form's `useFormContext` so primitives work inside any `FormProvider`
4. Match existing Tailwind styles from LoginForm/RegisterForm

## Implementation Steps
1. `cd apps/web && npm install react-hook-form @hookform/resolvers`
2. Create `apps/web/src/components/ui/form/form-field.tsx`
   - Props: name, label, type, placeholder, autoComplete, className, disabled
   - Uses useFormContext to get register, errors
   - Renders label, input with Tailwind classes, error message
3. Create `apps/web/src/components/ui/form/form-select.tsx`
   - Props: name, label, options: {label, value}[], className, disabled
   - Uses useFormContext
4. Create `apps/web/src/components/ui/form/form-submit-button.tsx`
   - Props: children, loadingText, className
   - Uses useFormContext to get formState.isSubmitting
5. Create `apps/web/src/components/ui/form/form-error.tsx`
   - Renders a form-level error message (e.g., "Invalid credentials")
   - Props: message (string | null)
6. Create `apps/web/src/components/ui/form/index.ts` barrel export
7. Run build and lint

## Files to Create / Modify
- `apps/web/src/components/ui/form/form-field.tsx` (create)
- `apps/web/src/components/ui/form/form-select.tsx` (create)
- `apps/web/src/components/ui/form/form-submit-button.tsx` (create)
- `apps/web/src/components/ui/form/form-error.tsx` (create)
- `apps/web/src/components/ui/form/index.ts` (create)
- `apps/web/package.json` (modify - new dependencies)

## Testing Plan
- Unit tests: Will be added in task 0014
- Manual verification: `npm run build`, `npm run lint`

## Security & Privacy
- Form primitives handle user input but validation is delegated to Zod schemas
- No XSS risk as React handles escaping

## Observability
- No changes to observability

## Rollback Plan
Remove packages and created files. Revert package.json.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
