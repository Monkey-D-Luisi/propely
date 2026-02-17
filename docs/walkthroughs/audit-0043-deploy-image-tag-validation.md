# Walkthrough: audit-0043 — Add input validation for workflow_dispatch image_tag

## Summary

Added a validation step to the `prepare` job in `deploy.yml` that rejects `image_tag` inputs not matching a 40-character hex SHA format.

## Changes

### `.github/workflows/deploy.yml`

New step added before "Resolve deployment parameters":

```yaml
- name: Validate workflow_dispatch inputs
  if: github.event_name == 'workflow_dispatch'
  env:
    INPUT_TAG: ${{ inputs.image_tag }}
  run: |
    if ! echo "$INPUT_TAG" | grep -qE '^[0-9a-f]{40}$'; then
      echo "::error::image_tag must be a full 40-character commit SHA (got: '$INPUT_TAG')"
      exit 1
    fi
```

## Design Decisions

1. **Separate step** — Keeps validation logic isolated from the resolve step. Clear step name makes failures easy to diagnose.
2. **Only on `workflow_dispatch`** — `workflow_run` and `push` triggers use GitHub-provided SHAs that are always valid.
3. **Env var pattern** — Consistent with audit-0036 hardening. `INPUT_TAG` is never injected into the script text.
4. **Strict regex** — `^[0-9a-f]{40}$` matches only full commit SHAs. Rejects short SHAs, tags, branch names, and injection attempts.

## Verification

```bash
npx yaml-lint .github/workflows/deploy.yml
```
