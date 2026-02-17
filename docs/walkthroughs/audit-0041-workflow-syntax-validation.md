# Walkthrough: audit-0041 — Add workflow syntax validation to CI

## Summary

Added an `actionlint` job to `.github/workflows/infra-ci.yml` and extended path triggers to cover `.github/workflows/**` and `.github/actions/**`.

## Changes

### `.github/workflows/infra-ci.yml`

- **Path triggers expanded:** Now triggers on changes to `infra/terraform/**`, `.github/workflows/**`, and `.github/actions/**`.
- **New `actionlint` job:** Uses `raven-actions/actionlint@v2` with `fail-on-error: true`. Runs independently of the `terraform-validate` job.

## Design Decisions

1. **Same workflow file** — Both Terraform validate and actionlint are CI validation concerns for infrastructure/CI files. Grouping them avoids workflow proliferation.
2. **`raven-actions/actionlint@v2`** — Well-maintained wrapper around `rhysd/actionlint`. Handles installation and output formatting.
3. **No custom config** — Default actionlint rules are sufficient. Can add `.actionlint.yml` later if needed.

## Verification

```bash
npx yaml-lint .github/workflows/infra-ci.yml
```
