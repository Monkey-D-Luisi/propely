# Code Review: CR-0059 — Cloud Run Deployment Pipeline

## Metadata
- PR: #259 — `feat(infra): add Cloud Run deployment pipeline (#0054)`
- Branch: `feat/cloud-run-deploy` → `main`
- CI Status: Detect Changes SUCCESS, service builds SKIPPED (no app changes), claude-review FAILURE (expected — new workflow files)

## Changed Files
- `.github/actions/copy-image-to-ar/action.yml`
- `.github/actions/verify-health/action.yml`
- `.github/workflows/deploy.yml`
- `.github/workflows/rollback.yml`
- `docs/backlog/epic-009-cloud-infra.md`
- `docs/tasks/0054-cloud-run-deploy.md`
- `docs/walkthroughs/0054-cloud-run-deploy.md`
- `infra/terraform/bootstrap/outputs.tf`
- `infra/terraform/bootstrap/terraform.tfvars.example`
- `infra/terraform/bootstrap/variables.tf`
- `infra/terraform/bootstrap/workload-identity.tf`

## Comment Sources
- Inline review comments: 10
- General reviews: 2 (Gemini summary, Copilot summary — no additional issues beyond inline comments)
- Issue comments: 2 (ChatGPT Codex usage limit notice, Gemini PR summary — not actionable)

## Comment Classification

### MUST_FIX

1. **[Gemini #2798596011] copy-image-to-ar: pull step command injection**
   - File: `.github/actions/copy-image-to-ar/action.yml:17`
   - Issue: `${{ inputs.source-image }}` directly in shell is an injection vector
   - Fix: Map inputs to env vars

2. **[Gemini #2798596017] copy-image-to-ar: tag step command injection**
   - File: `.github/actions/copy-image-to-ar/action.yml:21`
   - Issue: Both `source-image` and `target-image` inputs directly in shell
   - Fix: Map inputs to env vars

3. **[Gemini #2798596021] copy-image-to-ar: push step command injection**
   - File: `.github/actions/copy-image-to-ar/action.yml:25`
   - Issue: `target-image` input directly in shell
   - Fix: Map input to env var

4. **[Gemini #2798596007 + Copilot #2798615721] verify-health: injection + eval usage**
   - File: `.github/actions/verify-health/action.yml:43,51`
   - Issue: Direct `${{ inputs.* }}` substitution in shell + `eval` for curl is fragile and injection-prone
   - Fix: Map inputs to env vars, use bash array for curl args, remove eval, fail explicitly on missing token

5. **[Copilot #2798615698] deploy.yml: GHCR login before image poll**
   - File: `.github/workflows/deploy.yml:107`
   - Issue: `docker manifest inspect` runs before GHCR login; fails with 401 for private packages
   - Fix: Move GHCR login step before the availability poll

### SHOULD_FIX

6. **[Gemini #2798596029 + Copilot #2798615746] WIF attribute_condition too broad**
   - File: `infra/terraform/bootstrap/workload-identity.tf:31`
   - Issue: Any workflow/branch can impersonate the deploy SA
   - Fix: Restrict to `refs/heads/main` and `refs/tags/v*`

### OUT_OF_SCOPE

7. **[Gemini #2798596026] Hardcoded SA names in bootstrap**
   - File: `infra/terraform/bootstrap/workload-identity.tf:76`
   - Issue: `deploy_act_as` hardcodes environment-level SA names, creating tight coupling
   - Rationale: Moving these bindings to staging/production configs requires adding `deploy_sa_email` as a cross-config input variable and restructuring IAM bindings across multiple Terraform root modules. The current approach follows a deterministic naming convention documented in the project. Deferred to a future infrastructure hardening task.

8. **[Gemini #2798596033] AR writer at project level too broad**
   - File: `infra/terraform/bootstrap/workload-identity.tf:58`
   - Issue: `roles/artifactregistry.writer` grants write access to all AR repos in the project
   - Rationale: Scoping to specific AR repos requires `google_artifact_registry_repository_iam_member` resources in the environment configs, which pairs with the SA refactor in item 7. The deploy SA is already restricted by WIF to specific refs (after fix #6). Deferred to the same future hardening task.

## Behavioral Parity Checks
- [x] Redirect parity: N/A (infrastructure only, no auth flows)
- [x] Locale source correctness: N/A (no localized content)
- [x] API/UI contract parity: N/A (no API/UI changes)
- [x] Test parity: N/A (infrastructure only, no testable behavior changes)

## Comment Resolution Plan

- [x] Fix #1-3: Refactor copy-image-to-ar to use env vars for all 3 steps
- [x] Fix #4: Refactor verify-health to use env vars + bash array, remove eval, fail on missing token
- [x] Fix #5: Move GHCR login before image availability poll in deploy.yml
- [x] Fix #6: Tighten WIF attribute_condition to main + v* tags
- [x] Reply to all 10 inline comments
