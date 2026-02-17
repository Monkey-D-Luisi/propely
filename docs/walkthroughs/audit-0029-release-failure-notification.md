# Walkthrough: audit-0029-release-failure-notification

## Summary
Added a failure notification step to the release workflow that posts a commit comment when semantic-release fails, providing a link to the failed workflow run.

## Key Decisions
- **Commit comment over issue**: A commit comment is less noisy than creating an issue for each failure. It attaches the notification directly to the commit that failed to release.
- **`actions/github-script@v7`**: Uses the GitHub API directly without requiring additional action dependencies.
- **`if: failure()`**: Only runs when a previous step fails, ensuring no false alarms.

## Files Changed
- `.github/workflows/release.yml` — Added failure notification step using `actions/github-script`

## Tests
- No code tests affected (CI/CD workflow change only)

## Verification
- YAML syntax valid
- `actions/github-script` API usage matches GitHub REST API docs
