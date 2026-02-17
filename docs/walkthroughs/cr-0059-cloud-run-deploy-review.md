# Walkthrough: CR-0059 — Cloud Run Deployment Pipeline Review

## Task Reference
- Task: `docs/tasks/cr-0059-cloud-run-deploy-review.md`
- PR: #259

## What Changed

### Fix #1-3: copy-image-to-ar — env var injection hardening
All three steps (pull, tag, push) previously used `${{ inputs.* }}` directly in shell `run:` blocks. GitHub Actions performs string substitution before the shell is invoked, making this vulnerable to command injection. Refactored all steps to map inputs to environment variables, which the shell handles safely.

### Fix #4: verify-health — env vars + bash array, remove eval
The health check script used `${{ inputs.* }}` directly and built a curl command string executed via `eval`. This was fragile (quoting issues with the Authorization header) and an injection vector. Refactored to:
- Map all inputs to `env:` block
- Use a bash array (`CURL_ARGS`) instead of a string
- Invoke `curl` directly with `"${CURL_ARGS[@]}"` instead of `eval`
- Fail explicitly if `use-auth=true` but identity token cannot be fetched

### Fix #5: deploy.yml — GHCR login before image poll
The `docker manifest inspect` poll ran before the GHCR login step. For private GitHub packages, this would fail with 401 even when the image exists. Moved the GHCR login step before the availability poll.

### Fix #6: WIF attribute_condition — restrict to trusted refs
The WIF provider only restricted by repository, allowing any branch/workflow to impersonate the deploy SA. Tightened the `attribute_condition` to also require `refs/heads/main` or `refs/tags/v*`.

## Validation
- `terraform init -backend=false && terraform validate` passes for bootstrap
- All YAML files maintain valid structure
- No secrets committed

## Deferred Items
- Hardcoded SA names in bootstrap (item #7) — deferred to infrastructure hardening task
- Project-level AR writer role (item #8) — deferred to same hardening task
