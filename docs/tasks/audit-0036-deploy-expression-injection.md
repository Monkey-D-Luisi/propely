# Audit Action: audit-0036-deploy-expression-injection

## Metadata
- ID: audit-0036
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "#1 — Harden deploy.yml expression injection"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0036-deploy-expression-injection.md`

## Goal
Eliminate expression injection vectors in deploy.yml by mapping all `${{ }}` expressions in shell `run:` blocks to step-level `env:` variables.

## Context
GitHub Actions `${{ }}` expressions in shell `run:` blocks are string-interpolated into the script before execution. A malicious `inputs.image_tag` value (free-form string from workflow_dispatch) could inject arbitrary shell commands. This was identified as finding F1/F2 in the Epic 009 audit.

## Scope
### In scope
- Map all `${{ }}` expressions in `run:` blocks to `env:` variables in deploy.yml
- Covers: prepare job, deploy-apis job, deploy-web job, summary job

### Out of scope
- rollback.yml (separate audit action)
- Input validation for image_tag (separate audit action)

## Requirements
- R1: No `${{ }}` expressions remain inside shell `run:` blocks (except in `env:` mappings)
- R2: Workflow behavior is unchanged
- R3: `with:` blocks for action inputs are unchanged (safe context, not shell-interpolated)

## Acceptance Criteria
- [x] AC1: All shell `run:` blocks in deploy.yml use `env:` variable references instead of `${{ }}`
- [x] AC2: Workflow YAML syntax is valid
- [x] AC3: `with:` blocks for composite actions are left as-is (not shell context)

## Constraints
- Must not change workflow behavior
- Must not modify `with:` blocks (they are safe)

## Implementation Steps
1. Map expressions in `prepare` job `resolve` step to `env:` block
2. Map expressions in `deploy-apis` GHCR image wait step to `env:` block
3. Map expressions in `deploy-apis` configure Docker, deploy, and get URL steps
4. Map expressions in `deploy-web` configure Docker, deploy, and get URL steps
5. Map expressions in `summary` job write step

## Testing Plan
- Manual checks: Verify YAML syntax validity
- Visual diff: Confirm `run:` blocks no longer contain `${{ }}` (only `env:` and `with:` do)

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
