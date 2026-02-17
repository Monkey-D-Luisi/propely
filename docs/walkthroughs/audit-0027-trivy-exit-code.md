# Walkthrough: audit-0027-trivy-exit-code

## Summary
Changed Trivy vulnerability scanning from report-only mode to blocking mode, so CRITICAL/HIGH CVEs prevent container image publishing. Added `.trivyignore` for managing accepted exceptions.

## Key Decisions
- **`exit-code: 1`**: Trivy now fails the job when vulnerabilities matching the severity filter are found. Combined with `ignore-unfixed: true`, this only blocks on CVEs that have available fixes.
- **`.trivyignore` at repo root**: Provides a standard mechanism to allowlist CVEs that are false positives or accepted risks, with documentation for each entry.

## Files Changed
- `.github/workflows/publish.yml` — Changed `exit-code` from `0` to `1`
- `.trivyignore` — New file with instructions for managing accepted CVEs

## Tests
- No code tests affected (CI/CD workflow change only)

## Verification
- YAML syntax valid
- Trivy documentation confirms `exit-code: 1` causes non-zero exit on findings
