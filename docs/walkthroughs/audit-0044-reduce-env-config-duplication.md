# Walkthrough: audit-0044 — Reduce Staging/Production Config Duplication

## Summary

Extracted shared infrastructure setup (GCP API enablement, Artifact Registry repository, deploy SA AR writer binding) from both environment `main.tf` files into a reusable `environment-base` Terraform module, reducing ~37 lines of identical code per environment.

## Problem

Both `infra/terraform/environments/staging/main.tf` and `production/main.tf` contained identical blocks:
1. `google_project_service.apis` — enabling 8 GCP APIs (~15 lines)
2. `google_artifact_registry_repository.containers` — creating the AR repo (~14 lines)
3. `google_artifact_registry_repository_iam_member.deploy_ar_writer` — granting deploy SA write access (~8 lines)

This duplication meant any change to API enablement or AR configuration required editing both files, risking drift.

## Solution

### New Module: `infra/terraform/modules/environment-base/`

**`main.tf`** contains:
- `google_project_service.apis` — enables compute, run, sqladmin, secretmanager, vpcaccess, servicenetworking, redis, artifactregistry APIs
- `google_artifact_registry_repository.containers` — creates `saastemplate-{env}-containers` Docker repository
- `google_artifact_registry_repository_iam_member.deploy_ar_writer` — conditionally grants `roles/artifactregistry.writer` to deploy SA (only when `deploy_sa_email != ""`)

**`variables.tf`** inputs:
- `project_id` (string) — GCP project ID
- `region` (string) — GCP region
- `environment` (string) — Environment name (staging, production)
- `deploy_sa_email` (string, default "") — Deploy SA email; empty = skip IAM binding

**`outputs.tf`** exposes:
- `ar_repository_id` — AR repository ID for use in Cloud Run image references
- `apis_ready` — list of enabled APIs (for `depends_on` chains)

### Environment Changes

Both staging and production `main.tf` files:
1. **Removed**: inline `google_project_service.apis`, `google_artifact_registry_repository.containers`, and `google_artifact_registry_repository_iam_member.deploy_ar_writer` blocks
2. **Added**: `module "base"` calling `../../modules/environment-base` with project_id, region, environment, and deploy_sa_email
3. **Updated**: all `depends_on` references from `google_project_service.apis[...]` to `module.base`
4. **Updated**: all AR repo ID references from `google_artifact_registry_repository.containers.repository_id` to `module.base.ar_repository_id`

Both `outputs.tf` files:
- Updated `artifact_registry` output to reference `module.base.ar_repository_id` instead of `google_artifact_registry_repository.containers.repository_id`

## Files Changed

| File | Change |
|------|--------|
| `infra/terraform/modules/environment-base/main.tf` | New — shared API/AR/SA resources |
| `infra/terraform/modules/environment-base/variables.tf` | New — module inputs |
| `infra/terraform/modules/environment-base/outputs.tf` | New — module outputs |
| `infra/terraform/environments/staging/main.tf` | Replaced inline blocks with `module "base"` |
| `infra/terraform/environments/staging/outputs.tf` | Updated AR reference to `module.base` |
| `infra/terraform/environments/production/main.tf` | Replaced inline blocks with `module "base"` |
| `infra/terraform/environments/production/outputs.tf` | Updated AR reference to `module.base` |

## Verification

```bash
cd infra/terraform/environments/staging && terraform init -backend=false && terraform validate
# Success! The configuration is valid.

cd infra/terraform/environments/production && terraform init -backend=false && terraform validate
# Success! The configuration is valid.
```
