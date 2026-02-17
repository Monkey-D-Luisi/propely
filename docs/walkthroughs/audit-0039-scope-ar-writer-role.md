# Walkthrough: audit-0039 — Scope AR writer role to specific repositories

## Summary

Moved `roles/artifactregistry.writer` from a project-level binding in bootstrap to repository-scoped bindings in each environment config. The deploy SA now only has write access to the specific AR repository for each environment.

## Design Decisions

1. **Binding in environment configs (not bootstrap)** — AR repos are created by environment configs, so IAM bindings scoped to those repos belong alongside them. Bootstrap runs before environments exist, so repository-scoped bindings there would require the repos to already exist.

2. **`count` conditional** — Uses `count = var.deploy_sa_email != "" ? 1 : 0` so the binding is only created when the deploy SA email is provided. This makes the variable optional and prevents errors during initial `terraform apply` before the deploy SA exists.

3. **Default empty string** — `deploy_sa_email` defaults to `""` so existing deployments aren't broken. Users set it after running bootstrap.

## Files Changed

| File | Change |
|------|--------|
| `infra/terraform/bootstrap/workload-identity.tf` | Removed `roles/artifactregistry.writer` from `deploy_roles` |
| `infra/terraform/environments/staging/main.tf` | Added `google_artifact_registry_repository_iam_member.deploy_ar_writer` |
| `infra/terraform/environments/staging/variables.tf` | Added `deploy_sa_email` variable |
| `infra/terraform/environments/staging/terraform.tfvars.example` | Added `deploy_sa_email` example |
| `infra/terraform/environments/production/main.tf` | Same as staging |
| `infra/terraform/environments/production/variables.tf` | Same as staging |
| `infra/terraform/environments/production/terraform.tfvars.example` | Same as staging |

## Verification

```bash
cd infra/terraform/bootstrap && terraform init -backend=false && terraform validate
cd infra/terraform/environments/staging && terraform init -backend=false && terraform validate
cd infra/terraform/environments/production && terraform init -backend=false && terraform validate
```

All three pass.
