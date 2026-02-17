# Task: 0001 - Code Cleanup and Consistency Sweep

## Metadata
- ID: 0001
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0001-code-cleanup.md`

## Goal
Eliminate all code quality issues across the entire codebase: Spanish comments, `console.error`, `any` types, hardcoded error strings, and inconsistent patterns.

## Context
The codebase was built iteratively across multiple sessions. Some files still contain Spanish comments, `console.error` statements (should use proper logging), `any` types, and inconsistent patterns between similar components. This cleanup must happen first so subsequent tasks build on a clean foundation.

## Scope
### In scope
- All files in `apps/web/src/`
- All files in `services/orgs-api/src/`
- All files in `services/ai-api/src/`
- Remove or translate any remaining Spanish text
- Replace `console.error` / `console.log` with proper error handling
- Replace all `any` types with proper TypeScript types
- Standardize component patterns (consistent prop destructuring, return patterns, error handling)
- Standardize import ordering across files

### Out of scope
- Adding new features
- Refactoring architecture
- Adding tests (separate task)
- Changing functionality

## Requirements
- R1: Zero Spanish comments remaining in any source file
- R2: Zero `any` types in TypeScript files
- R3: Zero `console.error` or `console.log` in production code
- R4: Consistent import ordering: React > Next.js > external libs > internal aliases > relative
- R5: Consistent component structure: types/interfaces at top, helpers, component, exports

## Acceptance Criteria
- AC1: `grep -r "console\.\(log\|error\)" apps/web/src/` returns zero results
- AC2: `grep -r ": any" apps/web/src/` returns zero results (excluding type declarations that genuinely need it)
- AC3: No Spanish text in any source file (comments or strings)
- AC4: ESLint passes with zero warnings: `cd apps/web && npm run lint`
- AC5: Both .NET solutions build: `dotnet build` for both services
- AC6: `cd apps/web && npm run build` succeeds

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Do NOT change any functionality or behavior.

## Proposed Approach (high-level)
1. Scan all files for Spanish text using grep
2. Scan all files for `any` types
3. Scan all files for `console.error` / `console.log`
4. Fix each file, maintaining exact same functionality
5. Standardize import ordering
6. Run lint + build to verify

## Implementation Steps
1. Search for all Spanish comments and translate to English
2. Search for all `console.error` / `console.log` and remove or replace with proper error handling (re-throw, pass to error state, etc.)
3. Search for all `any` types and replace with proper types (`unknown`, `Record<string, unknown>`, specific interfaces)
4. Standardize import ordering in all files
5. Run `npm run lint` and fix any warnings
6. Run `npm run build` to verify
7. Run `dotnet build` on both services to verify

## Files to Create / Modify
- All `.tsx` and `.ts` files in `apps/web/src/`
- All `.cs` files in `services/orgs-api/src/` and `services/ai-api/src/` (if Spanish comments exist)

## Testing Plan
- Unit tests: N/A (no behavior changes)
- Integration tests: N/A
- Manual verification: `npm run lint`, `npm run build`, `dotnet build`, `dotnet test`

## Security & Privacy
- No security impact (cleanup only)

## Observability
- No changes to observability

## Rollback Plan
Revert the commit. No behavioral changes means safe rollback.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
