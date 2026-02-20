# Walkthrough: 0006-ci-pipeline-updates

## Task Reference
- Task: `docs/tasks/0006-ci-pipeline-updates.md`
- Walkthrough: `docs/walkthroughs/0006-ci-pipeline-updates.md`
- Branch/PR: `feat/0006-ci-pipeline-updates` / TBD
- Date: `2026-02-20`

## Summary
Updated all CI/CD workflows to support the full 6-service Propely topology. Refactored `ci.yml` from separate per-service jobs (`ai-api`, `orgs-api`) to a single `dotnet-services` matrix job covering all 6 .NET services. Updated `publish.yml`, `deploy.yml`, and `rollback.yml` to include the 4 new services. Updated `docker-compose.ci.yml` with CI container name overrides for new services.

## Context
- Background: The monorepo had 6 backend services but CI only covered ai-api, orgs-api, and web
- Problem statement: New services (properties-api, publishing-api, contacts-api, appointments-api) scaffolded in Task 0.3 were not being built, tested, published, deployed, or rollback-supported by CI/CD
- Constraints: Maintain existing CI patterns (coverage checks, vulnerability scanning, Trivy image scanning, artifact uploads)

## Decisions & Trade-offs
- **Decision: Matrix strategy replacing separate jobs**
  - Options considered: (A) Keep separate jobs per service (verbose but granular), (B) Single matrix job for all 6 services (DRY, maintainable), (C) Dynamic matrix based on change detection (complex)
  - Why this choice: Option B balances maintainability with simplicity. All 6 services share identical CI steps (restore, vuln check, build, test, coverage, artifact upload), making a matrix the natural fit. The 4 new services are small scaffolds, so running all 6 on every PR is inexpensive.
  - Consequences / risks: Every PR now builds all 6 services even if only one changed. This is acceptable given the minimal scaffold size and ensures the entire codebase compiles consistently.

- **Decision: Simplified change detection**
  - Options considered: (A) Per-service change filters (6 outputs), (B) Aggregate `backend` filter for all services
  - Why this choice: Option B -- the change detection is now only used for the E2E gate (whether to run expensive Docker-based E2E tests). The matrix always runs all services. A single `backend` filter (`services/**`) is sufficient for this purpose.
  - Consequences / risks: E2E now triggers on any backend file change, not per-service. This is appropriate since E2E tests exercise the full stack.

- **Decision: Conditional env var for ai-api secret**
  - The `AIAPI_OpenAi__ApiKey` secret is only needed by ai-api tests. In the matrix, it's conditionally set using `${{ matrix.service.name == 'ai-api' && secrets.AIAPI_OPENAI_API_KEY || '' }}` to avoid exposing it to other services.

## Implementation Notes
- Key changes:
  - `ci.yml`: Replaced `ai-api` and `orgs-api` jobs with a single `dotnet-services` matrix job. Added all 6 services to the matrix. Updated E2E job to depend on `dotnet-services` instead of individual jobs. Added ports 5030/5040/5050/5060 to E2E port cleanup. Added health checks for all 6 services in E2E wait loop. Updated third-party-notices hash to scan all 6 service `src/` directories.
  - `publish.yml`: Added properties-api, publishing-api, contacts-api, appointments-api to the Docker image build matrix (with their Dockerfile paths).
  - `deploy.yml`: Extended deploy-apis matrix from `[ai-api, orgs-api]` to include all 6 services. Updated job comment.
  - `rollback.yml`: Added 4 new services to the `services` input choice options. Updated the "all" case in resolve-services to include all 7 services (6 APIs + web).
  - `docker-compose.ci.yml`: Added CI container name overrides for properties-api, publishing-api, contacts-api, appointments-api.
- Edge cases handled: Conditional secret injection for ai-api in the matrix
- Known limitations: No per-service change-based skip in the matrix; all 6 services build on every PR

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
# Build verification (all 6 services)
dotnet build services/ai-api/Propely.AiApi.sln          # 0 errors
dotnet build services/orgs-api/Propely.OrgsApi.sln      # 0 errors
dotnet build services/properties-api/Propely.PropertiesApi.sln    # 0 errors
dotnet build services/publishing-api/Propely.PublishingApi.sln    # 0 errors
dotnet build services/contacts-api/Propely.ContactsApi.sln        # 0 errors
dotnet build services/appointments-api/Propely.AppointmentsApi.sln # 0 errors

# Test verification (all 6 services)
dotnet test services/ai-api/Propely.AiApi.sln --no-build          # 169 passed
dotnet test services/orgs-api/Propely.OrgsApi.sln --no-build      # 616 passed
dotnet test services/properties-api/Propely.PropertiesApi.sln --no-build  # 6 passed
dotnet test services/publishing-api/Propely.PublishingApi.sln --no-build  # 6 passed
dotnet test services/contacts-api/Propely.ContactsApi.sln --no-build     # 6 passed
dotnet test services/appointments-api/Propely.AppointmentsApi.sln --no-build # 6 passed

# SaasTemplate reference check
grep -ri "SaasTemplate\|saastemplate" .github/  # 0 matches
```

## Files Changed
- `.github/workflows/ci.yml` -- Refactored to matrix strategy for all 6 .NET services, updated E2E health checks and port cleanup, updated third-party-notices hash scan
- `.github/workflows/publish.yml` -- Added 4 new services to Docker image build matrix
- `.github/workflows/deploy.yml` -- Extended deploy-apis matrix to all 6 services
- `.github/workflows/rollback.yml` -- Added 4 new services to rollback input options and "all" matrix
- `docker-compose.ci.yml` -- Added CI container name overrides for 4 new services
- `docs/tasks/0006-ci-pipeline-updates.md` -- Task documentation (new)
- `docs/walkthroughs/0006-ci-pipeline-updates.md` -- This walkthrough (new)
- `docs/backlog/epic-P0-foundation.md` -- Task status updated

## Tests
### Unit
- N/A (workflow files are not unit-testable)

### Integration
- N/A

### Manual
- Verified all 6 services build with 0 errors
- Verified all 6 services pass all tests (0 failures)
- Verified zero SaasTemplate references in `.github/` directory
- Workflow YAML syntax verified by visual inspection

## Observability
- Logs added/updated: N/A
- Traces/metrics added/updated: N/A

## Security
- Validation: `AIAPI_OpenAi__ApiKey` secret is conditionally set only for ai-api matrix entry
- AuthN/AuthZ impact: None
- Sensitive data handling: No new secrets introduced; existing secret pattern preserved

## Follow-ups / Backlog
- [ ] Consider per-service change detection in matrix if build times grow significantly
- [ ] Add service-specific API URL build args to deploy-web step when new service URLs are available

## Checklist
- [x] Task scope matches `docs/tasks/0006-ci-pipeline-updates.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
