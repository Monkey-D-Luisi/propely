# Task: 0074 - Recovery Runbook

## Metadata
- ID: 0074
- Type: Documentation
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #279
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Write a recovery runbook that gives buyers confidence their SaaS can recover from failures, with step-by-step procedures and defined RPO/RTO targets.

## Context
The infrastructure includes Cloud SQL with automated backups, Cloud Run with revision history, RabbitMQ with DLQ, and Redis as a cache. However, there is no documentation on how to actually perform recovery operations. An enterprise buyer evaluating the product needs this documentation to assess operational readiness.

### Components Needing Recovery Procedures
- Cloud SQL (PostgreSQL) — point-in-time recovery
- Cloud Run — revision rollback
- RabbitMQ — DLQ message replay
- Redis — cache invalidation/rebuild
- Secrets — compromise response

## Scope
### In scope
- Write `docs/recovery-runbook.md`
- Cloud SQL PITR procedure (gcloud CLI + Console UI steps)
- Cloud Run revision rollback procedure
- RabbitMQ DLQ replay procedure
- Redis cache invalidation and rebuild procedure
- Secret compromise response procedure (references production-hardening.md)
- RPO/RTO targets table for each component
- Incident severity classification guide

### Out of scope
- Implementing automated recovery (documentation only)
- Chaos engineering / disaster recovery testing
- Multi-region failover

## Requirements
- R1: Each component has both CLI and Console UI recovery steps
- R2: RPO/RTO targets are realistic and documented
- R3: Secret compromise section references production-hardening.md for rotation
- R4: Procedures are testable in a staging environment

## Acceptance Criteria
- AC1: `docs/recovery-runbook.md` exists
- AC2: Cloud SQL PITR procedure with gcloud commands
- AC3: Cloud Run rollback procedure with revision selection
- AC4: RabbitMQ DLQ replay procedure
- AC5: Redis cache rebuild procedure
- AC6: Secret compromise response procedure
- AC7: RPO/RTO targets table
- AC8: Incident severity classification

## Implementation Steps
1. Research Cloud SQL PITR capabilities and document procedure
2. Research Cloud Run revision management and document rollback procedure
3. Document RabbitMQ DLQ replay using existing DLQ policy configuration
4. Document Redis cache invalidation strategy
5. Write secret compromise response procedure (referencing 0073)
6. Define RPO/RTO targets for each component
7. Write incident severity classification guide
8. Review all procedures for accuracy

## Files to Create
- `docs/recovery-runbook.md`
- `docs/walkthroughs/0074-recovery-runbook.md`

## Definition of Done Checklist
- [x] `docs/recovery-runbook.md` created
- [x] Cloud SQL PITR documented
- [x] Cloud Run rollback documented
- [x] RabbitMQ DLQ replay documented
- [x] Redis cache rebuild documented
- [x] Secret compromise response documented
- [x] RPO/RTO targets defined
- [x] Incident severity guide included
- [x] Walkthrough updated
