# Walkthrough: cr-0003-pr13-rename-review

## Task Reference
- Task: `docs/tasks/cr-0003-pr13-rename-review.md`
- PR: #13 (`feat/0002-rename-saastemplate-to-propely`)
- Branch: `feat/0002-rename-saastemplate-to-propely`
- Date: 2026-02-20

## Summary
Code review of PR #13 identified 4 categories of missed rename references: `saas-template` (hyphenated Docker image names), JWT audience values, `SaaS Starter Kit` brand name in config/Terraform, and root package.json name. All findings were fixed and validated.

## Context
- The original rename PR correctly targeted `SaasTemplate` (PascalCase) and `saastemplate` (lowercase no separator) but missed the hyphenated variant `saas-template` and the spaced brand name `SaaS Starter Kit`.

## Changes Made

### A1: Docker image name prefix `saas-template-` -> `propely-`
- `.github/workflows/deploy.yml`: 6 occurrences
- `.github/workflows/publish.yml`: 2 occurrences
- `.github/workflows/rollback.yml`: 2 occurrences
- `infra/terraform/environments/production/main.tf`: 3 occurrences
- `infra/terraform/environments/staging/main.tf`: 3 occurrences

### A2: JWT audience `saas-template` -> `propely`
- `services/ai-api/src/Propely.AiApi.Api/appsettings.json`
- `services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json`
- `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Services/JwtTokenService.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Infrastructure/Services/JwtTokenServiceTests.cs`

### A3: Brand name `SaaS Starter Kit` -> `Propely`
- `services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json` (SendGrid:FromName)
- `infra/terraform/modules/environment-base/main.tf` (AR description)

### A4: Root package.json name `saas-template` -> `propely`
- `package.json`
- `package-lock.json`

## Commands Run
```bash
# Fix saas-template- Docker image prefix
sed -i 's/saas-template-/propely-/g' .github/workflows/deploy.yml .github/workflows/publish.yml .github/workflows/rollback.yml infra/terraform/environments/production/main.tf infra/terraform/environments/staging/main.tf

# Fix JWT audience
sed -i 's/"saas-template"/"propely"/g' services/ai-api/src/Propely.AiApi.Api/appsettings.json services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs services/orgs-api/src/Propely.OrgsApi.Infrastructure/Services/JwtTokenService.cs services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs services/orgs-api/tests/Propely.OrgsApi.UnitTests/Infrastructure/Services/JwtTokenServiceTests.cs

# Fix SaaS Starter Kit brand name
sed -i 's/SaaS Starter Kit/Propely/g' services/orgs-api/src/Propely.OrgsApi.Api/appsettings.json infra/terraform/modules/environment-base/main.tf

# Fix root package.json
sed -i 's/"saas-template"/"propely"/g' package.json package-lock.json

# Build and test
dotnet build services/ai-api/Propely.AiApi.sln
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet test services/ai-api/Propely.AiApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
```

## Validation
- Both builds pass
- All 773 tests pass
- `grep -ri "saas.template\|saas-template"` returns zero hits (excluding EULA.md generic terms)

## Checklist
- [x] Task scope matches cr-* task file
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
