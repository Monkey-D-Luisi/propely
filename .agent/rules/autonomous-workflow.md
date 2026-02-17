# Autonomous Agent Workflow

## Overview

This document defines how the AI agent operates autonomously when processing tasks. The agent follows a strict, predictable workflow that ensures consistent quality and traceability.

## Trigger Command

**Command:** `"next task"`

When the user issues this command, the agent autonomously:
1. Determines the next task to implement
2. Creates all necessary documentation
3. Implements the solution
4. Verifies quality gates
5. Commits the changes

## Task Document Rules

### Task Document Immutability
The task document (`docs/tasks/NNNN-*.md`) is the **immutable specification**. When completing a task, only update:
- (a) `Status` field in the Metadata section (e.g., `PENDING` -> `DONE`)
- (b) Definition of Done checkboxes (check the boxes)

**NEVER** delete or rewrite these sections: Requirements, Acceptance Criteria, Context, Constraints, Scope, Implementation Steps, Testing Plan, Security & Privacy, Observability, or Rollback Plan.

You **MAY** add new sections at the end of the document (e.g., "Implemented Decisions", "API Endpoints", "Quality Notes") but existing content from the template must be preserved verbatim. Implementation notes, decisions, and results belong in the **walkthrough** file, not the task file.

### Manual Verification Hand-off
If the task's Testing Plan includes manual verification steps (e.g., "Manual test: Full OAuth flow with real credentials"), you **MUST**:
1. Complete all automated work first (implementation, automated tests, build verification).
2. List the pending manual verification steps explicitly in a message to the user.
3. Ask the user to perform the manual steps before marking the task DONE.
4. Do **NOT** mark a task DONE or check "Acceptance criteria met" if manual verification is still pending.

### Scope Discipline
Only create, modify, or delete files that are **directly required** by the task's Scope and Implementation Steps sections. Do not add CI workflows, tooling configurations, linter configs, or unrelated improvements — even if they seem valuable.

If you identify a worthwhile addition outside the task scope:
1. Note it in the walkthrough's "Follow-ups / Backlog" section.
2. Do **not** create the file or make the change.

## Workflow Steps

### Step 0: Sync with Main

Before anything else, ensure a clean starting point:

1. Stash or commit any uncommitted changes
2. Switch to `main` branch: `git checkout main`
3. Pull latest: `git pull origin main`
4. Create a new feature branch for the task (after identifying it in Step 1)

### Step 1: Identify Next Task

1. **Read the roadmap** (`docs/roadmap-v1.md`) to understand the execution phases and their ordering:
   - Phase A (Foundation) → Phase 0 (Design System) → Phase B (Core Features) → Phase C (Polish) → Phase D (Demo)
   - Within each phase, tasks are ordered by track and sequence number (e.g., B1.1, B1.2, B1.3)
2. **Read `docs/backlog/`** epic files to get the current status of each task (PENDING / IN_PROGRESS / DONE)
3. **Scan tasks in roadmap phase order** — iterate through the roadmap's "All 19 Tasks Summary" table from top to bottom:
   - For each task with status `PENDING`, verify all dependencies have status `DONE` (dependencies may reference tasks in other epics — check across all epic files)
   - The **first PENDING task with all dependencies met**, in roadmap order, is the next task
4. If a PENDING task exists in the backlog but is **not listed in the roadmap**, append it to the end of the queue (after all roadmap tasks)
5. If no task has all dependencies met, report to user and wait

> **Why roadmap-first?** The roadmap encodes strategic phase ordering (foundation → design → features → polish → demo) that dependencies alone may not fully capture. Scanning in roadmap order ensures the agent follows the intended execution plan.

### Step 2: Create Task Documentation

1. Create task file: `docs/tasks/NNNN-<task-title>.md`
   - Use template from `.agent/templates/task-template.md`
   - Copy requirements from epic file
   - Add implementation details

2. Create walkthrough file: `docs/walkthroughs/NNNN-<task-title>.md`
   - Use template from `.agent/templates/walkthrough-template.md`
   - Start with task reference and initial context

### Step 3: Update Task Status

In the epic file, change the task status:
```markdown
**Status:** PENDING  →  **Status:** IN_PROGRESS
```

### Step 4: Implement the Solution

Follow Clean Architecture layer order when applicable:
1. **Domain Layer** — Entities, value objects, domain events
2. **Application Layer** — Commands, queries, handlers, interfaces
3. **Infrastructure Layer** — Repositories, external services, persistence
4. **Presentation Layer** — Controllers, DTOs, validation

