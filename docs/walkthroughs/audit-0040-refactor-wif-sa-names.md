# Walkthrough: audit-0040 — Refactor hardcoded SA names in WIF config

## Summary

Replaced the hardcoded list of 6 Cloud Run service account names in `deploy_act_as` with a computed local using `setproduct(environments, services)`.

## Before

```hcl
resource "google_service_account_iam_member" "deploy_act_as" {
  for_each = toset([
    "staging-saastemplate-web",
    "staging-saastemplate-ai-api",
    "staging-saastemplate-orgs-api",
    "production-saastemplate-web",
    "production-saastemplate-ai-api",
    "production-saastemplate-orgs-api",
  ])
  ...
}
```

## After

```hcl
locals {
  deploy_environments = ["staging", "production"]
  deploy_services     = ["web", "ai-api", "orgs-api"]
  cloud_run_sa_names  = [for pair in setproduct(local.deploy_environments, local.deploy_services) : "${pair[0]}-saastemplate-${pair[1]}"]
}

resource "google_service_account_iam_member" "deploy_act_as" {
  for_each = toset(local.cloud_run_sa_names)
  ...
}
```

## Files Changed

| File | Change |
|------|--------|
| `infra/terraform/bootstrap/workload-identity.tf` | Replaced hardcoded list with `setproduct`-based locals |

## Verification

```bash
cd infra/terraform/bootstrap && terraform init -backend=false && terraform validate
# Success! The configuration is valid.
```

The `for_each` keys are identical before and after, so no Terraform state changes are needed.
