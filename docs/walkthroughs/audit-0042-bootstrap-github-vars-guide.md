# Walkthrough: audit-0042 — Add GitHub variables guidance to bootstrap tfvars

## Summary

Added a comment block to `infra/terraform/bootstrap/terraform.tfvars.example` documenting all GitHub repository variables that must be configured after bootstrap `terraform apply`.

## Changes

### `infra/terraform/bootstrap/terraform.tfvars.example`

Added a comment section listing:
- **From Terraform outputs:** `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_DEPLOY_SERVICE_ACCOUNT`
- **Manual configuration:** `GCP_PROJECT_ID`, `GCP_REGION`, `AR_REPO`, `NEXT_PUBLIC_WEB_URL`, `NEXT_PUBLIC_AI_API_URL`, `NEXT_PUBLIC_ORGS_API_URL`
- Guidance to use GitHub Environments for per-environment values

## Verification

Read the file and confirm all variables referenced in `deploy.yml` via `${{ vars.* }}` are documented.
