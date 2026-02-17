# Task: 0021 - CI Improvements (SAST, Dependency Checking, Coverage Gates)

## Metadata
- ID: 0021
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0021-ci-improvements.md`

## Goal
Add security scanning (SAST), dependency vulnerability checking, and code coverage gates to the CI pipeline. Add status badges to README.

## Context
The CI currently builds and runs tests but lacks security scanning, dependency auditing, and coverage enforcement. A professional template needs these guardrails to catch vulnerabilities and maintain quality.

## Scope
### In scope
- Add CodeQL or Semgrep analysis to CI for .NET and TypeScript
- Add `npm audit` and `dotnet list package --vulnerable` checks
- Add Dependabot configuration for automated dependency updates
- Add coverage collection and minimum coverage gates (fail CI if below threshold)
- Add status badges to README (build, coverage)
- Upload coverage to a service or as artifact

### Out of scope
- DAST (dynamic security testing)
- Container scanning (would need built images)
- License compliance checking

## Requirements
- R1: SAST runs on every PR and pushes to main
- R2: Dependency vulnerability check fails CI on high/critical vulnerabilities
- R3: Coverage gates: fail if below thresholds (frontend >70%, backend Domain >90%, Application >80%)
- R4: Dependabot checks weekly for dependency updates
- R5: README shows build status badge

## Acceptance Criteria
- AC1: SAST workflow runs and reports findings
- AC2: `npm audit --audit-level=high` runs in CI
- AC3: `.github/dependabot.yml` exists with correct configuration
- AC4: Coverage is collected and thresholds enforced
- AC5: README has status badge for CI
- AC6: All CI checks pass on a clean PR

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `.github/workflows/codeql.yml` for CodeQL analysis
2. Add `npm audit --audit-level=high` step to web job in ci.yml
3. Add `dotnet list package --vulnerable --include-transitive` to .NET jobs
4. Create `.github/dependabot.yml` for npm + NuGet
5. Add coverage collection to ci.yml (coverlet for .NET, vitest coverage for web)
6. Add coverage threshold checks
7. Add badges to README.md

## Files to Create / Modify
- `.github/workflows/codeql.yml` (create)
- `.github/workflows/ci.yml` (modify - add audit + coverage steps)
- `.github/dependabot.yml` (create)
- `README.md` (modify - add badges)

## Testing Plan
- Manual verification: Push a PR and verify all CI checks run

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
