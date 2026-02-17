# Walkthrough: audit-0031-coverage-parsing

## Summary
Extracted the fragile inline coverage parsing bash script into a reusable composite GitHub Action with robust `awk`-based parsing and explicit error handling.

## Key Decisions
- **Composite action over reusable workflow**: Composite actions run in the caller's job context (same `working-directory`), avoiding the complexity of reusable workflows that need artifact passing.
- **`awk` over `grep -oP` + `bc`**: `awk` is POSIX-compliant and handles both extraction and numeric comparison natively. No dependency on `bc` or Perl regex.
- **Explicit error handling**: The action fails with clear error messages if the summary file is missing or if the coverage value cannot be parsed, instead of silently passing.

## Files Changed
- `.github/actions/check-coverage/action.yml` — New composite action
- `.github/workflows/ci.yml` — Replaced inline bash in both ai-api (line 97) and orgs-api (line 166) jobs with composite action usage

## Tests
- No code tests affected (CI infrastructure change only)

## Verification
- YAML syntax valid for both action and workflow files
- `awk` parsing tested against expected ReportGenerator output format
