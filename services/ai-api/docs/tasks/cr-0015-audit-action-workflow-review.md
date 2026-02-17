# CR-0015: Audit Action Workflow Review

## Metadata
- **PR:** [#28](https://github.com/Monkey-D-Luisi/ai-api-template/pull/28)
- **Title:** docs(agent): add `next audit action` workflow, template, and governance updates
- **Branch:** codex/aggregate-audit-findings-and-summarize-health
- **CI Status:** claude-review (FAILURE), claude (SKIPPED)

## Changed Files
| File | Additions | Deletions |
|------|-----------|-----------|
| `.agent.md` | 3 | 0 |
| `.agent/rules/audit-action-workflow.md` | 86 | 0 |
| `.agent/rules/docs-standards.md` | 5 | 0 |
| `.agent/templates/audit-action-template.md` | 54 | 0 |
| `.github/copilot-instructions.md` | 14 | 3 |
| `AGENTS.md` | 1 | 0 |
| `CLAUDE.md` | 1 | 0 |
| `GEMINI.md` | 1 | 0 |
| `docs/audits/2026-01-30-executive-summary.md` | 70 | 0 |
| `docs/tasks/ft-0004-audit-action-command.md` | 48 | 0 |
| `docs/walkthroughs/ft-0004-audit-action-command.md` | 47 | 0 |

## Review Sources Checked
- **Review Comments (inline):** 3 comments (2 Gemini, 1 Copilot)
- **Reviews (general):** 1 review with 1 suppressed suggestion
- **Issue Comments:** 1 comment (Gemini summary, no action required)

---

## Comment Resolution Plan

### MUST_FIX
- [x] [Gemini Comment #2746355045](https://github.com/Monkey-D-Luisi/ai-api-template/pull/28#discussion_r2746355045): Incomplete directory scanning instruction
  - **File:** `.github/copilot-instructions.md` (line 105)
  - **Issue:** With audit actions added, the instruction to scan `docs/tasks/` is incomplete.
  - **Proposed change:** Clarify which directories to scan for which task types.

- [x] [Copilot Comment #2746372772](https://github.com/Monkey-D-Luisi/ai-api-template/pull/28#discussion_r2746372772): Ambiguous directory scanning instruction
  - **File:** `.github/copilot-instructions.md` (line 107)
  - **Issue:** The instruction "Always scan `docs/tasks/`..." needed to clarify audit action handling.
  - **Proposed change:** Reword to specify which task types scan which directories.

### SHOULD_FIX
- [x] [Gemini Comment #2746355026](https://github.com/Monkey-D-Luisi/ai-api-template/pull/28#discussion_r2746355026): Hardcoded executive summary link
  - **File:** `.agent/rules/audit-action-workflow.md` (line 85)
  - **Issue:** Hardcoded link to specific executive summary contradicts Step 1 instruction to locate the *latest* summary.
  - **Proposed change:** Link to the directory instead of a specific file.

### SUGGESTION
- [x] Suppressed Comment: Extra trailing blank line in executive summary
  - **File:** `docs/audits/2026-01-30-executive-summary.md`
  - **Issue:** File ends with two blank lines; should be one for consistency.
  - **Action:** Remove extra trailing blank line.

---

## Process Improvement

During this code review, comments from Gemini were initially missed because only partial output was reviewed. To prevent this in future reviews, the code review workflow (`.agent/rules/code-review-workflow.md`) has been updated with:

1. **Step 1.1**: Explicit requirement to fetch from ALL three comment sources
2. **Step 1.2**: Mandatory verification step to count comments from each source
3. **Warning**: Explicit note about the common mistake of missing multi-reviewer comments

---

## Status
- [x] Comments classified
- [x] Resolution plan created
- [x] MUST_FIX items implemented
- [x] SUGGESTION items implemented
- [x] Changes committed (0fadc17)
- [ ] PR comments replied
