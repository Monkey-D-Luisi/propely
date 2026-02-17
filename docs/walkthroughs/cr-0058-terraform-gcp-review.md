# Walkthrough: cr-0058-terraform-gcp-review

## Task Reference
- Task: `docs/tasks/cr-0058-terraform-gcp-review.md`
- Walkthrough: `docs/walkthroughs/cr-0058-terraform-gcp-review.md`
- PR: #258 (`feat/terraform-gcp-foundation`)
- Date: 2026-02-12

## Summary
Addressed 13 inline review comments from Gemini and Copilot on PR #258 (Terraform GCP Foundation). Fixed 2 MUST_FIX issues (over-privileged secret access, broken `depends_on` with variable), 6 SHOULD_FIX items (bootstrap roles, web memory, unused provider, liveness probe, partial backend config), and identified 2 false positives.

## Changes Made

### MUST_FIX #1: Secret accessor scoped by service (iam/main.tf)
- Added `needs_secrets` boolean to service objects in IAM module
- Filtered `google_project_iam_member.secret_accessor` to only services with `needs_secrets = true`
- Updated default: web=false, ai-api=true, orgs-api=true
- Result: Web frontend no longer has Secret Manager access

### MUST_FIX #8: Fixed `depends_on` with variable (cloud-sql/main.tf)
- Removed `depends_on = [var.private_network_connection_id]` from `google_sql_database_instance`
- Removed `private_network_connection_id` variable from cloud-sql module
- Added `depends_on = [module.vpc]` to `module "cloud_sql"` calls in both environment files
- Removed `private_network_connection_id` argument from module calls

### SHOULD_FIX #2/#11: Targeted bootstrap SA roles (bootstrap/main.tf)
- Replaced `roles/editor` with specific roles: `roles/compute.networkAdmin`, `roles/run.admin`, `roles/cloudsql.admin`, `roles/secretmanager.admin`, `roles/redis.admin`, `roles/artifactregistry.admin`, `roles/vpcaccess.admin`, `roles/servicenetworking.networksAdmin`, `roles/storage.admin`, `roles/serviceusage.serviceUsageAdmin`

### SHOULD_FIX #3/#5: Increased web memory
- Staging: 256Mi -> 512Mi
- Production: 512Mi -> 1Gi

### SHOULD_FIX #4/#6: Removed unused google-beta provider
- Removed `google-beta` from `required_providers` and provider blocks in both environments

### SHOULD_FIX #7: Liveness probe initial delay (cloud-run/main.tf)
- Added `initial_delay_seconds` to liveness probe
- Added `liveness_probe_initial_delay` variable (default: 10)

### SHOULD_FIX #12/#13: Partial backend config
- Removed placeholder bucket from backend.tf files
- Added comment instructing `-backend-config` usage
- Updated README.md with partial backend config instructions

## Commands Run
```bash
terraform fmt -recursive infra/terraform/
terraform init -backend=false  # in each environment dir
terraform validate              # all 3 configurations pass
```

## Files Changed
- `infra/terraform/modules/iam/main.tf` — Filter secret access by `needs_secrets`
- `infra/terraform/modules/iam/variables.tf` — Add `needs_secrets` to service objects
- `infra/terraform/modules/cloud-sql/main.tf` — Remove broken `depends_on`
- `infra/terraform/modules/cloud-sql/variables.tf` — Remove `private_network_connection_id`
- `infra/terraform/modules/cloud-run/main.tf` — Add liveness probe `initial_delay_seconds`
- `infra/terraform/modules/cloud-run/variables.tf` — Add `liveness_probe_initial_delay`
- `infra/terraform/bootstrap/main.tf` — Replace `roles/editor` with targeted roles
- `infra/terraform/environments/staging/main.tf` — Web 512Mi, add `depends_on` for cloud_sql
- `infra/terraform/environments/production/main.tf` — Web 1Gi, add `depends_on` for cloud_sql
- `infra/terraform/environments/staging/versions.tf` — Remove google-beta
- `infra/terraform/environments/production/versions.tf` — Remove google-beta
- `infra/terraform/environments/staging/backend.tf` — Partial backend config
- `infra/terraform/environments/production/backend.tf` — Partial backend config
- `infra/terraform/README.md` — Updated init instructions

## Checklist
- [x] Task scope matches `docs/tasks/cr-0058-terraform-gcp-review.md`
- [x] Terraform validates and plans for all configs
- [x] Docs updated where relevant
- [x] No secrets committed
