# Walkthrough: ft-0004-audit-action-command

## Summary
Added an audit action workflow command, template, and governance references to track remediation work derived from audit action plans. Updated command documentation across agent instruction files and Copilot instructions.

## Context
The executive summary provides a prioritized action plan but lacks a workflow to convert those items into tracked remediation tasks. A new audit action workflow was needed to standardize how the plan is executed.

## Decisions & Trade-offs
- Created a dedicated audit action workflow and template to avoid overloading standard task conventions while preserving matching walkthrough requirements.
- Added audit action naming guidance to documentation standards and governance to make the new document type explicit.

## Implementation Notes
- Introduced `.agent/rules/audit-action-workflow.md` to define the `next audit action` command.
- Added `.agent/templates/audit-action-template.md` for consistent audit action documents.
- Updated command references in CLAUDE/GEMINI/AGENTS and Copilot instructions.
- Documented audit action files and walkthroughs in `.agent.md` and documentation standards.

## Commands Run
- `rg -n "next task" CLAUDE.md GEMINI.md AGENTS.md copilot-instructions.md .agent -g "*.md"`
- `find . -name "copilot-instructions.md" -print`
- `sed -n '1,200p' CLAUDE.md`
- `sed -n '1,200p' GEMINI.md`
- `sed -n '1,200p' AGENTS.md`
- `sed -n '1,200p' .github/copilot-instructions.md`
- `sed -n '1,220p' .agent/rules/autonomous-workflow.md`
- `ls .agent/rules`
- `sed -n '1,200p' .agent/rules/fast-track-workflow.md`
- `sed -n '1,200p' .agent/rules/docs-standards.md`

## Files Changed
- `.agent/rules/audit-action-workflow.md` (new)
- `.agent/templates/audit-action-template.md` (new)
- `.agent/rules/docs-standards.md`
- `.agent.md`
- `CLAUDE.md`
- `GEMINI.md`
- `AGENTS.md`
- `.github/copilot-instructions.md`
- `docs/tasks/ft-0004-audit-action-command.md` (new)
- `docs/walkthroughs/ft-0004-audit-action-command.md` (new)

## Tests
- Not run (documentation changes only).

## Follow-ups
- Create the first audit action document from the executive summary (P0 authentication/authorization) when requested.
