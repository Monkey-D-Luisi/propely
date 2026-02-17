# Task: 0054 - Cloud Run Deployment Pipeline

## Metadata
- ID: 0054
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #206
- Epic: `docs/backlog/epic-009-cloud-infra.md`
- Old Issue: #56
- Dependencies: 0050 (GHCR images)
- Milestone: v1.0

## Goal
Create a GitHub Actions workflow that deploys Docker images from GHCR to Cloud Run, with environment-based deploys and rollback capability.

## Context
Task 0050 publishes images to GHCR. Task 0053 provisions Cloud Run services. This task connects them with a deployment pipeline: pull images from GHCR, deploy to Cloud Run, with environment-specific behavior.

## Scope
### In scope
- GitHub Actions deployment workflow
- Pull images from GHCR, deploy to Cloud Run
- Staging deploy on PR merge to main
- Production deploy on release tag
- Rollback capability (deploy previous image)
- Health check verification after deploy
- Deployment notifications (GitHub environment)

### Out of scope
- Blue-green deployments (Cloud Run handles traffic splitting natively)
- Custom domain setup
- SSL certificate management

## Requirements
- R1: Merge to main deploys to staging
- R2: Release tag deploys to production
- R3: Manual rollback is possible (re-deploy previous tag)
- R4: Health checks verified after deployment
- R5: Deployment logged in GitHub environments

## Acceptance Criteria
- AC1: Merge to main triggers staging deployment
- AC2: Release tag triggers production deployment
- AC3: Health check passes after deploy
- AC4: Rollback workflow deploys previous version
- AC5: GitHub environment shows deployment history

## Implementation Steps

1. **Create deploy workflow** (`.github/workflows/deploy.yml`)
2. **Configure GCP authentication** with Workload Identity Federation
3. **Deploy steps**: gcloud run deploy for each service
4. **Health check**: curl health endpoint after deploy
5. **Rollback workflow** (`.github/workflows/rollback.yml`) - manual trigger with version input

## Files to Create / Modify

### Create
- `.github/workflows/deploy.yml`
- `.github/workflows/rollback.yml`
- `docs/walkthroughs/0054-cloud-run-deploy.md`

## Testing Plan
- Manual: Test deploy to staging GCP project
- Verify health check passes

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Staging deploy works
- [x] Rollback works
- [x] Walkthrough updated
