# Code Review: CR-0072 — Secret Rotation Guide Review

## Metadata
- PR: #287 (`feat/0073-secret-rotation-guide`)
- Target branch: `main`
- Reviewers: gemini-code-assist (2 inline), copilot-pull-request-reviewer (10 inline)
- Total comments: 12 inline review comments, 2 review bodies, 2 issue comments (non-actionable)

## Changed Files
- `docs/production-hardening.md` — New production hardening guide
- `docs/walkthroughs/0073-secret-rotation-guide.md` — Walkthrough
- `docs/tasks/0073-secret-rotation-guide.md` — Task marked DONE
- `docs/roadmap-v1.md` — Roadmap updated
- `docs/backlog/epic-013-presale-hardening.md` — Epic updated
- `infra/terraform/environments/staging/variables.tf` — `secret_versions` variable
- `infra/terraform/environments/staging/main.tf` — Secret version pinning + AIAPI_Jwt__Secret
- `infra/terraform/environments/staging/terraform.tfvars.example` — Example
- `infra/terraform/environments/production/variables.tf` — `secret_versions` variable
- `infra/terraform/environments/production/main.tf` — Secret version pinning + AIAPI_Jwt__Secret
- `infra/terraform/environments/production/terraform.tfvars.example` — Example

## Comment Resolution Plan

### MUST_FIX

- [x] **Partial map override breaks Terraform lookups** (Comments #3, #7, #11, #12)
  - Copilot identified that `var.secret_versions["key"]` fails if tfvars provides a partial map (Terraform replaces entire default, not merge).
  - Fix: Switch all accesses in both main.tf files to `lookup(var.secret_versions, "key", "latest")`. This allows partial overrides safely.
  - Files: `staging/main.tf`, `production/main.tf`

- [x] **tfvars.example shows partial map without warning** (Comments #4, #8)
  - The examples show only a few keys, which would break direct-index access. After switching to lookup(), partial maps are safe.
  - Fix: Update both tfvars.example files to clarify partial overrides are supported.
  - Files: `staging/terraform.tfvars.example`, `production/terraform.tfvars.example`

### SHOULD_FIX

- [x] **DB credential rotation uses hardcoded placeholder** (Comment #1, Gemini)
  - `NEW_SECURE_PASSWORD` appears as literal text in 3 places — error-prone. Other sections use shell variables.
  - Fix: Use shell variable `$NEW_PASSWORD` consistently, add `openssl rand` generation step.
  - File: `docs/production-hardening.md` (section 5)

- [x] **Terraform pinning docs show misleading variable default** (Comment #2, Gemini)
  - Doc example shows hardcoded version numbers as defaults, but actual implementation defaults to `"latest"`.
  - Fix: Restructure section to show actual variable definition, then tfvars override example.
  - File: `docs/production-hardening.md` (Terraform section)

- [x] **CSRF recommendation Terraform snippet incomplete** (Comment #5, Copilot)
  - Only shows adding secret shell, not the Cloud Run env var mapping needed to actually use it.
  - Fix: Expand snippet to include Cloud Run `secret_env_vars` mapping and `secret_versions` entry.
  - File: `docs/production-hardening.md` (section 2)

- [x] **PR checklist shows ai-api build/test unchecked** (Comment #6, Copilot)
  - Walkthrough says both were run. PR body has ai-api boxes unchecked.
  - Fix: Update PR body to check ai-api boxes.

- [x] **JwtTokenService also needs dual-key update** (Comment #9, Copilot)
  - Dual-key docs only mention DependencyInjection.cs, but JwtTokenService also validates tokens for email verification and password reset.
  - Fix: Add note about updating JwtTokenService validation too.
  - File: `docs/production-hardening.md` (section 1)

- [x] **JWT PreviousSecret creation bypasses Terraform** (Comment #10, Copilot)
  - Rotation procedure uses `gcloud secrets create` directly, contradicting Terraform-managed approach.
  - Fix: Add note recommending managing the previous-secret in Terraform to avoid state drift.
  - File: `docs/production-hardening.md` (section 1)

### OUT_OF_SCOPE
None.

## Behavioral Parity Checks
- [x] Redirect parity (`next` propagation): N/A — documentation-only PR, no auth flow changes
- [x] Locale source correctness: N/A — no localized flows changed
- [x] API/UI contract parity: N/A — no API or UI changes (Terraform + docs only)
- [x] Test parity: N/A — no behavior changes to test
