# Audit Action: audit-0027-trivy-exit-code

## Metadata
- ID: audit-0027
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-008-executive-summary.md`
  - Action plan item: "#1 — Tighten Trivy to block on vulnerabilities"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0027-trivy-exit-code.md`

## Goal
Make Trivy vulnerability scanning fail the publish pipeline when CRITICAL or HIGH CVEs are found, preventing vulnerable images from being published to GHCR.

## Context
The GHCR publish workflow runs Trivy with `exit-code: 0`, meaning it reports vulnerabilities but never blocks the pipeline. This allows container images with known CRITICAL/HIGH CVEs to be published.

## Scope
### In scope
- Change Trivy `exit-code` from `0` to `1`
- Add `.trivyignore` file for accepted CVEs

### Out of scope
- Changing Trivy severity levels
- Adding additional scanning tools

## Requirements
- R1: Trivy must fail the publish job when CRITICAL or HIGH vulnerabilities are found
- R2: A `.trivyignore` file must exist for managing accepted CVEs

## Acceptance Criteria
- [x] AC1: `exit-code: 1` in publish workflow Trivy step
- [x] AC2: `.trivyignore` file exists at repo root with instructions

## Constraints
- Must not break existing workflow structure
- Must maintain `ignore-unfixed: true` to avoid blocking on unpatched base image CVEs

## Implementation Steps
1. Change `exit-code` from `0` to `1` in `.github/workflows/publish.yml`
2. Create `.trivyignore` at repo root with usage instructions

## Testing Plan
- Manual checks: Verify YAML syntax is valid
- CI: Workflow will be validated on next push to main

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
