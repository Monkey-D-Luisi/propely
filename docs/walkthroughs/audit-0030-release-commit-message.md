# Walkthrough: audit-0030-release-commit-message

## Summary
Shortened the semantic-release git commit message to exclude inline release notes. Full notes remain available on the GitHub Release page and in CHANGELOG.md.

## Key Decisions
- **Remove `${nextRelease.notes}` only**: The `chore(release):` prefix is retained because the release workflow uses it in the skip condition (`!contains(..., 'chore(release):')`).
- **No `[skip ci]`**: Considered adding `[skip ci]` to avoid a CI run on the release commit, but the existing `!contains` condition in `release.yml` already prevents re-triggering.

## Files Changed
- `.releaserc.json` — Simplified git commit message template

## Tests
- No code tests affected (configuration change only)

## Verification
- JSON syntax valid (`jq` parses without errors)
- `chore(release):` prefix preserved for workflow skip logic
