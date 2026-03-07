# Epic Workflow

## Overview

This document defines how the AI agent operates when processing an entire epic. Unlike the `next task` workflow (which processes one task and creates one PR), the epic workflow processes ALL remaining tasks in a single epic on a shared feature branch and creates ONE combined pull request at the end.

**CRITICAL: Non-stop execution.** The agent MUST execute ALL actionable tasks in the epic sequentially without pausing, asking for confirmation, or stopping between tasks. Each task is handled by recursively applying the `next task` workflow logic (read requirements → TDD → implement → test → commit → mark complete) to every task in dependency order. The agent should only stop when:
1. All tasks in the epic are DONE, or
2. All remaining tasks have unresolvable cross-epic blockers, or
3. An insurmountable technical error occurs that cannot be fixed.

Do NOT stop between tasks. Do NOT ask for user input between tasks. Run to completion.

## Trigger Command

**Command:** `"next epic"` or `"next epic: <epic-ref>"`

When the user issues this command, the agent autonomously:
1. Selects the target epic (auto or user-specified)
2. Plans the execution order (topological sort by dependencies)
3. Executes each task sequentially (docs, implement, test, commit)
4. Creates a single combined PR for the entire epic

## Task Document Rules

All rules from `autonomous-workflow.md` apply identically:
- Task Document Immutability (only Status and DoD checkboxes may change)
- Manual Verification Hand-off
- Scope Discipline

## Workflow Steps

### Step 0: Sync with Main

Before anything else, ensure a clean starting point:

1. Stash or commit any uncommitted changes
2. Switch to `main` branch: `git checkout main`
3. Pull latest: `git pull origin main`
4. **Do NOT create the branch yet** -- wait until Step 1 identifies the epic

### Step 1: Select Target Epic

#### 1a: User-specified epic

If the user provided an epic reference (e.g., `next epic: P3`, `next epic: properties`, `next epic: P7-polish`):

1. Resolve the reference to an epic file in `docs/backlog/epic-P*-*.md`
2. Read the epic file and verify it has remaining tasks (not all DONE/DEFERRED)
3. If the epic is fully complete or DEFERRED, report to user and wait

#### 1b: Automatic selection

If no epic was specified:

1. **Read `docs/roadmap.md`** to get the phase ordering (P0 through P7)
2. **Read ALL `docs/backlog/epic-P*-*.md` files** to get current task statuses
3. **Scan epics in phase order** (P0, P1, P2, ..., P7):
   - Skip epics where all tasks are DONE or DEFERRED
   - For each epic with remaining tasks, check if at least one task has all dependencies met (both intra-epic and cross-epic dependencies resolved to DONE)
   - A BLOCKED task whose blockers are now all DONE counts as actionable
4. **Select the first epic** in phase order with at least one actionable task
5. If no epic has actionable tasks, report to user and wait

### Step 2: Plan Epic Execution

After selecting the epic:

1. **Read the epic file** and extract the task summary table
2. **Identify remaining tasks**: tasks with status PENDING, IN_PROGRESS, or BLOCKED (skip DONE and DEFERRED)
3. **Build dependency graph**:
   - For each remaining task, parse its dependency list (e.g., "3.1, 4.5")
   - Classify each dependency as intra-epic (same phase) or cross-epic (different phase)
   - For cross-epic dependencies, read the relevant epic file and check if the dependency is DONE
4. **Topological sort** remaining tasks:
   - Tasks with all cross-epic dependencies met AND no unresolved intra-epic dependencies are eligible first
   - Use Kahn's algorithm (BFS) with task number as tiebreaker
5. **Partition into actionable vs. blocked**:
   - **Actionable**: all cross-epic deps are DONE; intra-epic deps are either DONE or will be completed by a preceding task in the topological order
   - **Blocked**: at least one cross-epic dependency is not DONE (and is not within this epic)
6. **Report the execution plan** before starting:
   ```
   === Epic Execution Plan ===
   Epic: P4 -- Contacts & Leads
   Branch: epic/P4-contacts-leads

   Actionable tasks (in execution order):
   1. Task 4.1 -- Contact Domain Model
   2. Task 4.2 -- Lead Domain Model
   3. Task 4.3 -- Contacts & Leads Persistence + API
   ...

   Blocked tasks (cannot execute):
   - Task 4.6 -- Frontend Contacts UI (depends on 2.1, not DONE)

   Proceeding with execution...
   ```
7. **Create the epic branch**: `git checkout -b epic/<PX>-<slug>`
   - Slug derived from epic file name (e.g., `epic-P4-contacts-leads.md` -> `epic/P4-contacts-leads`)

### Step 3: Execute Task Loop

**Execute ALL tasks without stopping.** For each task in the planned execution order, execute Steps 3a through 3h. After completing each task, immediately proceed to the next — do not pause, ask the user, or yield control. Re-evaluate remaining tasks after each completion (a previously blocked intra-epic task may become eligible).

#### Step 3a: Create Task Documentation

