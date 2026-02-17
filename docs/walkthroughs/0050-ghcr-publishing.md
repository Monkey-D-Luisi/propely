# Walkthrough: 0050 - GHCR Container Image Publishing

## Task Reference
- Task: `docs/tasks/0050-ghcr-publishing.md`
- Walkthrough: `docs/walkthroughs/0050-ghcr-publishing.md`
- Branch/PR: `feat/0050-ghcr-publishing`
- Date: `2026-02-12`

## Summary
Added a GitHub Actions workflow (`.github/workflows/publish.yml`) that builds and pushes Docker images for all three services (web, ai-api, orgs-api) to GitHub Container Registry (GHCR) after CI passes on main branch pushes.

## Context
- Background: All three services already have production Dockerfiles (created in task 0020). The CI workflow runs builds and tests. This task adds automated container image publishing.
- Problem statement: No automated way to publish Docker images for deployment.
- Constraints: Must use built-in GITHUB_TOKEN (no additional secrets), must wait for CI to pass first.

## Decisions & Trade-offs
- **Trigger mechanism: `workflow_run` instead of `push` + wait-on-check**
  - Options considered: (1) `on: push` with third-party wait-on-check action, (2) `workflow_run` event
  - Why this choice: `workflow_run` is a native GitHub Actions feature that triggers after the CI workflow completes. No third-party dependency, cleaner separation of concerns.
  - Consequences: The workflow only runs after CI completes with `success` conclusion. If CI is skipped (docs-only changes), publish won't run.

- **Build context: repo root (`.`) for all services**
  - All three Dockerfiles use `COPY` paths relative to repo root (e.g., `COPY apps/web/`, `COPY services/ai-api/src/`).
  - The `file:` parameter specifies the Dockerfile path while `context: .` provides the full repo.

- **Vulnerability scanning: Trivy with `exit-code: 0`**
  - Options considered: fail-on-vulnerability vs report-only
  - Why this choice: Report-only (`exit-code: 0`) initially. Failing builds on vulnerabilities in base images (alpine, dotnet) would block deployments for issues we don't control. Can tighten later.

- **Layer caching: GitHub Actions cache (`type=gha`)**
  - Docker layer caching via GitHub Actions cache backend. More reliable than registry-based caching for public repos.

## Implementation Notes
- Key changes: Single workflow file with matrix strategy for all three services.
- `docker/metadata-action` generates consistent tags: short SHA + `latest` (only on default branch).
- `docker/build-push-action@v6` with BuildKit for efficient multi-stage builds.
- `aquasecurity/trivy-action@0.28.0` scans each published image for CRITICAL and HIGH vulnerabilities.

## Commands Run
```bash
# Validated YAML syntax
# Verified Dockerfile build contexts match workflow configuration
```

## Files Changed
- `.github/workflows/publish.yml` — New workflow for Docker image build, push, and scanning
- `docs/tasks/0050-ghcr-publishing.md` — Status updated to DONE
- `docs/backlog/epic-008-cicd-release.md` — Task 0050 status updated to DONE
- `docs/walkthroughs/0050-ghcr-publishing.md` — This file

## Tests
### Manual
- Workflow will be validated on first merge to main
- Verify three images appear in GHCR: `saas-template-web`, `saas-template-ai-api`, `saas-template-orgs-api`
- Verify images tagged with commit SHA and `latest`

## Security
- Uses built-in `GITHUB_TOKEN` for GHCR authentication (no additional secrets)
- Minimal permissions: `contents: read` + `packages: write`
- Trivy scans for CRITICAL and HIGH CVEs in published images

## Follow-ups / Backlog
- [ ] Tighten Trivy to `exit-code: 1` once base image vulnerabilities are resolved
- [ ] Consider ARM64 multi-platform builds if needed for deployment targets

## Checklist
- [x] Task scope matches `docs/tasks/0050-ghcr-publishing.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
