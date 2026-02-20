# Walkthrough: cr-0007 — PR #17 Review

## Task Reference
- Task: `docs/tasks/cr-0007-pr17-ci-pipeline-review.md`
- PR: #17 (`feat/0006-ci-pipeline-updates`)
- Date: `2026-02-20`

## Summary
Code review of PR #17 that updates CI/CD pipelines for all 6 services. Addressed 1 MUST_FIX (deploy matrix includes unprovisioned services) and 4 SHOULD_FIX items (web job lost change detection, E2E cleanup volume safety, task doc scope inconsistency, walkthrough PR link placeholder).

## Changes Made

### MUST_FIX
1. **deploy.yml**: Reverted deploy-apis matrix from 6 services back to `[ai-api, orgs-api]`. The 4 new services don't have Cloud Run instances yet (Terraform not provisioned). Deploying them would cause automatic deploy failures.

### SHOULD_FIX
2. **ci.yml**: Restored `needs: changes` and `if` condition on the `web` job so it only runs when web files change (or on push to main).
3. **ci.yml**: Removed `--volumes` from the first `docker compose down` in E2E cleanup to prevent accidental dev volume removal on self-hosted runners.
4. **docs/tasks/0006-ci-pipeline-updates.md**: Updated scope/approach wording to reflect aggregate `backend` filter instead of per-service change detection.
5. **docs/walkthroughs/0006-ci-pipeline-updates.md**: Updated PR reference from TBD to #17.

### OUT_OF_SCOPE (documented, not actioned)
- Volume isolation for per-service obj volumes (pre-existing pattern, not a regression)

## Commands Run
```bash
# Verified deploy.yml changes
# Verified ci.yml changes compile correctly
# No runtime code changes — no build/test needed
```

## Files Changed
- `.github/workflows/ci.yml` — Restored web change detection; fixed E2E cleanup volume safety
- `.github/workflows/deploy.yml` — Reverted deploy matrix to provisioned services only
- `docs/tasks/0006-ci-pipeline-updates.md` — Updated scope wording
- `docs/walkthroughs/0006-ci-pipeline-updates.md` — Updated PR link
- `docs/tasks/cr-0007-pr17-ci-pipeline-review.md` — This review task (new)
- `docs/walkthroughs/cr-0007-pr17-ci-pipeline-review.md` — This walkthrough (new)

## Checklist
- [x] Task scope matches review findings
- [x] No secrets committed
- [x] Docs updated where relevant
