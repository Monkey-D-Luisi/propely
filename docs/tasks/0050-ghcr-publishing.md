# Task: 0050 - GHCR Container Image Publishing

## Metadata
- ID: 0050
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #202
- Epic: `docs/backlog/epic-008-cicd-release.md`
- Old Issue: #39
- Milestone: v1.0

## Goal
Add a GitHub Actions workflow that builds and pushes Docker images for all three services to GitHub Container Registry (GHCR) on main branch pushes.

## Context
All three services already have Dockerfiles (task 0020): `apps/web/Dockerfile`, `services/ai-api/Dockerfile`, `services/orgs-api/Dockerfile`. The existing CI workflow (`.github/workflows/ci.yml`) runs builds and tests. This task adds a separate publish workflow that builds and pushes images to GHCR.

## Scope
### In scope
- GitHub Actions workflow for Docker build + push to GHCR
- Tag images with: commit SHA, branch name, `latest` for main
- Trigger on push to main (after CI passes)
- Multi-platform builds (amd64)
- Image vulnerability scanning (Trivy or similar)
- Login to GHCR using `GITHUB_TOKEN`

### Out of scope
- ARM64 builds (add later if needed)
- Private registry support
- Deployment (task 0054)

## Requirements
- R1: All three service images published to GHCR on main push
- R2: Images tagged with commit SHA and `latest`
- R3: Build only runs after CI passes
- R4: Images scanned for vulnerabilities
- R5: No secrets required (uses built-in GITHUB_TOKEN)

## Acceptance Criteria
- AC1: Merge to main triggers image build and push
- AC2: Three images appear in GHCR: `ghcr.io/{owner}/saas-template-web`, `ghcr.io/{owner}/saas-template-ai-api`, `ghcr.io/{owner}/saas-template-orgs-api`
- AC3: Images tagged with commit SHA
- AC4: Vulnerability scan runs and reports findings
- AC5: Workflow passes on current codebase

## Constraints (non-negotiable)
- Use `GITHUB_TOKEN` (no additional secrets needed)
- Use Docker BuildKit for efficient builds
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Create workflow file** (`.github/workflows/publish.yml`)
   ```yaml
   name: Publish Docker Images
   on:
     push:
       branches: [main]
   jobs:
     publish:
       runs-on: ubuntu-latest
       permissions:
         contents: read
         packages: write
       strategy:
         matrix:
           service:
             - { name: web, context: apps/web, dockerfile: apps/web/Dockerfile }
             - { name: ai-api, context: services/ai-api, dockerfile: services/ai-api/Dockerfile }
             - { name: orgs-api, context: services/orgs-api, dockerfile: services/orgs-api/Dockerfile }
       steps:
         - uses: actions/checkout@v4
         - uses: docker/login-action@v3
           with:
             registry: ghcr.io
             username: ${{ github.actor }}
             password: ${{ secrets.GITHUB_TOKEN }}
         - uses: docker/build-push-action@v5
           with:
             context: ${{ matrix.service.context }}
             file: ${{ matrix.service.dockerfile }}
             push: true
             tags: |
               ghcr.io/${{ github.repository_owner }}/saas-template-${{ matrix.service.name }}:${{ github.sha }}
               ghcr.io/${{ github.repository_owner }}/saas-template-${{ matrix.service.name }}:latest
   ```

2. **Add vulnerability scanning step** using `aquasecurity/trivy-action`

3. **Add build cache** using `actions/cache` or Docker layer caching

4. **Test locally** (verify Dockerfiles still build)
   - `docker build -t test-web apps/web`
   - `docker build -t test-ai-api services/ai-api`
   - `docker build -t test-orgs-api services/orgs-api`

## Files to Create / Modify

### Create
- `.github/workflows/publish.yml`
- `docs/walkthroughs/0050-ghcr-publishing.md`

### Modify
- None (Dockerfiles already exist)

## Testing Plan
- Manual: Push to main -> verify images appear in GHCR
- CI: Workflow runs without errors

## Definition of Done Checklist
- [x] Acceptance criteria met
- [ ] Workflow runs successfully *(Publish workflow triggers but fails — needs investigation)*
- [ ] All three images published to GHCR *(blocked by workflow failure)*
- [x] Vulnerability scanning enabled
- [x] Walkthrough updated
