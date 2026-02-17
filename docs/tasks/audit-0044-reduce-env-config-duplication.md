# Audit Action: audit-0044-reduce-env-config-duplication

## Metadata
- ID: audit-0044
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P3
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "Reduce staging/production config duplication"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0044-reduce-env-config-duplication.md`

## Goal
Extract shared infrastructure setup (GCP API enablement, Artifact Registry, deploy SA binding) into a reusable `environment-base` module to reduce duplication between staging and production configs.

## Context
Both staging and production `main.tf` files contained identical blocks for enabling GCP APIs (~15 lines), creating the Artifact Registry repository (~14 lines), and binding the deploy SA to AR (~8 lines). This ~37-line duplication per environment increased maintenance burden and risked drift if one environment was updated without the other.

## Scope
### In scope
- Extract GCP API enablement, AR repo, and deploy SA AR writer binding into `environment-base` module
- Update staging and production `main.tf` to use the new module
- Update `outputs.tf` references in both environments

### Out of scope
- Full Terragrunt adoption or shared variable files
- Deduplicating Cloud Run, Cloud SQL, Redis, or other module compositions (these differ meaningfully between environments)

## Requirements
- R1: Create `infra/terraform/modules/environment-base/` with main.tf, variables.tf, outputs.tf
- R2: Module handles GCP API enablement, AR repo creation, and deploy SA AR writer binding
- R3: Both staging and production configs use the new module
- R4: `terraform validate` passes for both environments

## Acceptance Criteria
- [x] AC1: `environment-base` module exists with proper inputs/outputs
- [x] AC2: Staging `main.tf` uses `module "base"` instead of inline API/AR/SA blocks
- [x] AC3: Production `main.tf` uses `module "base"` instead of inline API/AR/SA blocks
- [x] AC4: `terraform validate` passes for both staging and production

## Constraints
- C1: Must not change any resource IDs or naming patterns (would cause state drift for existing deployments)
- C2: Module must accept `deploy_sa_email` as optional (default "") for backward compatibility

## Implementation Steps
1. Create `infra/terraform/modules/environment-base/main.tf` with API enablement, AR repo, and deploy SA writer resources
2. Create `variables.tf` with project_id, region, environment, deploy_sa_email inputs
3. Create `outputs.tf` exposing ar_repository_id and apis_ready
4. Update staging `main.tf`: replace inline blocks with `module "base"`, update depends_on and AR references
5. Update production `main.tf`: same changes as staging
6. Update `outputs.tf` in both environments to reference `module.base.ar_repository_id`
7. Run `terraform validate` on both environments

## Testing Plan
- Unit tests: N/A (Terraform module)
- Integration tests: N/A
- Manual checks: `terraform init -backend=false && terraform validate` on both staging and production

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes (`terraform validate` on both environments)
- [x] Tests pass (N/A)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
