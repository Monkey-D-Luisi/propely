# Task: 0021-property-create-edit-form

## Metadata
- ID: 0021
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0021-property-create-edit-form.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.8)

## Goal
Implement the Property Create and Edit forms as a multi-step wizard with 6 steps, auto-save to localStorage, draft saving, step validation, and reusable step components.

## Context
Task 2.4 created the JSON Schema and UI Schema for property data. Task 0020 created the data-fetching hooks. This task builds the guided multi-step property creation/editing experience with 6 steps mapping to the categories defined in the UI Schema: Basic Info, Location, Features, Financial, Descriptions, and Media.

## Scope
### In scope
- PropertyFormWizard component integrating all 6 steps
- StepIndicator component with active/completed/pending visual states
- 6 step components: BasicInfoStep, LocationStep, FeaturesStep, FinancialStep, DescriptionsStep, MediaStep
- useAutoSave hook for localStorage persistence (configurable interval)
- usePropertyForm hook for form state management, step navigation, validation, save/publish
- Create page (`/[locale]/properties/new`)
- Edit page (`/[locale]/properties/[id]/edit`) with pre-fill from existing data
- TDD test coverage for wizard, step indicator, auto-save

### Out of scope
- Backend form validation (already in properties-api)
- Media upload integration (handled by media management API endpoints)
- Full JSON Forms schema-driven rendering (step components use native form controls)

## Requirements
- R1: 6-step wizard: Basic Info, Location, Features, Financial, Descriptions, Media
- R2: StepIndicator shows all 6 steps with active/completed/pending visual states
- R3: Auto-save to localStorage every 30 seconds with restore on mount
- R4: Save as Draft action persists property without publishing
- R5: Publish action saves and navigates to property list
- R6: Edit page pre-fills form with existing property data via useProperty hook
- R7: Step validation prevents advancing with invalid data

## Acceptance Criteria
- AC1: Step indicator shows 6 steps with active/completed visual states
- AC2: Each step renders correct form fields for its category
- AC3: Auto-save persists form state to localStorage every 30 seconds
- AC4: Auto-save restores last saved state on mount
- AC5: Save Draft calls createProperty/updateProperty mutation
- AC6: Publish calls mutation then navigates to property list
- AC7: Edit page loads property data into form
- AC8: Step validation prevents advancing with invalid data
- AC9: All tests pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Create step components for each of the 6 form categories
2. Build StepIndicator with visual states
3. Assemble PropertyFormWizard with step navigation, validation, and actions
4. Create useAutoSave hook for localStorage persistence
5. Create usePropertyForm hook for form state management
6. Create new/edit pages wiring everything together

## Implementation Steps
1. Create BasicInfoStep component with title, type, operation fields
2. Create LocationStep component with address fields
3. Create FeaturesStep component with bedrooms, bathrooms, area, amenity toggles
4. Create FinancialStep component with price, currency, expenses
5. Create DescriptionsStep component with multi-language text areas
6. Create MediaStep component with photo upload/preview placeholder
7. Create StepIndicator component with tests
8. Create useAutoSave hook with tests
9. Create usePropertyForm hook
10. Create PropertyFormWizard integrating all steps with tests
11. Create `/properties/new` page
12. Create `/properties/[id]/edit` page

## Files to Create / Modify
### New files
- `apps/web/src/components/properties/form/PropertyFormWizard.tsx`
- `apps/web/src/components/properties/form/StepIndicator.tsx`
- `apps/web/src/components/properties/form/steps/BasicInfoStep.tsx`
- `apps/web/src/components/properties/form/steps/LocationStep.tsx`
- `apps/web/src/components/properties/form/steps/FeaturesStep.tsx`
- `apps/web/src/components/properties/form/steps/FinancialStep.tsx`
- `apps/web/src/components/properties/form/steps/DescriptionsStep.tsx`
- `apps/web/src/components/properties/form/steps/MediaStep.tsx`
- `apps/web/src/hooks/useAutoSave.ts`
- `apps/web/src/hooks/usePropertyForm.ts`
- `apps/web/src/app/[locale]/properties/new/page.tsx`
- `apps/web/src/app/[locale]/properties/[id]/edit/page.tsx`
- Tests: StepIndicator.test.tsx, useAutoSave.test.ts, PropertyFormWizard.test.tsx

## Testing Plan
- Unit tests:
  - 4 StepIndicator tests (render steps, active state, completed state, click navigation)
  - 4 useAutoSave tests (save, load, clear, interval-based persistence)
  - 10 PropertyFormWizard tests (step navigation, field rendering per step, save draft, publish, validation)
- Integration tests: N/A
- Manual verification: Walk through all 6 wizard steps, verify auto-save, test create and edit flows

## Security & Privacy
- Auto-save uses localStorage scoped by a unique form ID per property
- No sensitive data stored beyond property form drafts in localStorage
- All mutations go through authenticated API client

## Observability
- Logs: Console warnings when auto-save restore fails
- Metrics: N/A (frontend-only)
- Traces: N/A (frontend-only)

## Rollback Plan
Revert the commit on the `feat/p2-properties-core` branch. No database migrations involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
