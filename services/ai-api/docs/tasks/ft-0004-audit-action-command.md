# Task: ft-0004-audit-action-command

## Metadata
- ID: ft-0004
- Type: FastTrack
- Status: DONE
- Owner: Agent
- Created: 2026-01-30
- Related docs:
  - Walkthrough: `docs/walkthroughs/ft-0004-audit-action-command.md`

## Goal
Define an audit action workflow command and supporting documentation to turn audit action plan items into executable work items.

## Context
The audit executive summary includes a prioritized action plan, but there is no command or workflow to convert those actions into tracked remediation work. A new workflow and templates are needed for consistent execution.

## Scope
### In scope
- Add an audit action workflow definition under `.agent/rules/`.
- Add an audit action template.
- Update command references in CLAUDE/GEMINI/AGENTS and Copilot instructions.
- Document audit action naming in governance and documentation standards.

### Out of scope
- Implementing the actual remediation tasks.
- Modifying existing audit findings.

## Acceptance Criteria
- [x] A new audit action workflow exists under `.agent/rules/`.
- [x] An audit action template exists under `.agent/templates/`.
- [x] Command lists include `next audit action`.
- [x] Documentation standards include audit action naming.

## Files to Create / Modify
- `.agent/rules/audit-action-workflow.md`
- `.agent/templates/audit-action-template.md`
- `.agent.md`
- `.agent/rules/docs-standards.md`
- `CLAUDE.md`
- `GEMINI.md`
- `AGENTS.md`
- `.github/copilot-instructions.md`

## Definition of Done
- [x] Goal achieved
- [x] No secrets committed
- [x] Walkthrough updated
