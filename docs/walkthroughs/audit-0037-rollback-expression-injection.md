# Walkthrough: audit-0037 — Harden rollback.yml expression injection

## Summary

Mapped all `${{ }}` expressions in shell `run:` blocks to step-level `env:` variables throughout `.github/workflows/rollback.yml`, consistent with the deploy.yml hardening in audit-0036.

## Changes

### `.github/workflows/rollback.yml`

| Job | Step | Expressions mapped |
|-----|------|--------------------|
| `resolve-services` | Build service matrix | `inputs.services` |
| `rollback` | Verify image exists in AR | `vars.GCP_REGION`, `vars.GCP_PROJECT_ID`, `vars.AR_REPO`, `matrix.service`, `inputs.image_tag` |
| `rollback` | Rollback Cloud Run service | `inputs.environment`, `matrix.service`, `vars.*`, `inputs.image_tag` |
| `rollback` | Get service URL | `inputs.environment`, `matrix.service`, `vars.GCP_REGION` |
| `rollback` | Determine health path | `matrix.service` |
| `summary` | Write summary | `inputs.environment`, `inputs.image_tag`, `inputs.services`, `needs.rollback.result` |

## Verification

```bash
npx yaml-lint .github/workflows/rollback.yml
```
