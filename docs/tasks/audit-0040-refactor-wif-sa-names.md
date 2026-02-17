# Audit Action: audit-0040-refactor-wif-sa-names

## Metadata
- ID: audit-0040
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "Refactor hardcoded SA names in WIF config"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0040-refactor-wif-sa-names.md`

## Goal
Replace the hardcoded service account name list in `deploy_act_as` with a computed local using `setproduct`.

## Context
The WIF bootstrap config hardcoded all 6 Cloud Run service account names (3 services x 2 environments). Adding a new service or environment required manual updates in the `for_each` list, creating maintenance burden and risk of drift.

## Scope
### In scope
- Replace hardcoded `for_each` list with computed `locals` block
- Use `setproduct` to generate combinations of environments and services

### Out of scope
- Changing the SA naming convention itself
- Moving the `deploy_act_as` binding to environment configs

## Requirements
- R1: SA names must be identical before and after refactoring
- R2: `terraform validate` must pass

## Acceptance Criteria
- [x] AC1: `deploy_act_as` uses computed local instead of hardcoded list
- [x] AC2: `terraform validate` passes on bootstrap config

## Constraints
- C1: Must produce the same `for_each` keys to avoid Terraform state drift

## Implementation Steps
1. Add `locals` block with `deploy_environments`, `deploy_services`, and `cloud_run_sa_names`
2. Replace hardcoded `toset([...])` with `toset(local.cloud_run_sa_names)`
3. Run `terraform validate`

## Testing Plan
- Unit tests: N/A (IaC)
- Integration tests: N/A
- Manual checks: `terraform validate` on bootstrap config

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
