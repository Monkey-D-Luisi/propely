# Audit Action: audit-0039-scope-ar-writer-role

## Metadata
- ID: audit-0039
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-12
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-009-executive-summary.md`
  - Action plan item: "#4 — Scope AR writer role to specific repositories"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0039-scope-ar-writer-role.md`

## Goal
Narrow the deploy SA's Artifact Registry write access from project-level to repository-scoped bindings.

## Context
The deploy SA had `roles/artifactregistry.writer` at the project level, allowing it to push images to any AR repo in the project. By moving the binding to environment configs and scoping it to each environment's specific AR repo, we follow least-privilege principles.

## Scope
### In scope
- Remove `roles/artifactregistry.writer` from bootstrap project-level roles
- Add `google_artifact_registry_repository_iam_member` to each environment config
- Add `deploy_sa_email` variable to environment configs

### Out of scope
- Other project-level role changes

## Acceptance Criteria
- [x] AC1: `roles/artifactregistry.writer` removed from bootstrap `deploy_roles`
- [x] AC2: Repository-scoped IAM binding exists in staging and production configs
- [x] AC3: `deploy_sa_email` variable added with safe default (empty string)
- [x] AC4: `terraform validate` passes for all configs

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