1. Determine the next task NNNN number by scanning `docs/tasks/` for the highest existing number
2. Create task file: `docs/tasks/NNNN-<task-title>.md`
   - Use template from `.agent/templates/task-template.md`
   - Copy requirements from the epic file's task section
   - Add implementation details
3. Create walkthrough file: `docs/walkthroughs/NNNN-<task-title>.md`
   - Use template from `.agent/templates/walkthrough-template.md`
   - Start with task reference and initial context

#### Step 3b: Update Task Status to IN_PROGRESS

In the epic file, change the task status:
```markdown
**Status:** PENDING  ->  **Status:** IN_PROGRESS
```
(or `BLOCKED` -> `IN_PROGRESS` if the task was previously blocked but is now eligible)

Also update the task summary table row to show `IN_PROGRESS`.

#### Step 3c: Implement the Solution

Follow the same rules as `autonomous-workflow.md` Step 4:

**For backend tasks**, follow Clean Architecture layer order with TDD:
1. **Domain Layer** -- Write unit tests FIRST, then entities/value objects/events
2. **Application Layer** -- Write handler tests FIRST, then commands/queries/handlers
3. **Infrastructure Layer** -- Write integration tests, then repositories/services
4. **Presentation Layer** -- Write API integration tests, then controllers/DTOs

**For UI tasks**, follow the Use Case -> Design -> TDD flow:
1. Define use cases exhaustively
2. Generate Stitch MCP design (mandatory before code)
3. Write component tests FIRST (React Testing Library)
4. Implement components pixel-perfect against Stitch design
5. Verify with visual comparison

For each layer:
- Write tests FIRST, then production code (TDD: Red -> Green -> Refactor)
- Follow coding standards from `.agent/rules/coding-standards.md`

#### Step 3d: Run Quality Checks (Per-Task Gate)

After implementing each task, run quality checks for ALL affected services:

```bash
# Build affected service(s)
dotnet build services/<affected>/Propely.<Service>.sln

# Test affected service(s)
dotnet test services/<affected>/Propely.<Service>.sln

# Frontend (if affected)
cd apps/web && npm run build && npm test
```

All checks must pass before proceeding to the next task.

#### Step 3e: Update Walkthrough

Document in the walkthrough file:
- **Summary**: What was implemented
- **Decisions**: Choices made and why
- **Commands Run**: Exact commands executed
- **Files Changed**: List with brief descriptions
- **Tests**: What was tested and results
- **Follow-ups**: Any items for future tasks

#### Step 3f: Commit Implementation

```bash
git add <specific-files>
git commit -m "<type>(<scope>): <description> (#NNNN)

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

Use the standard commit message format. Types: `feat`, `fix`, `test`, `docs`, `refactor`, `chore`.

#### Step 3g: Mark Task Complete

1. Update epic file: `IN_PROGRESS` -> `DONE` (both in the task detail section and the summary table)
2. Update walkthrough checklist (check all boxes)
3. Update task file `docs/tasks/NNNN-*.md`: Status -> `DONE`, check all DoD boxes
4. Verify all three files were updated
5. Commit status updates:
```bash
git add <updated-docs>
git commit -m "docs(<scope>): mark task NNNN complete

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

#### Step 3h: Re-evaluate Remaining Tasks

After completing the current task:

1. Check if any previously blocked intra-epic tasks are now eligible (their dependencies within the epic may now be DONE)
2. If newly eligible tasks exist, add them to the execution queue in topological order
3. Proceed to the next task in the queue
4. If no more tasks are eligible, exit the task loop

### Step 4: Final Quality Gate

After ALL actionable tasks are complete, run a comprehensive quality check across ALL services:

```bash
# Build ALL services
dotnet build services/ai-api/Propely.AiApi.sln
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln

# Test ALL services
dotnet test services/ai-api/Propely.AiApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
dotnet test services/appointments-api/Propely.AppointmentsApi.sln
dotnet test services/publishing-api/Propely.PublishingApi.sln

# Frontend
cd apps/web && npm run build && npm test
```

All checks must pass. If any fail, diagnose and fix before proceeding.

### Step 5: Create Combined Pull Request

**Abort condition:** If zero tasks were completed (all were blocked), do NOT create a PR. Report the situation to the user and exit.

1. Push the branch: `git push -u origin epic/<PX>-<slug>`
2. Identify any linked GitHub Issues from task files
3. Create PR using `gh pr create` with the epic PR format:

