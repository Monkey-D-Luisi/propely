# Task: 0001-documentation-roadmap-overhaul

## Metadata
- ID: 0001
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-19
- Related docs:
  - Walkthrough: `docs/walkthroughs/0001-documentation-roadmap-overhaul.md`
  - Epic: `docs/backlog/epic-P0-foundation.md` (Task 0.1)

## Goal
Rewrite all documentation, agent instructions, roadmap, and backlog to reflect the Propely product. Remove all SaaS Starter Kit template-specific artifacts and replace them with Propely-specific content.

## Context
The repository was originally forked from a SaaS Starter Kit template. While much of the documentation has already been rewritten for Propely (`.agent.md`, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`, `README.md`, `docs/roadmap.md`, backlog epics, getting-started guide, Claude Code Skills), several template-era artifacts remain: old docs under `services/ai-api/docs/`, and files like `apps/web/README.md`, `SECURITY.md`, `EULA.md`, `CHANGELOG.md`, and `infra/terraform/README.md` still reference "SaaS Starter Kit" or "SaasTemplate". Additionally, `docs/architecture/` is empty and needs topology documentation.

## Scope
### In scope
- Update remaining files with SaasTemplate/SaaS Starter Kit references to Propely
- Remove template-era documentation under `services/ai-api/docs/` (tasks, walkthroughs, audits, old backlog)
- Create architecture documents in `docs/architecture/` describing the 6-service topology

### Out of scope
- Actual code changes (renaming namespaces is Task 0.2)
- New service scaffolding (Task 0.3)
- Infrastructure changes (Task 0.5)

## Requirements
- R1: Zero references to "SaaS Starter Kit" or "SaasTemplate" in documentation files (excluding intentional references in roadmap/backlog describing the rename task 0.2)
- R2: Architecture documents describe all 6 backend services, inter-service communication, and data flow
- R3: All documentation is in English

## Acceptance Criteria
- AC1: `.agent.md` references Propely, not SaaS Starter Kit or SaasTemplate
- AC2: `CLAUDE.md` accurately describes Propely's architecture (6 services, port allocations, database names)
- AC3: `AGENTS.md`, `GEMINI.md`, and `.github/copilot-instructions.md` are updated for Propely
- AC4: `docs/roadmap.md` exists with phases P0 through P7 defined
- AC5: Backlog epic files exist for each phase under `docs/backlog/`
- AC6: All template-specific documentation artifacts are removed
- AC7: `docs/architecture/` documents describe the 6-service Propely topology
- AC8: Getting-started guide references Propely and correct commands
- AC9: Claude Code Skills exist for: next-task, code-review, fast-track, pr
- AC10: `README.md` is rewritten for Propely

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Update remaining doc files that reference SaasTemplate/SaaS Starter Kit
2. Remove old template-era docs under `services/ai-api/docs/`
3. Create architecture overview documents in `docs/architecture/`

## Implementation Steps
1. Update `apps/web/README.md` to reference Propely
2. Update `SECURITY.md` to reference Propely
3. Update `EULA.md` to reference Propely
4. Update `CHANGELOG.md` to reference Propely
5. Update `infra/terraform/README.md` to reference Propely
6. Remove template-era docs: `services/ai-api/docs/tasks/`, `services/ai-api/docs/walkthroughs/`, `services/ai-api/docs/audits/`, `services/ai-api/docs/backlog/` old files
7. Create `docs/architecture/service-topology.md` with 6-service overview
8. Create `docs/architecture/inter-service-communication.md` with SDK client patterns
9. Verify zero SaasTemplate/SaaS Starter Kit references remain in docs

## Files to Create / Modify
- Modify: `apps/web/README.md`, `SECURITY.md`, `EULA.md`, `CHANGELOG.md`, `infra/terraform/README.md`
- Delete: Old template-era docs under `services/ai-api/docs/`
- Create: `docs/architecture/service-topology.md`, `docs/architecture/inter-service-communication.md`

## Testing Plan
- Unit tests: N/A (documentation only)
- Integration tests: N/A
- Manual verification: grep for "SaasTemplate" and "SaaS Starter Kit" returns zero results in doc files

## Security & Privacy
- Ensure no secrets are committed in any documentation files
- Verify `.env.example` references use placeholder values only

## Observability
- N/A (documentation only)

## Rollback Plan
Revert the commit. All changes are documentation-only with no code impact.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
