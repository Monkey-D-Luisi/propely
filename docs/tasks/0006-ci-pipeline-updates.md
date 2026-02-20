# Task: 0006-ci-pipeline-updates

## Metadata
- ID: 0006
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/0006-ci-pipeline-updates.md`

## Goal
Update the CI/CD pipeline to build, test, and validate all 6 backend services plus the frontend. Use a matrix strategy in CI to avoid duplicating workflow steps. Update publish, deploy, and rollback workflows to include the 4 new services.

## Context
The Propely monorepo now has 6 backend services (ai-api, orgs-api, properties-api, publishing-api, contacts-api, appointments-api) plus a Next.js frontend. The CI workflow currently only handles ai-api, orgs-api, and web. The publish, deploy, and rollback workflows similarly only reference these 3 services. The 4 new services scaffolded in Task 0.3 need to be integrated into all CI/CD workflows.

## Scope
### In scope
- Refactor `ci.yml` to use a reusable matrix strategy for building/testing all 6 .NET services
- Add path-based change detection for the 4 new services
- Update E2E job to reference all 6 backend services in health checks
- Update `docker-compose.ci.yml` to include CI container overrides for 4 new services
- Update `publish.yml` to build Docker images for all 6 services
- Update `deploy.yml` to deploy all 6 API services
- Update `rollback.yml` to include all 6 services in rollback options
- Update `third-party-notices` hash job to scan all 6 service csproj files
- Verify zero `SaasTemplate` references remain in any workflow file

### Out of scope
- Production deployment configuration (Terraform, Cloud Run)
- Setting up staging environments
- Performance testing in CI
- Modifying service code or Dockerfiles

## Requirements
- R1: All 6 .NET services must be built and tested in CI
- R2: Path-based change detection must work for all 6 services
- R3: Docker images must be published for all 6 services
- R4: Deploy and rollback must support all 6 services
- R5: Zero `SaasTemplate` references in workflow files

## Acceptance Criteria
- AC1: `ci.yml` uses a matrix strategy with entries for all 6 services
- AC2: CI matrix correctly resolves solution file paths
- AC3: CI runs `dotnet build` for each service in the matrix
- AC4: CI runs `dotnet test` for each service in the matrix
- AC5: CI builds the Next.js frontend
- AC6: CI runs frontend linting
- AC7: `deploy.yml` references all 6 API services for deployment
- AC8: `publish.yml` builds Docker images for all 6 services
- AC9: `rollback.yml` includes all 6 services in rollback options
- AC10: Zero references to `SaasTemplate` remain in any workflow file

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Refactor `ci.yml` from separate per-service jobs to a single matrix job covering all 6 services
2. Add change detection filters for the 4 new services
3. Update downstream workflows (publish, deploy, rollback) to include the new services
4. Update `docker-compose.ci.yml` with CI container overrides for new services
5. Update E2E job health checks and port freeing for new service ports

## Implementation Steps
1. Update `ci.yml`: refactor to matrix strategy for all 6 services
2. Update `ci.yml`: add change detection for 4 new services
3. Update `ci.yml`: update E2E job for all services
4. Update `ci.yml`: update third-party-notices hash computation
5. Update `docker-compose.ci.yml`: add 4 new service CI overrides
6. Update `publish.yml`: add 4 new services to build matrix
7. Update `deploy.yml`: add 4 new services to deploy matrix
8. Update `rollback.yml`: add 4 new services to rollback options
9. Verify zero `SaasTemplate` references across all workflow files

## Files to Create / Modify
- `.github/workflows/ci.yml`
- `.github/workflows/publish.yml`
- `.github/workflows/deploy.yml`
- `.github/workflows/rollback.yml`
- `docker-compose.ci.yml`

## Testing Plan
- Unit tests: N/A (workflow files)
- Integration tests: N/A
- Manual verification: Push changes to feature branch and verify CI triggers, actionlint passes, workflow YAML is valid

## Security & Privacy
- Ensure CI workflows do not expose secrets in logs
- Verify Docker image publishing only occurs on protected branches (main)
- Ensure GITHUB_TOKEN permissions are scoped appropriately
- No new services accidentally bypass security scanning steps (Trivy)

## Observability
- Logs: GitHub Actions workflow logs
- Metrics: CI run duration per service
- Traces: N/A

## Rollback Plan
Revert the workflow file changes to restore the previous 2-service CI configuration.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