```bash
gh pr create --title "feat(<scope>): <epic title> (#epic-PX)" --body "$(cat <<'EOF'
<Closes #NNN if applicable>

# Summary

Epic PX -- <Epic Title>

<2-3 sentence summary of the epic and what was accomplished>

## Tasks Completed

| Task | ID | Title | Commit |
|------|-----|-------|--------|
| X.Y | #NNNN | <Title> | <short-sha> |
| X.Z | #NNNN | <Title> | <short-sha> |

## Tasks Remaining (Blocked)

<If all tasks completed: "None -- epic fully completed.">
<If some tasks blocked:>

| Task | Title | Blocked By | Reason |
|------|-------|------------|--------|
| X.W | <Title> | <dep ref> | <cross-epic dependency not DONE> |

## Scope / Changes

- <Service/area 1>: <brief description>
- <Service/area 2>: <brief description>

## How to Test

1. <Step 1>
2. <Step 2>

## Testing

- [ ] `dotnet build services/ai-api/Propely.AiApi.sln`
- [ ] `dotnet build services/orgs-api/Propely.OrgsApi.sln`
- [ ] `dotnet build services/properties-api/Propely.PropertiesApi.sln`
- [ ] `dotnet build services/contacts-api/Propely.ContactsApi.sln`
- [ ] `dotnet build services/appointments-api/Propely.AppointmentsApi.sln`
- [ ] `dotnet build services/publishing-api/Propely.PublishingApi.sln`
- [ ] `dotnet test services/ai-api/Propely.AiApi.sln`
- [ ] `dotnet test services/orgs-api/Propely.OrgsApi.sln`
- [ ] `dotnet test services/properties-api/Propely.PropertiesApi.sln`
- [ ] `dotnet test services/contacts-api/Propely.ContactsApi.sln`
- [ ] `dotnet test services/appointments-api/Propely.AppointmentsApi.sln`
- [ ] `dotnet test services/publishing-api/Propely.PublishingApi.sln`
- [ ] `cd apps/web && npm run build`
- [ ] `cd apps/web && npm test`
- [ ] Other (describe):

## Risks

- <Risk description or "None identified">

## Docs / Notes

- Task files: docs/tasks/NNNN-*.md (one per task completed)
- Walkthroughs: docs/walkthroughs/NNNN-*.md (one per task completed)
- Epic file: docs/backlog/epic-PX-<slug>.md (updated statuses)

## Quality Gates

- [x] Code compiles and runs locally
- [x] Formatting/linting checks pass (if applicable)
- [x] Tests added/updated and passing
- [x] No secrets committed
- [x] Walkthroughs updated (docs/walkthroughs)
EOF
)"
```

### Step 6: Report Results

Output to user:
- PR URL
- PR title
- Number of tasks completed vs. total in epic
- Any tasks that remain blocked with reasons
- Target branch

## Quality Gates (Per-Task Definition of Done)

Same as `autonomous-workflow.md`:

| Gate | Verification |
|------|--------------|
| Acceptance criteria met | All AC items checked in task file |
| Code builds | `dotnet build` succeeds for affected service(s) |
| Tests pass | `dotnet test` succeeds for affected service(s) |
| No secrets | No credentials in code or config |
| Walkthrough accurate | Reflects actual implementation |
| Committed | Changes are in version control |

## Error Handling

### Build Failure (during task)
1. Identify and fix the error
2. Document the issue in walkthrough
3. Re-run build
4. Do NOT skip to the next task until the current one passes

### Test Failure (during task)
1. Determine if test is correct or code is wrong
2. Fix the appropriate part
3. Document any unexpected behavior
4. Re-run tests

### Task Cannot Be Completed (implementation blocker)
If a task encounters an insurmountable technical blocker during implementation:
1. Document the blocker in the walkthrough
2. Mark the task as BLOCKED in the epic file with a reason note
3. Commit the partial work and documentation
4. Proceed to the next eligible task
5. Include the blocked task in the PR's "Tasks Remaining" section

### All Remaining Tasks Blocked
If no tasks can be executed (all remaining tasks have unmet cross-epic dependencies):
1. Report to user with specific blockers
2. Do NOT create a PR if zero tasks were completed
3. If some tasks were completed before hitting the wall, create the PR with partial results

### Final Quality Gate Failure
If the comprehensive build/test in Step 4 fails:
1. Identify which service/test is failing
2. Determine if the failure is related to the epic's changes or pre-existing
3. Fix the issue
4. Re-run the full quality gate
5. Do NOT create the PR until all checks pass

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Epic branch | `epic/<PX>-<slug>` | `epic/P4-contacts-leads` |
| Task file | `NNNN-<task-title>.md` | `0045-contact-domain-model.md` |
| Walkthrough | `NNNN-<task-title>.md` | `0045-contact-domain-model.md` |

## Commit Message Format

### Implementation commits
```
<type>(<scope>): <description> (#NNNN)

[optional body]

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

### Status update commits
```
docs(<scope>): mark task NNNN complete

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

## PR Title Format

| Scenario | Format | Example |
|----------|--------|---------|
| Full epic | `feat(<scope>): <epic title> (#epic-PX)` | `feat(contacts): Contacts & Leads (#epic-P4)` |
| Partial epic | `feat(<scope>): <epic title> - partial (#epic-PX)` | `feat(contacts): Contacts & Leads - partial (#epic-P4)` |

## Related Documents

- [Autonomous Workflow](autonomous-workflow.md) -- single-task workflow (reused patterns)
- [PR Workflow](pr-workflow.md) -- PR creation patterns
- [Task Template](../templates/task-template.md)
- [Walkthrough Template](../templates/walkthrough-template.md)
- [Coding Standards](coding-standards.md)
- [Testing Standards](testing-standards.md)
- [v1.0 Roadmap](../../docs/roadmap.md)
