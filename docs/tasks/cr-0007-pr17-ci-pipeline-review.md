# Task: cr-0007 — PR #17 Review

## Metadata
- PR: #17 (`feat/0006-ci-pipeline-updates`)
- Target branch: `main`
- CI status: Running (matrix jobs queued/in-progress, Infrastructure CI passed, actionlint passed)

## Changed Files
- `.github/workflows/ci.yml` — Refactored to matrix strategy for 6 .NET services
- `.github/workflows/deploy.yml` — Extended deploy matrix to 6 services
- `.github/workflows/publish.yml` — Extended publish matrix to 6 services
- `.github/workflows/rollback.yml` — Extended rollback options to 6 services
- `docker-compose.ci.yml` — CI container overrides for 4 new services
- `docs/backlog/epic-P0-foundation.md` — Task 0.6 status → DONE
- `docs/tasks/0006-ci-pipeline-updates.md` — New task doc
- `docs/walkthroughs/0006-ci-pipeline-updates.md` — New walkthrough

---

## Section 1: Agent Review Findings

### Finding A1: `web` job lost change detection
- **File**: `.github/workflows/ci.yml`
- **Severity**: SHOULD_FIX
- **Category**: Code Quality
- **Description**: The `web` job previously had `needs: changes` and `if: needs.changes.outputs.web == 'true' || github.event_name == 'push'`. Both were removed in the refactor. The web job now runs on every PR even when no web files changed.
- **Fix**: Restore the `needs` and `if` conditions on the `web` job.

### Finding A2: Behavioral parity checks
- **Severity**: N/A
- **Description**: No auth, locale, API-to-UI, or test parity issues. This is a CI-only change with no runtime behavior modifications.

---

## Section 2: Review Comment Threads

### Comment R1 (gemini-code-assist, docker-compose.ci.yml:45) — Volume isolation
- **Classification**: OUT_OF_SCOPE
- **Rationale**: The comment suggests overriding per-service obj volumes (e.g., `properties-api-obj-domain`). However, the existing ai-api and orgs-api obj volumes were also never overridden in docker-compose.ci.yml — this is pre-existing behavior that applies to all services equally, not a regression introduced by this PR. Obj volumes are build caches; removing them just triggers a clean rebuild, which is appropriate for CI.

### Comment R2 (chatgpt-codex-connector, deploy.yml:93) — Deploy matrix includes unprovisioned services
- **Classification**: MUST_FIX
- **Rationale**: Valid. The Terraform only provisions Cloud Run for ai-api and orgs-api currently. Deploying the 4 new services will hard-fail, causing the overall deploy workflow to report failure. Since `deploy.yml` triggers automatically after `Publish Docker Images` completes on main, this would cause every auto-deploy to show as failed.
- **Fix**: Revert deploy-apis matrix to `[ai-api, orgs-api]` until infrastructure is provisioned.

### Comment R3 (Copilot, ci.yml:209) — E2E cleanup `down --volumes` without CI override
- **Classification**: SHOULD_FIX
- **Rationale**: Valid concern. The first `docker compose --profile apps down --volumes` without the CI override file could remove dev volumes on a self-hosted runner. Pre-existing behavior but worth fixing since we're touching this area.
- **Fix**: Remove `--volumes` from the first `down` command (keep it on the CI-specific `down`).

### Comment R4 (Copilot, docs/tasks/0006-ci-pipeline-updates.md:22) — Task doc scope inconsistency
- **Classification**: SHOULD_FIX
- **Rationale**: Valid. The task doc mentions "per-service change detection" but the implementation uses an aggregate `backend` filter. Should update the scope/approach wording.
- **Fix**: Update task doc to reflect aggregate backend filter approach.

### Comment R5 (Copilot, docs/walkthroughs/0006-ci-pipeline-updates.md:6) — Walkthrough PR link says TBD
- **Classification**: SHOULD_FIX
- **Rationale**: Valid. The walkthrough should reference PR #17.
- **Fix**: Update the PR reference in the walkthrough.

---

## Resolution Plan

### MUST_FIX
- [x] R2: Revert deploy.yml matrix to `[ai-api, orgs-api]` until Cloud Run is provisioned

### SHOULD_FIX
- [x] A1: Restore change detection on `web` job in ci.yml
- [x] R3: Remove `--volumes` from first `down` command in E2E cleanup
- [x] R4: Update task doc scope/approach to reflect aggregate backend filter
- [x] R5: Update walkthrough PR link from TBD to #17

### OUT_OF_SCOPE
- [x] R1: Volume isolation for obj volumes (pre-existing pattern, not a regression)
