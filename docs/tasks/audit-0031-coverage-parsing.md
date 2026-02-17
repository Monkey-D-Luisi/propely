# Audit Action: audit-0031-coverage-parsing

## Metadata
- ID: audit-0031
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#5 — Harden coverage threshold parsing"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0031-coverage-parsing.md`

## Goal
Replace fragile `grep -oP` + `bc` coverage parsing with a robust composite action using `awk`, with proper error handling and DRY reuse across both .NET service jobs.

## Context
The CI workflow parses coverage percentages using `grep -oP` (Perl regex) piped to `bc -l`. If the ReportGenerator output format changes or the line doesn't exist, `$line_coverage` is empty and `bc` silently fails. The same block is duplicated in both ai-api and orgs-api jobs.

## Scope
### In scope
- Create composite action `.github/actions/check-coverage/action.yml`
- Replace inline bash in both ai-api and orgs-api jobs
- Add error handling for missing files and unparseable output

### Out of scope
- Changing the coverage threshold value (stays at 60%)
- Adding coverage badge generation

## Requirements
- R1: Coverage parsing must fail explicitly if the summary file is missing
- R2: Coverage parsing must fail explicitly if the line coverage value cannot be extracted
- R3: Both .NET service jobs must use the same composite action

## Acceptance Criteria
- [x] AC1: Composite action exists at `.github/actions/check-coverage/action.yml`
- [x] AC2: Both ai-api and orgs-api jobs use the composite action
- [x] AC3: Error handling covers missing file and unparseable output scenarios

## Constraints
- Must use `awk` instead of `grep -oP` + `bc` (POSIX-compatible)
- Must maintain the same 60% threshold

## Implementation Steps
1. Create `.github/actions/check-coverage/action.yml` with inputs for coverage-path, threshold, and report-dir
2. Replace inline bash blocks in ci.yml ai-api and orgs-api jobs

## Testing Plan
- Manual checks: Verify YAML syntax
- CI: Coverage check runs on next CI execution

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
