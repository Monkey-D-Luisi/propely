# Audit Action: audit-0042-bootstrap-github-vars-guide

## Metadata
- ID: audit-0042
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "Add GitHub variables guidance to bootstrap tfvars"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0042-bootstrap-github-vars-guide.md`

## Goal
Add a comment block to `terraform.tfvars.example` explaining which bootstrap outputs map to GitHub variables.

## Context
After running `terraform apply` on bootstrap, users need to configure GitHub repository variables for the deploy workflow. The mapping between Terraform outputs and GitHub variable names was only documented in the `workload-identity.tf` header comment, not in the tfvars example where users first interact with the config.

## Scope
### In scope
- Comment block in `terraform.tfvars.example` listing all required GitHub variables

### Out of scope
- Automating GitHub variable creation

## Requirements
- R1: Document all GitHub variables needed by deploy.yml
- R2: Indicate which come from Terraform outputs vs manual configuration

## Acceptance Criteria
- [x] AC1: Comment block lists `GCP_WORKLOAD_IDENTITY_PROVIDER` and `GCP_DEPLOY_SERVICE_ACCOUNT` with output source
- [x] AC2: Comment block lists `GCP_PROJECT_ID`, `GCP_REGION`, `AR_REPO`, `NEXT_PUBLIC_*` variables

## Constraints
- C1: Must be a comment block (not HCL variables) since these are GitHub settings, not Terraform inputs

## Implementation Steps
1. Add comment block after the variables in `terraform.tfvars.example`

## Testing Plan
- Unit tests: N/A
- Integration tests: N/A
- Manual checks: Read the file and verify completeness

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
