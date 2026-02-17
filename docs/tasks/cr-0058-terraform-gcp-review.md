# Code Review: CR-0058 — Terraform GCP Foundation Review

## Metadata
- CR ID: cr-0058
- PR: #258
- PR Title: feat(infra): add Terraform GCP foundation modules (#0053)
- Target Branch: main
- CI Status: Detect Changes SUCCESS, claude-review SUCCESS, others SKIPPED (IaC only, no app code)
- Reviewers: gemini-code-assist, copilot

## Changed Files (PR #258)
- `infra/terraform/bootstrap/` (4 files)
- `infra/terraform/modules/vpc/` (3 files)
- `infra/terraform/modules/cloud-sql/` (3 files)
- `infra/terraform/modules/cloud-run/` (3 files)
- `infra/terraform/modules/iam/` (3 files)
- `infra/terraform/modules/secrets/` (3 files)
- `infra/terraform/modules/redis/` (3 files)
- `infra/terraform/environments/staging/` (6 files)
- `infra/terraform/environments/production/` (6 files)
- `infra/terraform/README.md`
- `.gitignore`
- `docs/tasks/0053-terraform-gcp.md`
- `docs/backlog/epic-009-cloud-infra.md`
- `docs/walkthroughs/0053-terraform-gcp.md`

## Review Threads (13 inline, 2 reviews, 2 issue comments)

### Comment 1 — Gemini (security-high)
- File: `infra/terraform/modules/iam/main.tf:19`
- Claim: Project-level `roles/secretmanager.secretAccessor` gives ALL service accounts access to ALL secrets. Web frontend doesn't need any secret access.
- **Classification: MUST_FIX**
- Fix: Add `needs_secrets` flag to services list (like `needs_cloudsql`), only grant secret access to API services.

### Comment 2 — Gemini (high)
- File: `infra/terraform/bootstrap/main.tf:65`
- Claim: `roles/editor` too permissive for Terraform automation SA. Should use specific roles.
- **Classification: SHOULD_FIX**
- Fix: Replace `roles/editor` with targeted roles for the resources Terraform manages.

### Comment 3 — Gemini (medium)
- File: `infra/terraform/environments/production/main.tf:284`
- Claim: Web memory 512Mi may be too low for production Next.js.
- **Classification: SHOULD_FIX**
- Fix: Increase production web memory to 1Gi.

### Comment 4 — Gemini (medium)
- File: `infra/terraform/environments/production/versions.tf:12`
- Claim: `google-beta` provider declared but unused.
- **Classification: SHOULD_FIX**
- Fix: Remove `google-beta` from both environment versions.tf files.

### Comment 5 — Gemini (medium)
- File: `infra/terraform/environments/staging/main.tf:280`
- Claim: Web memory 256Mi too low for staging Next.js.
- **Classification: SHOULD_FIX**
- Fix: Increase staging web memory to 512Mi.

### Comment 6 — Gemini (medium)
- File: `infra/terraform/environments/staging/versions.tf:12`
- Claim: Same as #4 — unused `google-beta` provider.
- **Classification: SHOULD_FIX**
- Fix: Same as #4.

### Comment 7 — Gemini (medium)
- File: `infra/terraform/modules/cloud-run/main.tf:101`
- Claim: Liveness probe missing `initial_delay_seconds`. Could restart container prematurely during slow startup.
- **Classification: SHOULD_FIX**
- Fix: Add `initial_delay_seconds` to liveness probe with a configurable variable.

### Comment 8 — Copilot
- File: `infra/terraform/modules/cloud-sql/main.tf:11`
- Claim: `depends_on = [var.private_network_connection_id]` won't work — `depends_on` only accepts resource/module references, not variables.
- **Classification: MUST_FIX**
- Fix: Remove `depends_on` from the cloud-sql resource. Add `depends_on = [module.vpc]` to module calls in environment files. Remove unused `private_network_connection_id` variable.

### Comment 9 — Copilot
- File: `infra/terraform/environments/staging/main.tf:77`
- Claim: Secrets module doesn't exist at `../../modules/secrets`.
- **Classification: FALSE_POSITIVE**
- Rationale: Module exists at `infra/terraform/modules/secrets/`. `terraform validate` passes for all configs.

### Comment 10 — Copilot
- File: `infra/terraform/environments/production/main.tf:80`
- Claim: Same as #9 — missing secrets module.
- **Classification: FALSE_POSITIVE**
- Rationale: Same as #9.

### Comment 11 — Copilot
- File: `infra/terraform/bootstrap/main.tf:66`
- Claim: Same as #2 — `roles/editor` too permissive.
- **Classification: SHOULD_FIX**
- Fix: Same as #2 (duplicate).

### Comment 12 — Copilot
- File: `infra/terraform/environments/staging/backend.tf:3`
- Claim: Placeholder bucket name forces editing tracked files. Suggest partial backend config.
- **Classification: SHOULD_FIX**
- Fix: Use partial backend config — remove bucket from file, pass via `terraform init -backend-config`.

### Comment 13 — Copilot
- File: `infra/terraform/environments/production/backend.tf:3`
- Claim: Same as #12.
- **Classification: SHOULD_FIX**
- Fix: Same as #12 (duplicate).

## Comment Resolution Plan

### MUST_FIX
- [x] Fix #1: Add `needs_secrets` filter to IAM secret_accessor binding
- [x] Fix #8: Remove broken `depends_on` from cloud-sql, add module-level dependency

### SHOULD_FIX
- [x] Fix #2/#11: Replace `roles/editor` with targeted roles in bootstrap
- [x] Fix #3: Increase production web memory to 1Gi
- [x] Fix #5: Increase staging web memory to 512Mi
- [x] Fix #4/#6: Remove unused `google-beta` provider from both environments
- [x] Fix #7: Add `initial_delay_seconds` to liveness probe
- [x] Fix #12/#13: Use partial backend config (remove placeholder bucket)

### FALSE_POSITIVE
- [x] #9/#10: Secrets module exists. Copilot hallucination.

## Behavioral Parity Checks
- [x] Redirect parity: N/A — IaC only, no auth flows
- [x] Locale source: N/A — no localization
- [x] API/UI contract: N/A — no API/UI changes
- [x] Test parity: N/A — Terraform validated via `terraform validate`
