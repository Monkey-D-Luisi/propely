# Task: 0055 - Cost Matrix and Resource Planning

## Metadata
- ID: 0055
- Type: Documentation
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #207
- Epic: `docs/backlog/epic-009-cloud-infra.md`
- Old Issue: #57
- Dependencies: 0053 (Terraform GCP)
- Milestone: v1.0

## Goal
Document GCP cost estimation for three scale tiers, helping users understand infrastructure costs before deploying.

## Context
Users of this SaaS template need to understand what it costs to run in production. A cost matrix with three tiers (starter, growth, scale) helps with budgeting and architecture decisions.

## Scope
### In scope
- Cost matrix document for 3 tiers
- Starter: 0-100 users
- Growth: 100-1,000 users
- Scale: 1,000-10,000 users
- Include: Cloud Run, Cloud SQL, networking, storage, secrets
- Cost optimization recommendations
- Comparison with alternative approaches

### Out of scope
- Exact pricing (GCP prices change)
- Cost optimization automation
- FinOps tooling

## Requirements
- R1: Three tier cost estimates documented
- R2: All GCP services included in estimates
- R3: Optimization recommendations provided

## Acceptance Criteria
- AC1: Document exists at `docs/infrastructure/cost-matrix.md`
- AC2: Three tiers with monthly cost ranges
- AC3: Optimization recommendations included

## Implementation Steps

1. **Research current GCP pricing** for: Cloud Run, Cloud SQL, VPC, Secret Manager, Load Balancing
2. **Calculate per-tier costs** based on expected usage patterns
3. **Document optimization tips**: committed use discounts, autoscaling config, Cloud SQL instance sizing
4. **Create the document** with tables and recommendations

## Files to Create

- `docs/infrastructure/cost-matrix.md`
- `docs/walkthroughs/0055-cost-matrix.md`

## Definition of Done Checklist
- [x] Document created with three tiers
- [x] Monthly cost ranges provided
- [x] Optimization recommendations included
- [x] Walkthrough updated
