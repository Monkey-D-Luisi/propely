# Audit Action: audit-0037-rollback-expression-injection

## Metadata
- ID: audit-0037
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "#2 — Harden rollback.yml expression injection"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0037-rollback-expression-injection.md`

## Goal
Eliminate expression injection vectors in rollback.yml by mapping all `${{ }}` expressions in shell `run:` blocks to step-level `env:` variables.

## Context
Same pattern as audit-0036 but for the rollback workflow. While `inputs.services` and `inputs.environment` are `type: choice` (limiting injection surface), `inputs.image_tag` is `type: string` (free-form). Hardening all expressions for consistency and defense-in-depth.

## Scope
### In scope
- Map all `${{ }}` expressions in `run:` blocks to `env:` variables in rollback.yml
- Covers: resolve-services job, rollback job, summary job

### Out of scope
- deploy.yml (done in audit-0036)

## Requirements
- R1: No `${{ }}` expressions remain inside shell `run:` blocks
- R2: Workflow behavior is unchanged

## Acceptance Criteria
- [x] AC1: All shell `run:` blocks in rollback.yml use `env:` variable references
- [x] AC2: Workflow YAML syntax is valid

## Constraints
- Must not change workflow behavior

## Implementation Steps
1. Map expressions in `resolve-services` job to `env:` block
2. Map expressions in `rollback` job steps (verify image, rollback, get URL, health path)
3. Map expressions in `summary` job write step

## Testing Plan
- Manual checks: Verify YAML syntax validity

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
