# Walkthrough: cr-0057-audit-epic-008-batch-review

## Task Reference
`docs/tasks/cr-0057-audit-epic-008-batch-review.md`

## PR Reference
PR #257 — fix(audit): address all audit findings for epic 008

## Changes Made

### 1. Fix coverage paths for composite action (MUST_FIX)
The `defaults.run.working-directory` in GitHub Actions only applies to `run:` steps, not `uses:` steps. The composite action was receiving relative paths (`./coverage/**`) which resolve from the repo root, not the service directory. Fixed by passing repo-rooted paths (`services/ai-api/coverage/**` and `services/orgs-api/coverage/**`).

### 2. Clear auth state in duplicate-registration test (MUST_FIX)
The `registeredPage` fixture leaves the browser authenticated. Navigating to `/en/register` while authenticated may cause a redirect away from the register page. Fixed by clearing cookies, localStorage, and sessionStorage before navigating to the register page.

### 3. Pin reportgenerator tool version (SHOULD_FIX)
Changed `dotnet tool update -g dotnet-reportgenerator-globaltool` to `dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.4.5` for CI reproducibility.

## Commands Run
- `npm run build` — SUCCESS
- `npm test` — 305/305 pass

## Process Deviations
None.
