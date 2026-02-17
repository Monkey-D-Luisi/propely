# Code Review: cr-0061 — Audit Epic 009 Batch Remediation

## PR Metadata
- **PR:** #261 — fix: audit epic 009 batch remediation (audit-0036 through audit-0044)
- **Branch:** `fix/audit-epic-009-batch` → `main`
- **CI Status:** FAILING — Terraform Validate (staging, production), actionlint

## Changed Files
- `.agent.md` — governance priority update
- `.agent/rules/autonomous-workflow.md` — roadmap-first task ordering
- `.github/workflows/deploy.yml` — expression injection hardening + image_tag validation
- `.github/workflows/infra-ci.yml` — new terraform validate + actionlint CI workflow
- `.github/workflows/rollback.yml` — expression injection hardening
- `docs/audits/epic-009-executive-summary.md` — new audit summary
- `docs/tasks/audit-0036-*.md` through `audit-0044-*.md` — 9 audit task docs
- `docs/walkthroughs/audit-0036-*.md` through `audit-0044-*.md` — 9 walkthrough docs
- `infra/terraform/bootstrap/terraform.tfvars.example` — GitHub vars guidance
- `infra/terraform/bootstrap/workload-identity.tf` — scoped AR role + setproduct SA names
- `infra/terraform/environments/production/main.tf` — use environment-base module
- `infra/terraform/environments/production/outputs.tf` — update AR reference
- `infra/terraform/environments/production/terraform.tfvars.example` — deploy_sa_email
- `infra/terraform/environments/production/variables.tf` — deploy_sa_email
- `infra/terraform/environments/staging/main.tf` — use environment-base module
- `infra/terraform/environments/staging/outputs.tf` — update AR reference
- `infra/terraform/environments/staging/terraform.tfvars.example` — deploy_sa_email
- `infra/terraform/environments/staging/variables.tf` — deploy_sa_email
- `infra/terraform/modules/environment-base/main.tf` — new module
- `infra/terraform/modules/environment-base/outputs.tf` — new module
- `infra/terraform/modules/environment-base/variables.tf` — new module

## Review Comments

### Source 1: Inline review comments
- Count: 0

### Source 2: Reviews
- Count: 2
1. **gemini-code-assist[bot]** — COMMENTED: Positive review, no actionable items. "I have no specific comments as the work is excellent."
2. **copilot-pull-request-reviewer[bot]** — COMMENTED: Positive review, 0 inline comments. 1 suppressed low-confidence comment about bootstrap/environment SA ordering (pre-existing concern, not introduced by this PR).

### Source 3: Issue comments
- Count: 2
1. **chatgpt-codex-connector** — Usage limit notification. Not actionable.
2. **gemini-code-assist** — Summary/changelog. Not actionable.

## CI Failures (Self-Identified Issues)

### F1. `.gitignore` blocks Terraform secrets module from git
- **Files:** `.gitignore:65`, `infra/terraform/modules/secrets/`
- **Problem:** `.gitignore` has `secrets/` which matches any directory named `secrets/` at any depth. This prevents `infra/terraform/modules/secrets/` from being committed. The module exists locally (created in task 0053) but was never tracked in git. CI clones from git and can't find the module → `terraform init` fails with "Unreadable module directory".
- **Root cause:** The gitignore pattern was intended for a root-level `secrets/` directory (actual credentials), but the unanchored pattern also matches the Terraform module.

### F2. SC2129 shellcheck violations in deploy.yml and rollback.yml
- **Files:** `.github/workflows/deploy.yml:77,275`, `.github/workflows/rollback.yml:148`
- **Problem:** actionlint (via shellcheck) flags multiple individual `>> file` redirects. SC2129 recommends grouping them as `{ cmd1; cmd2; } >> file`.
- **Impact:** actionlint CI job fails with `fail-on-error: true`.

## Comment Resolution Plan

### MUST_FIX (CI blockers)
- [x] F1: Fix `.gitignore` pattern `secrets/` → `/secrets/` and commit the Terraform secrets module
- [x] F2: Fix SC2129 in deploy.yml resolve step (line 77) — group `>> "$GITHUB_OUTPUT"` redirects
- [x] F3: Fix SC2129 in deploy.yml summary step (line 275) — group `>> "$GITHUB_STEP_SUMMARY"` redirects
- [x] F4: Fix SC2129 in rollback.yml summary step (line 148) — group `>> "$GITHUB_STEP_SUMMARY"` redirects

### OUT_OF_SCOPE
- [ ] Copilot suppressed comment: `deploy_act_as` in bootstrap references SAs from environment configs → pre-existing architecture, not introduced by this PR. Document for future consideration.

## Behavioral Parity Checks
- [x] Redirect parity checked — N/A (no auth/frontend changes)
- [x] Locale source correctness checked — N/A (no localized flows)
- [x] API/UI contract parity checked — N/A (no API/UI changes)
- [x] Test parity checked — N/A (infra-only changes; validation via `terraform validate`)
