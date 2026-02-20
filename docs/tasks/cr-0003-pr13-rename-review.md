# Code Review: cr-0003-pr13-rename-review

## Metadata
- PR: #13 (`feat/0002-rename-saastemplate-to-propely`)
- Target branch: main
- CI status: Pending (first push)
- Review date: 2026-02-20

## Changed Files
633 files changed (directory/file renames + content replacements). See PR description for full list.

---

## Section 1: Agent Review Findings

### Finding A1: Missed `saas-template` (hyphenated) Docker image names
- **Severity:** MUST_FIX
- **Category:** Code Quality / Completeness
- **Files:** `.github/workflows/deploy.yml` (6 occurrences), `.github/workflows/publish.yml` (2), `.github/workflows/rollback.yml` (2), `infra/terraform/environments/production/main.tf` (3), `infra/terraform/environments/staging/main.tf` (3)
- **Description:** The rename only targeted `SaasTemplate` (PascalCase) and `saastemplate` (no separator). The hyphenated variant `saas-template` used in Docker image names was missed entirely.
- **Fix:** Replace `saas-template-` with `propely-` in all Docker image references.

### Finding A2: Missed JWT audience `saas-template`
- **Severity:** MUST_FIX
- **Category:** Security / Configuration
- **Files:** `services/ai-api/src/Propely.AiApi.Api/appsettings.json`, `services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json`, `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs`, `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs`, `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Services/JwtTokenService.cs`, plus 4 test files
- **Description:** JWT audience value `"saas-template"` was not caught by the rename. This is a security-relevant configuration value that must be consistent across token generation and validation.
- **Fix:** Replace `"saas-template"` with `"propely"` in all JWT audience references.

### Finding A3: Missed `SaaS Starter Kit` brand name
- **Severity:** MUST_FIX
- **Category:** Branding / Completeness
- **Files:** `services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json` (SendGrid:FromName), `infra/terraform/modules/environment-base/main.tf` (AR description)
- **Description:** The literal string `"SaaS Starter Kit"` (with spaces) was not targeted by the rename. It remains in the SendGrid email from-name and Terraform Artifact Registry description.
- **Fix:** Replace `"SaaS Starter Kit"` with `"Propely"` in both locations.

### Finding A4: Root `package.json` name still `saas-template`
- **Severity:** SHOULD_FIX
- **Category:** Code Quality
- **Files:** `package.json`, `package-lock.json`
- **Description:** The root monorepo package.json still uses `"name": "saas-template"`.
- **Fix:** Update to `"name": "propely"` and regenerate package-lock.json.

### Finding A5: `SaaS Starter Kit` in copyright headers, scripts, and frontend i18n
- **Severity:** MUST_FIX
- **Category:** Branding / Completeness
- **Files:** All `.cs` files (copyright headers), all `.tsx`/`.ts` files (copyright headers), `apps/web/messages/en.json`, `apps/web/messages/es.json`, `apps/web/src/app/globals.css`, `scripts/add-license-headers.{ps1,sh}`, `scripts/bootstrap.{ps1,sh}`, `scripts/verify-license-headers.{ps1,sh}`, `scripts/dev-up.{ps1,sh}`, `scripts/seed-demo.{ps1,sh}`, `docker-compose.yml`, `docker-compose.production.yml`, `infra/terraform/modules/redis/main.tf`, `LICENSE`, `Makefile`, `.env`, `.env.docker`, `.env.example`
- **Description:** The brand name `"SaaS Starter Kit"` appeared in copyright headers across all source files, i18n/localization messages, script banners, LICENSE file, and infrastructure config. Additionally, `.env.example` had commented JWT audience references with `saas-template`.
- **Fix:** Replace all occurrences with `"Propely"`.

---

## Section 2: Review Comment Threads

### Copilot Comments (12 inline)

| # | File | Comment | Classification | Rationale |
|---|------|---------|----------------|-----------|
| 1-8 | `.github/workflows/ci.yml` | Solution files referenced at root level | FALSE_POSITIVE | CI jobs already set `working-directory: services/ai-api` and `services/orgs-api`, so `Propely.AiApi.sln` resolves correctly from within those directories |
| 9-10 | `.github/workflows/deploy.yml` | Docker image names still use `saas-template-*` | MUST_FIX | Aligns with Agent Finding A1 |
| 11 | `.github/workflows/rollback.yml` | Docker image names still use `saas-template-*` | MUST_FIX | Aligns with Agent Finding A1 |
| 12 | `infra/terraform/modules/environment-base/main.tf` | Description still says "SaaS Starter Kit" | MUST_FIX | Aligns with Agent Finding A3 |

### Gemini Code Assist Review

| # | Finding | Classification | Rationale |
|---|---------|----------------|-----------|
| G1 | JWT Audience `saas-template` in both appsettings.json | MUST_FIX | Aligns with Agent Finding A2 |
| G2 | SendGrid FromName `SaaS Starter Kit` in orgs-api appsettings.json | MUST_FIX | Aligns with Agent Finding A3 |
| G3 | Terraform AR description `SaaS Starter Kit` | MUST_FIX | Aligns with Agent Finding A3 |

### Gemini Code Assist Issue Comment

Summary/changelog only. No actionable items.

---

## Resolution Plan

### MUST_FIX (blocking)
- [x] A1: Replace `saas-template-` with `propely-` in deploy.yml, publish.yml, rollback.yml, Terraform staging/production main.tf
- [x] A2: Replace `"saas-template"` with `"propely"` in JWT audience (appsettings, DependencyInjection, JwtTokenService, tests)
- [x] A3: Replace `"SaaS Starter Kit"` with `"Propely"` in appsettings.json and Terraform
- [x] A5: Replace `"SaaS Starter Kit"` in all copyright headers, frontend i18n, scripts, LICENSE, Makefile, .env files

### SHOULD_FIX
- [x] A4: Update root package.json name from `saas-template` to `propely`

### FALSE_POSITIVE
- Copilot comments 1-8 on ci.yml: CI jobs use `working-directory` so solution paths are correct relative to the job directory

---

## Validation Results
- `dotnet build services/ai-api/Propely.AiApi.sln`: PASS
- `dotnet build services/orgs-api/Propely.OrgsApi.sln`: PASS
- `dotnet test services/ai-api/Propely.AiApi.sln`: 169 tests passed
- `dotnet test services/orgs-api/Propely.OrgsApi.sln`: 604 tests passed
- `grep -ri "saas.template\|saas-template\|SaasTemplate\|saastemplate"`: Zero hits in source files (excluding EULA.md generic terms and documentation task titles)
