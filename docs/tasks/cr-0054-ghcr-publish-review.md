# Task: CR-0054 — GHCR Publish PR Review

## PR Metadata
- PR: [#253](https://github.com/Monkey-D-Luisi/saas-template/pull/253)
- Target branch: main
- CI: All checks passed

## Changed Files
- `.github/workflows/publish.yml`
- `docs/backlog/epic-008-cicd-release.md`
- `docs/tasks/0050-ghcr-publishing.md`
- `docs/walkthroughs/0050-ghcr-publishing.md`

## Review Comments (5 inline, 2 reviews, 2 issue comments)

### Comment Resolution Plan

#### MUST_FIX
- [x] **Copilot #2797246409** — SHA tag divergence in `workflow_run` context. `docker/metadata-action` defaults to `github.sha` which in `workflow_run` events is the default branch tip, not the triggering commit. Fixed by explicitly passing `workflow_run.head_sha` to the SHA tag.

#### SHOULD_FIX
- [x] **Copilot #2797246433** — Trivy scan should use build digest, not metadata version tag. Fixed by using `steps.build.outputs.digest` from `docker/build-push-action`.
- [x] **Copilot #2797246452** — DoD checklist items checked before workflow actually runs. Unchecked "Workflow runs successfully" and "All three images published to GHCR" with note *(pending first merge to main)*.

#### REJECTED
- **Gemini #2797214440** — Claims `trivy-action@0.28.0` is invalid, suggests `0.22.0`. Rejected: `aquasecurity/trivy-action@0.28.0` is a valid release (Nov 2024). Gemini's knowledge is outdated.
- **Copilot #2797246443** — Missing semver tag strategy. Rejected as OUT_OF_SCOPE: Semantic versioning is task 0051's scope. Task 0050 requirements (R2) only specify SHA + latest.

## Parity Verification
- [x] Redirect parity checked — N/A (no auth changes)
- [x] Locale source correctness checked — N/A (no i18n changes)
- [x] API/UI contract parity checked — N/A (no API/UI changes)
- [x] Test parity checked — N/A (CI/CD workflow only)
