# Walkthrough: audit-0038 — Add Terraform validate CI step

## Summary

Created a new `.github/workflows/infra-ci.yml` workflow that runs `terraform validate` on PRs modifying infrastructure files. Uses a matrix strategy to validate all three Terraform configurations (bootstrap, staging, production) in parallel.

## Design Decisions

1. **Separate workflow (not added to ci.yml)** — Infrastructure CI has different path triggers and tooling requirements than application CI. Keeping it separate avoids adding Terraform setup overhead to PRs that only change application code.

2. **Matrix strategy** — Three configs are validated in parallel: bootstrap, staging, and production. Each has its own directory and provider configuration.

3. **`-backend=false`** — Skips backend initialization, which would require a real GCS bucket. Validation only checks syntax and provider schema, which doesn't need a backend.

4. **Terraform version `~> 1.9`** — Matches the `required_version = ">= 1.9.0"` constraint in all Terraform configs.

## Files Created

| File | Purpose |
|------|---------|
| `.github/workflows/infra-ci.yml` | Infrastructure CI workflow with Terraform validate matrix |

## Verification

```bash
# Verify YAML syntax
npx yaml-lint .github/workflows/infra-ci.yml

# Local verification (requires Terraform installed)
cd infra/terraform/bootstrap && terraform init -backend=false && terraform validate
cd infra/terraform/environments/staging && terraform init -backend=false && terraform validate
cd infra/terraform/environments/production && terraform init -backend=false && terraform validate
```
