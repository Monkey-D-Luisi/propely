# Audit Action: audit-0038-terraform-validate-ci

## Metadata
- ID: audit-0038
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "#3 — Add Terraform validate CI step"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0038-terraform-validate-ci.md`

## Goal
Automatically validate Terraform configurations on PRs that modify infrastructure files.

## Context
Terraform validate is mentioned in the testing plans for tasks 0053-0055 but is not automated in any CI workflow. PRs modifying `infra/terraform/` files should trigger validation to catch syntax and configuration errors before merge.

## Scope
### In scope
- New `infra-ci.yml` workflow triggered on PRs that modify `infra/terraform/**`
- Matrix strategy to validate bootstrap, staging, and production configs
- Uses `terraform init -backend=false` (no real backend needed for validation)

### Out of scope
- `terraform plan` against a real GCP project (requires credentials)
- Terratest or module unit tests

## Requirements
- R1: Terraform validate runs on PRs that modify infra files
- R2: All three configs (bootstrap, staging, production) are validated

## Acceptance Criteria
- [x] AC1: New workflow file `.github/workflows/infra-ci.yml` exists
- [x] AC2: Triggers on `pull_request` with path filter `infra/terraform/**`
- [x] AC3: Matrix validates bootstrap, staging, and production configs
- [x] AC4: Uses `terraform init -backend=false && terraform validate`

## Constraints
- Must not require GCP credentials
- Must not create any real infrastructure

## Implementation Steps
1. Create `.github/workflows/infra-ci.yml` with path-filtered trigger
2. Add matrix strategy for bootstrap, staging, production
3. Use `hashicorp/setup-terraform@v3` and `terraform init -backend=false`

## Testing Plan
- Manual checks: Verify YAML syntax validity

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
