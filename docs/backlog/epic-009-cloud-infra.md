# Epic 009: Cloud Infrastructure

## Overview

Set up cloud deployment infrastructure on GCP using Terraform for infrastructure-as-code, Cloud Run for container hosting, and a cost estimation matrix. This enables production deployment of the SaaS template.

## Success Criteria

- Terraform modules for GCP resources (Cloud Run, Cloud SQL, VPC, IAM)
- Automated deployment pipeline from GHCR images to Cloud Run
- Cost matrix document for different scale tiers (starter, growth, scale)
- Infrastructure can be provisioned with a single `terraform apply`
- Environment-specific configs (staging, production)

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Cloud provider | GCP | Cloud Run is container-native, good free tier, simple scaling |
| IaC | Terraform | Multi-cloud capable, industry standard, declarative |
| Compute | Cloud Run | Serverless containers, auto-scaling, pay-per-use |
| Database | Cloud SQL (PostgreSQL) | Managed PostgreSQL, compatible with existing EF Core setup |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0053 - Terraform GCP foundation
- **Status:** DONE
- **GitHub Issue:** #205
- **Dependencies:** None
- **File:** `docs/tasks/0053-terraform-gcp.md`
- **Scope:** Create Terraform modules: VPC/network, Cloud SQL (PostgreSQL), Cloud Run services (x3), IAM/service accounts, Secret Manager for env vars. Add remote state backend (GCS). Create environment configs (staging, prod).
- **Old Issue:** #55

### Task 0054 - Cloud Run deployment pipeline
- **Status:** DONE
- **GitHub Issue:** #206
- **Dependencies:** 0050 (GHCR images)
- **File:** `docs/tasks/0054-cloud-run-deploy.md`
- **Scope:** Add GitHub Actions workflow for deployment. Pull images from GHCR, deploy to Cloud Run. Environment-based deploys (staging on PR merge, prod on release). Add rollback capability. Configure health check routes.
- **Old Issue:** #56

### Task 0055 - Cost matrix and resource planning
- **Status:** DONE
- **GitHub Issue:** #207
- **Dependencies:** 0053
- **File:** `docs/tasks/0055-cost-matrix.md`
- **Scope:** Document GCP cost estimation for 3 tiers: starter (0-100 users), growth (100-1K users), scale (1K-10K users). Include compute, database, networking, storage costs. Add optimization recommendations.
- **Old Issue:** #57

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Cloud Infra | 0053-0055 | 3 | 0 |
| **Total** | **3** | **3** | **0** |

## Dependency Graph

```
                                    0050 (GHCR, Epic 008)
                                         │
0053 (Terraform) ──► 0055 (Cost matrix)  ▼
                     0054 (Cloud Run deploy)
```
