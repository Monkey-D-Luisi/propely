# Task: 0015-property-create-edit-form

## Metadata
- ID: 0015
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0015-property-create-edit-form.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.8)

## Goal
Implement the Property Create and Edit forms using a multi-step wizard that integrates JSON Forms for schema-driven data entry, with auto-save support and step validation.

## Context
Task 2.4 created the JSON Schema and UI Schema for property data. This task creates the frontend wizard that uses those schemas to provide a guided multi-step property creation/editing experience. The wizard maps to the 6 categories defined in the UI Schema.

## Scope
### In scope
- StepIndicator component with 6 steps (active/completed states)
- PropertyFormWizard integrating @jsonforms/react per step
- useAutoSave hook (localStorage persistence every 30s)
- usePropertyForm hook (form state management, step navigation, save/publish)
- Create page (/[locale]/properties/new)
- Edit page (/[locale]/properties/[id]/edit)
- TDD test coverage

### Out of scope
- Backend form validation (already in properties-api)
- Media upload in form (separate from JSON Forms)

## Requirements
- R1: 6-step wizard matching UI Schema categories (Basic Info, Location, Features, Financial, Description, Media & Links)
- R2: JSON Forms renders correct fields per step
- R3: Auto-save to localStorage every 30 seconds
- R4: Save as Draft and Publish actions
- R5: Edit pre-fills form with existing property data

## Acceptance Criteria
- AC1: Step indicator shows 6 steps with active/completed visual states
- AC2: JSON Forms renders correct schema fields per step
- AC3: Auto-save persists form state to localStorage
- AC4: Save Draft calls createProperty/updateProperty mutation
- AC5: Publish calls mutation then navigates to property list
- AC6: Edit page loads property data into form
- AC7: All tests pass

## Files Created
- `apps/web/src/components/properties/form/StepIndicator.tsx`
- `apps/web/src/components/properties/form/PropertyFormWizard.tsx`
- `apps/web/src/hooks/useAutoSave.ts`
- `apps/web/src/hooks/usePropertyForm.ts`
- `apps/web/src/app/[locale]/properties/new/page.tsx`
- `apps/web/src/app/[locale]/properties/[id]/edit/page.tsx`
- Tests: StepIndicator.test.tsx, useAutoSave.test.ts, PropertyFormWizard.test.tsx

## Testing Plan
- 4 StepIndicator tests (render steps, active state, completed state)
- 4 useAutoSave tests (save, load, clear, interval)
- 10 PropertyFormWizard tests (step navigation, JSON Forms integration, save/publish)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added and pass
- [x] No secrets committed
- [x] Walkthrough created
