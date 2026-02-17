# Walkthrough: 0004-react-hook-form-setup

## Task Reference
- Task: `docs/tasks/0004-react-hook-form-setup.md`
- Walkthrough: `docs/walkthroughs/0004-react-hook-form-setup.md`
- Branch/PR: `feat/0004-react-hook-form-setup`
- Date: 2026-02-05

## Summary
Implemented a strongly-typed, reusable form system using `react-hook-form` and `zod`. Created primitives (`FormField`, `FormSelect`, `FormSubmitButton`, `FormError`) that integrate seamlessly with the existing Tailwind design system.

## Context
- **Problem**: Forms were manually managing state and validation, leading to verbose/duplicated code.
- **Solution**: Adopted `react-hook-form` for performance and easier validation integration.
- **Constraints**: Must match existing design exactly.

## Files Changed
- `apps/web/package.json`: Added `react-hook-form`, `@hookform/resolvers`
- `apps/web/src/components/ui/form/form-field.tsx`: New input primitive
- `apps/web/src/components/ui/form/form-select.tsx`: New select primitive
- `apps/web/src/components/ui/form/form-submit-button.tsx`: New submit button
- `apps/web/src/components/ui/form/form-error.tsx`: New error banner
- `apps/web/src/components/ui/form/index.ts`: Barrel export
- `apps/web/src/components/ui/form/form-demo.tsx`: Usage demonstration

## Commands Run
```bash
npm install react-hook-form @hookform/resolvers
npm run build
npm run lint
```

## PR Review Fixes (cr-0003)

Post-PR review, the following issues were identified and fixed:

### Critical Fixes
1. **Ref Conflict**: The forwarded `ref` was overriding react-hook-form's internal ref
   - **Fix**: Implemented manual ref merging using callback refs
   - **Files**: `form-field.tsx`, `form-select.tsx`

2. **Props Override Issue**: `{...props}` was spread after `{...registerProps}`, allowing consumers to override RHF's handlers
   - **Fix**: Moved `{...props}` before `{...registerProps}` so RHF handlers take precedence
   - **Fix**: Omitted `onChange` and `onBlur` from type definitions to prevent misuse
   - **Files**: `form-field.tsx:6-9`, `form-select.tsx:11-17`

### Accessibility & Quality Fixes
3. **FormField Accessibility**: Was using implicit label wrapping while FormSelect used explicit htmlFor
   - **Fix**: Updated FormField to use `useId()` + explicit `htmlFor` for consistency
   - **File**: `form-field.tsx:17-21`

4. **className Readability**: Template literals were split across multiple lines
   - **Fix**: Simplified to single-line template literals
   - **Files**: `form-field.tsx:36`, `form-select.tsx:51`

5. **Linter Warning**: Unused `data` parameter in demo
   - **Fix**: Prefixed with underscore and added eslint-disable comment
   - **File**: `form-demo.tsx:26-27`

## Checklist
- [x] Task scope matches `docs/tasks/0004-react-hook-form-setup.md`
- [x] Tests updated and passing (Build & Lint verified)
- [x] PR review feedback addressed (see cr-0003)
- [x] Docs updated where relevant
- [x] No secrets committed