For each layer:
- Write production code in `src/`
- Write tests in `tests/`
- Follow coding standards from `.agent/rules/coding-standards.md`

### Step 5: Run Quality Checks

Before considering the task complete:

```bash
# Build the affected service(s)
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln

# Run all tests
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

# Frontend (if affected)
cd apps/web && npm run build && npm test
```

All checks must pass.

### Step 6: Update Walkthrough

Document in the walkthrough file:
- **Summary**: What was implemented
- **Decisions**: Choices made and why
- **Commands Run**: Exact commands executed
- **Files Changed**: List with brief descriptions
- **Tests**: What was tested and results
- **Follow-ups**: Any items for future tasks

### Step 7: Commit Changes

Use conventional commits with task ID:

```bash
git add <specific-files>
git commit -m "feat(scope): description (#NNNN)"
```

#### Commit Message Format

```
<type>(<scope>): <description> (#NNNN)

[optional body]

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

**Types:**
- `feat` — New feature
- `fix` — Bug fix
- `test` — Adding or updating tests
- `docs` — Documentation only
- `refactor` — Code change that neither fixes nor adds
- `chore` — Maintenance tasks

**Scope:** The feature area (e.g., `auth`, `orgs`, `ai`, `infra`, `web`)

**Task ID:** Always include `(#NNNN)` at the end

### Step 8: Mark Task Complete

1. Update epic file:
```markdown
**Status:** IN_PROGRESS  →  **Status:** DONE
```

2. Update walkthrough checklist:
```markdown
- [x] Task scope matches docs/tasks/NNNN-*.md
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
```

3. Update task file `docs/tasks/NNNN-*.md`:
   - Change `Status: TODO` → `Status: DONE` in the Metadata section
   - Check all boxes in the Definition of Done Checklist
   - **Do NOT delete or rewrite any other sections** (see Task Document Rules above)
   - You may add new additive sections (e.g., "Implemented Decisions") at the end
```markdown
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
```

4. **Verify** all three files were updated before committing:
   - [ ] Epic file shows `DONE` for this task
   - [ ] Walkthrough checklist boxes are checked
   - [ ] Task file Status is `DONE` and all DoD boxes are checked

5. Commit the status updates:
```bash
git add <updated-docs>
git commit -m "docs(scope): mark task NNNN complete"
```

### Step 9: Create Pull Request

Push the branch and create a PR following the PR workflow (`.agent/rules/pr-workflow.md`):

1. Push branch to remote: `git push -u origin <branch-name>`
2. Create PR using `gh pr create` with the template at `.github/PULL_REQUEST_TEMPLATE.md`
3. Fill ALL sections of the template
4. Report the PR URL

> **Note:** This step runs after "Mark Task Complete" so the DONE status and
> final walkthrough are included in the PR.

## Quality Gates (Definition of Done)

A task is **not complete** until ALL of these are true:

| Gate | Verification |
|------|--------------|
| Acceptance criteria met | All AC items checked in task file |
| Code builds | `dotnet build` succeeds for affected service(s) |
| Tests pass | `dotnet test` succeeds for affected service(s) |
| No secrets | No credentials in code or config |
| Walkthrough accurate | Reflects actual implementation |
| Committed | Changes are in version control |

## Error Handling

### Build Failure
1. Identify and fix the error
2. Document the issue in walkthrough
3. Re-run build

### Test Failure
1. Determine if test is correct or code is wrong
2. Fix the appropriate part
3. Document any unexpected behavior
4. Re-run tests

### Blocked Task
If a task cannot proceed:
1. Document the blocker in walkthrough
2. Report to user with specific issue
3. Wait for user guidance
4. Do NOT skip to next task

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Standard task | `NNNN-<title>.md` | `0003-workitem-domain-model.md` |
| Fast track | `ft-NNNN-<title>.md` | `ft-0001-agent-autonomy-documentation.md` |
| Code review | `cr-NNNN-<title>.md` | `cr-0015-security-review.md` |
| Audit action | `audit-####-<title>.md` | `audit-0001-authentication-hardening.md` |

## Related Documents

- [Task Template](../templates/task-template.md)
- [Walkthrough Template](../templates/walkthrough-template.md)
- [Coding Standards](coding-standards.md)
- [Testing Standards](testing-standards.md)
- [v1.0 Roadmap](../../docs/roadmap-v1.md)
