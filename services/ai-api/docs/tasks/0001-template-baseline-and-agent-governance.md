# Task: 0001-template-baseline-and-agent-governance

## Metadata
- ID: 0001
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2025-01-27
- Related docs:
  - Walkthrough: `docs/walkthroughs/0001-template-baseline-and-agent-governance.md`

## Goal
Create the baseline repository structure and agent governance system for task-driven development.

## Context
Building a reusable internal template with .NET 10 API, RabbitMQ, Postgres, Redis, OpenTelemetry, and AI client integration. This task establishes the foundational structure before any product features.

## Scope
### In scope
- Repository scaffolding
- Agent instructions and templates
- Documentation structure (tasks/walkthroughs/architecture)
- Vertical slice definition
- Local dev scripts (stubs)

### Out of scope
- Product features
- Actual docker-compose
- Source code implementation
- CI/CD pipelines

## Requirements
- R1: Create exact repository tree as specified
- R2: All files contain meaningful English content
- R3: Scripts are executable with safe behavior
- R4: Vertical slice is precisely defined

## Acceptance Criteria
- AC1: Repository tree matches specification exactly
- AC2: All files exist with meaningful English content
- AC3: `.agent.md` enforces strict task-driven workflow
- AC4: `docs/architecture/vertical-slice.md` defines slice unambiguously
- AC5: Scripts exist, are executable, have safe behavior
- AC6: Walkthrough file exists

## Constraints (non-negotiable)
- English-only repository content
- No product features
- Clean Architecture + SOLID + DDD conventions in documentation
- Matching walkthrough file required

## Proposed Approach (high-level)
1. Create directory structure
2. Create all standard repo files (.editorconfig, .gitignore, etc.)
3. Create agent governance files
4. Create documentation structure
5. Define vertical slice
6. Create dev scripts as stubs
7. Document in walkthrough

## Implementation Steps
1. Create directory tree
2. Create root files (README, .agent.md, .editorconfig, .gitattributes, .gitignore)
3. Create .agent/templates/* files
4. Create .agent/rules/* files
5. Create docs/architecture/* files
6. Create docs/standards/* files
7. Create docs/runbooks/* files
8. Create scripts/* files
9. Create walkthrough

## Files to Create / Modify
See deliverables section - all files listed in repository tree.

## Testing Plan
- Manual verification: all files exist
- Manual verification: scripts are executable
- Manual verification: content is meaningful

## Security & Privacy
- No secrets in repository
- .env.example pattern documented
- Security baseline documented

## Observability
- Not applicable for this task (documentation only)

## Rollback Plan
Delete the created files and directories.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] All files created with meaningful content
- [x] Scripts are executable
- [x] Walkthrough updated
