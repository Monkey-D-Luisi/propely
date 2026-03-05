# Walkthrough: 0015-property-create-edit-form

## Summary
Implements the Property Create/Edit Form as a multi-step wizard using JSON Forms for schema-driven data entry, with auto-save support and step navigation.

## Architecture Decisions

### JSON Forms Wizard Integration
The property UI Schema defines a `Categorization` with 6 `Category` elements (Basic Info, Location, Features, Financial, Description, Media & Links). The `PropertyFormWizard` component maps each category to a wizard step, rendering only the fields for the current step using JsonForms with the category's sub-elements.

### State Management
`usePropertyForm` hook manages wizard state: currentStep, completedSteps, formData, and provides saveDraft/publish actions. For Create, it calls `useCreateProperty` mutation; for Edit, it calls `useUpdateProperty`. After publish, it navigates to the properties list.

### Auto-Save
`useAutoSave` hook persists form state to localStorage every 30 seconds using a debounced interval. On mount, it restores the last saved state. The hook exposes `save`, `load`, `clear`, and `lastSaved` timestamp. Auto-save is keyed by a unique form ID to prevent conflicts between create and edit forms.

### Create vs Edit Pages
Both `/properties/new` and `/properties/[id]/edit` render the same `PropertyFormWizard` component but with different data sources. Create starts with empty data + auto-save restore. Edit fetches existing property data via `useProperty(id)` and pre-fills the form.

## Files Changed

### Hooks
- `apps/web/src/hooks/useAutoSave.ts` — localStorage-based auto-save with configurable interval
- `apps/web/src/hooks/usePropertyForm.ts` — Form wizard state management

### Components
- `StepIndicator.tsx` — 6-step indicator with active/completed visual states
- `PropertyFormWizard.tsx` — Integrates JsonForms per step, save draft/publish buttons

### Pages
- `apps/web/src/app/[locale]/properties/new/page.tsx` — Create property page
- `apps/web/src/app/[locale]/properties/[id]/edit/page.tsx` — Edit property page

## Test Coverage
- StepIndicator: 4 tests (render steps, active state, completed state)
- useAutoSave: 4 tests (save, load, clear, interval)
- PropertyFormWizard: 10 tests (step navigation, JsonForms rendering, save/publish)
- **Total: 18 tests, all passing**
