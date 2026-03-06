# Walkthrough: 0021-property-create-edit-form

## Task Reference
- Task: `docs/tasks/0021-property-create-edit-form.md`
- Walkthrough: `docs/walkthroughs/0021-property-create-edit-form.md`
- Branch/PR: `feat/p2-properties-core`
- Date: `2026-03-05`

## Summary
Implemented the Property Create/Edit Form as a multi-step wizard with 6 steps (Basic Info, Location, Features, Financial, Descriptions, Media). Each step is a reusable component. The wizard includes auto-save to localStorage via a custom useAutoSave hook, draft saving, publish flow, and step validation. A usePropertyForm hook manages all form state and navigation. Create and Edit pages wire everything together.

## Context
- Background: The property JSON Schema and UI Schema defined 6 data categories. The CRUD hooks from Task 0020 provide API mutation capabilities. This task creates the guided form experience.
- Problem statement: Agents need a structured, multi-step form to create and edit properties efficiently without losing work (auto-save).
- Constraints: Must support both create (empty form + auto-save restore) and edit (pre-fill from API data) flows. TDD mandatory.

## Decisions & Trade-offs
- **Decision: 6 dedicated step components instead of JSON Forms auto-rendering**
  - Options considered: Full JSON Forms with schema-driven rendering per category, native form controls in dedicated step components
  - Why this choice: Dedicated step components provide more control over UX, validation, and layout. JSON Forms integration can be layered in later if needed, but native controls were faster to build and easier to test.
  - Consequences / risks: More boilerplate per step, but each step is independently testable and customizable.

- **Decision: useAutoSave with localStorage**
  - Options considered: Server-side draft persistence, sessionStorage, localStorage
  - Why this choice: localStorage survives browser refreshes and tab closures. Server-side drafts would require backend changes. sessionStorage would lose data on tab close.
  - Consequences / risks: localStorage has size limits (~5MB), but property form data is small. Auto-save keyed by form ID prevents conflicts.

- **Decision: Separate usePropertyForm hook**
  - Options considered: Form state inline in wizard, separate hook
  - Why this choice: Extracting form state management into usePropertyForm keeps the wizard component focused on rendering. The hook handles step navigation, validation, and mutation coordination.
  - Consequences / risks: Hook is tightly coupled to wizard, but testable in isolation.

## Implementation Notes
- Key changes:
  - BasicInfoStep: title, property type dropdown, operation type dropdown
  - LocationStep: street, city, province, postal code, country fields
  - FeaturesStep: bedrooms, bathrooms, area inputs, amenity toggle switches (pool, garden, garage, elevator, terrace)
  - FinancialStep: price, currency, community expenses, IBI tax fields
  - DescriptionsStep: multi-language text areas (es, en, fr, pt, de, nl)
  - MediaStep: photo upload placeholder, media preview area
  - StepIndicator: renders 6 numbered steps with active (primary color), completed (check icon), and pending (gray) states
  - PropertyFormWizard: orchestrates steps with Next/Back buttons, Save Draft, and Publish actions
  - useAutoSave: configurable interval (default 30s), save/load/clear with unique key, lastSaved timestamp
  - usePropertyForm: manages currentStep, completedSteps, formData, provides saveDraft/publish functions that call createProperty or updateProperty mutations
- Edge cases handled: Auto-save restore prompts user on mount, clearing auto-save on successful publish, step validation prevents skipping required fields
- Known limitations: Step validation is client-side only; backend validation provides the authoritative check

## Data / Schema / Migrations
- DB changes: None (frontend-only task)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
cd apps/web && npx tsc --noEmit
cd apps/web && npx vitest run
```

## Files Changed

### Hooks
- `apps/web/src/hooks/useAutoSave.ts` -- localStorage-based auto-save with configurable interval, save/load/clear/lastSaved
- `apps/web/src/hooks/usePropertyForm.ts` -- Form wizard state management (currentStep, completedSteps, formData, saveDraft, publish)

### Components
- `apps/web/src/components/properties/form/StepIndicator.tsx` -- 6-step indicator with active/completed/pending visual states
- `apps/web/src/components/properties/form/PropertyFormWizard.tsx` -- Orchestrates steps with navigation, validation, save/publish
- `apps/web/src/components/properties/form/steps/BasicInfoStep.tsx` -- Title, type, operation fields
- `apps/web/src/components/properties/form/steps/LocationStep.tsx` -- Address fields
- `apps/web/src/components/properties/form/steps/FeaturesStep.tsx` -- Bedrooms, bathrooms, area, amenity toggles
- `apps/web/src/components/properties/form/steps/FinancialStep.tsx` -- Price, currency, expenses
- `apps/web/src/components/properties/form/steps/DescriptionsStep.tsx` -- Multi-language text areas
- `apps/web/src/components/properties/form/steps/MediaStep.tsx` -- Photo upload placeholder

### Pages
- `apps/web/src/app/[locale]/properties/new/page.tsx` -- Create property page
- `apps/web/src/app/[locale]/properties/[id]/edit/page.tsx` -- Edit property page (pre-fills from API)

## Tests
### Unit
- What was added/updated:
  - StepIndicator: 4 tests (render steps, active state, completed state, click navigation)
  - useAutoSave: 4 tests (save to localStorage, load from localStorage, clear, interval-based persistence)
  - PropertyFormWizard: 10 tests (step navigation forward/back, field rendering per step, save draft action, publish action, validation blocking)
- How to run: `cd apps/web && npx vitest run`

### Integration
- What was added/updated: N/A
- How to run: N/A

### Manual
- What you verified: Full create flow through all 6 steps, auto-save restoration after page refresh, edit flow with pre-filled data, save draft and publish actions
- Steps: Start dev server, navigate to /properties/new, fill steps, verify auto-save in localStorage, publish. Then edit an existing property at /properties/[id]/edit.

## Observability
- Logs added/updated: Console warning when auto-save restore fails (corrupted data)
- Traces/metrics added/updated: N/A (frontend-only)

## Security
- Validation: Client-side step validation; backend provides authoritative validation
- AuthN/AuthZ impact: Mutations use authenticated API client with tenant context
- Sensitive data handling: localStorage drafts contain property form data only (no secrets/PII)

## Follow-ups / Backlog
- [x] Property detail view (Task 0022)
- [x] Advanced search filters (Task 0023)

## Checklist
- [x] Task scope matches `docs/tasks/0021-property-create-edit-form.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
