# Walkthrough: 0074-recovery-runbook

## Task Reference
- Task: `docs/tasks/0074-recovery-runbook.md`
- Walkthrough: `docs/walkthroughs/0074-recovery-runbook.md`
- Branch/PR: `feat/0074-recovery-runbook`
- Date: `2026-02-14`

## Summary
Created `docs/recovery-runbook.md` covering step-by-step recovery procedures for all infrastructure components: Cloud SQL PITR, Cloud Run revision rollback, RabbitMQ DLQ replay, Redis cache invalidation, and secret compromise response. Includes incident severity classification, RPO/RTO targets, and a post-incident checklist.

## Context
- Background: Enterprise buyers evaluating the SaaS Starter Kit need operational recovery documentation to assess production readiness.
- Problem statement: Infrastructure includes Cloud SQL with automated backups, Cloud Run with revision history, RabbitMQ with DLQ, and Redis as cache — but no documentation on how to actually perform recovery operations.
- Constraints: Documentation only (no code changes). Must reference `docs/production-hardening.md` for secret rotation. Must include both CLI and Console UI steps.

## Decisions & Trade-offs
- **Decision:** Used actual Terraform-configured values (instance names, backup retention, availability types) rather than generic placeholders.
  - Options considered: Generic GCP documentation vs. project-specific procedures
  - Why this choice: Project-specific procedures are immediately actionable. A buyer can follow them without translating generic docs to their setup.
  - Consequences: Procedures need updating if Terraform module defaults change.

- **Decision:** Secret compromise response references `production-hardening.md` rather than duplicating rotation procedures.
  - Why: Single source of truth for rotation steps; runbook adds the incident-response wrapper (contain, assess, remediate, review).

- **Decision:** Included RabbitMQ Shovel plugin as the recommended DLQ replay method.
  - Options considered: Shovel plugin vs. manual republish only
  - Why: Shovel handles bulk replay atomically and auto-deletes after completion. Manual republish is included as a fallback for per-message selective replay.

## Implementation Notes
- Key changes: Single new file `docs/recovery-runbook.md`
- RPO/RTO targets derived from actual Terraform configuration (PITR enabled only for REGIONAL instances, backup retention 14 days production / 7 days staging)
- Cloud SQL section covers both PITR (clone-based, production) and daily backup restore (in-place, staging)
- Redis section notes that Memorystore doesn't expose FLUSHALL via gcloud — requires redis-cli from a VPC-connected bastion
- All gcloud commands use shell variables for project ID, region, and instance names

## Data / Schema / Migrations
- N/A (documentation only)

## Commands Run
```bash
# Build verification (docs-only task, no code changes)
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build
```

## Files Changed
- `docs/recovery-runbook.md` — New file: complete recovery runbook
- `docs/tasks/0074-recovery-runbook.md` — Status updated to DONE, DoD checkboxes checked
- `docs/walkthroughs/0074-recovery-runbook.md` — This file
- `docs/backlog/epic-013-presale-hardening.md` — Task 0074 status PENDING → DONE

## Tests
### Unit
- N/A (documentation only)

### Integration
- N/A (documentation only)

### Manual
- Verified all gcloud commands use correct instance naming patterns from Terraform modules
- Verified RabbitMQ DLQ topology matches `WorkItemProjectorService` and `ProjectorConfiguration`
- Verified Cloud SQL backup settings match `cloud-sql` Terraform module
- Verified Redis settings match `redis` Terraform module
- Cross-referenced secret names with `production-hardening.md`

## Observability
- N/A (documentation only)

## Security
- No secrets committed — all example commands use shell variables
- Secret compromise response procedure included

## Follow-ups / Backlog
- [ ] Add automated DLQ depth alerting (Cloud Monitoring + PagerDuty/Opsgenie)
- [ ] Consider adding a `scripts/dlq-replay.sh` helper script
- [ ] Add chaos engineering / DR testing procedures in a future task

## Checklist
- [x] Task scope matches `docs/tasks/0074-recovery-runbook.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
