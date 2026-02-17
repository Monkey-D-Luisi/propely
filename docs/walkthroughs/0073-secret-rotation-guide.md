# Walkthrough: 0073-secret-rotation-guide

## Task Reference
- Task: `docs/tasks/0073-secret-rotation-guide.md`
- Walkthrough: `docs/walkthroughs/0073-secret-rotation-guide.md`
- Branch/PR: `feat/0073-secret-rotation-guide`
- Date: `2026-02-14`

## Summary
Created `docs/production-hardening.md` — a comprehensive production hardening guide covering step-by-step secret rotation procedures for all 7 secret types, Terraform Secret Manager version pinning, a pre-deploy security checklist, and production environment variable reference. Also updated Terraform configs to use pinned secret versions and added the missing `AIAPI_Jwt__Secret` injection for ai-api.

## Context
- Background: The product includes Terraform for GCP deployment and Secret Manager references, but lacked documentation on how to rotate secrets, what security configurations to verify before going live, and how to pin Secret Manager versions.
- Problem statement: A buyer evaluating the product for production use needs clear, actionable documentation on secret rotation and security hardening.
- Constraints: Documentation only — no automated rotation implementation. Must be practical for a developer deploying to GCP for the first time.

## Decisions & Trade-offs
- **Decision: Default to `"latest"` in secret_versions variable**
  - Options considered: (a) Default to `"latest"` for backward compatibility, (b) Require explicit version numbers with no default
  - Why this choice: `"latest"` preserves existing behavior. Users who don't set `secret_versions` in their `terraform.tfvars` get the same behavior as before. The guide recommends switching to explicit versions.
  - Consequences: Users must opt-in to pinned versions. This is documented in the guide and tfvars examples.

- **Decision: Add `AIAPI_Jwt__Secret` to Terraform**
  - The ai-api was missing JWT secret injection in both staging and production Terraform configs. Without it, ai-api in production would have no JWT validation. This was a pre-existing gap discovered during codebase exploration.

- **Decision: Document dual-key JWT rotation as a code change, not implement it**
  - The task scope is "documentation only" — no automated rotation. The guide explains the code change needed (`IssuerSigningKeys` instead of `IssuerSigningKey`) and provides the rotation procedure.

## Implementation Notes
- Key changes:
  - Created `docs/production-hardening.md` with 5 major sections
  - Added `secret_versions` variable to both staging and production Terraform
  - Replaced all `version = "latest"` with `var.secret_versions[...]` references
  - Added `AIAPI_Jwt__Secret` injection (both environments) — uses same secret as orgs-api
  - Updated `terraform.tfvars.example` files with commented examples
- Edge cases handled:
  - CSRF secret fallback to JWT secret is documented with recommendation to use dedicated CSRF secret
  - In-memory rate limiting scaling caveat documented
  - ai-api `Security:AllowAnonymous` guard documented
- Known limitations:
  - Dual-key JWT rotation requires a code change (documented but not implemented)
  - No CSRF secret in Secret Manager yet (documented as recommendation)

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "UnitTests|ArchitectureTests"
dotnet test services/ai-api/SaasTemplate.AiApi.sln --filter "UnitTests|ArchitectureTests"
```

## Files Changed
- `docs/production-hardening.md` — New: comprehensive production hardening guide
- `infra/terraform/environments/staging/variables.tf` — Added `secret_versions` variable
- `infra/terraform/environments/production/variables.tf` — Added `secret_versions` variable
- `infra/terraform/environments/staging/main.tf` — Pinned secret versions, added `AIAPI_Jwt__Secret`
- `infra/terraform/environments/production/main.tf` — Pinned secret versions, added `AIAPI_Jwt__Secret`
- `infra/terraform/environments/staging/terraform.tfvars.example` — Added `secret_versions` example
- `infra/terraform/environments/production/terraform.tfvars.example` — Added `secret_versions` example

## Tests
### Unit
- No new code requiring tests (documentation task)
- All existing tests pass: 416 + 5 orgs-api, 88 + 5 ai-api

## Security
- Validation: Guide reviewed against actual codebase patterns (JWT, CSRF, Stripe, OAuth, SendGrid, OpenAI)
- AuthN/AuthZ impact: Identified and fixed missing `AIAPI_Jwt__Secret` in Terraform
- Sensitive data handling: No secrets in documentation — all examples use placeholders

## Follow-ups / Backlog
- [ ] Implement dual-key JWT rotation in code (currently documented as manual code change)
- [ ] Add `orgsapi-csrf-secret` to Terraform secrets module (currently CSRF falls back to JWT secret)
- [ ] Consider Redis-backed rate limiting for multi-instance scenarios

## Checklist
- [x] Task scope matches `docs/tasks/0073-secret-rotation-guide.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
