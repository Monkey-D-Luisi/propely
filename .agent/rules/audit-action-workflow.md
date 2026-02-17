# Audit Action Workflow

## Overview

This workflow converts audit action plan items into actionable work items. It reads the Prioritized Action Plan from an epic's executive summary and implements each remediation item sequentially, tracking progress in both the action documentation and the executive summary.

## Trigger Command

**Command:** `next audit action` or `next audit action <NNN>`

- `next audit action` — Automatically selects the next pending action across all audits.
- `next audit action <NNN>` — Works only on the specified epic's audit (e.g., `next audit action 002`).

When the user issues this command, the agent:
1. Locates the latest executive summary in `docs/audits/`.
2. Finds the next unstarted action item from the Prioritized Action Plan.
3. Creates a feature branch for the work.
4. Creates audit action documentation and a matching walkthrough.
5. Implements the remediation.
6. Runs quality checks.
7. Updates the executive summary status to "Done".
8. Commits all changes.

## Workflow Steps

### Step 0: Sync Main Branch

```bash
git checkout main && git pull origin main
```

Ensure you're starting from the latest state.

### Step 1: Find the Next Action Item

#### If the user specified an epic number (`next audit action <NNN>`):
1. Read `docs/audits/epic-<NNN>-executive-summary.md`.
2. If the file does not exist, inform the user and suggest running `audit epic <NNN>` first.
3. Parse the **Prioritized Action Plan** table.
4. Select the first row where `Status` = `Not started` (respecting priority order and dependencies).
5. If all items are `Done`, update the `Status` field in the metadata section to `Complete` and inform the user.

#### If no epic number was specified (`next audit action`):
1. List all executive summaries in `docs/audits/` (pattern: `epic-<NNN>-executive-summary.md`).
2. Read the **Audit Metadata** section of each file and check the `Status` field.
3. **Skip** any summary where `Status: Complete`.
4. Among the remaining (`In Progress`), select by **lowest epic number first** (oldest audit gets resolved first).
5. Within that audit, parse the **Prioritized Action Plan** table and select the first `Not started` row (respecting priority order and dependencies).
6. If no `In Progress` audits remain, inform the user that all audit actions across all epics are complete.

#### Dependency resolution:
- If an item has `Dependencies` other than `None`, verify those dependency items are `Done` first.
- If dependencies are not met, skip to the next eligible item in the same audit.

#### Status lifecycle:
- When the agent picks an action from an audit, the audit stays `In Progress`.
- When the **last** `Not started` item in an audit is completed, the agent updates the metadata `Status` to `Complete`.

### Step 2: Create a Feature Branch

```bash
git checkout -b fix/audit-<NNNN>-<short-slug>
```

Example: `fix/audit-0001-cookie-secure-flag`

### Step 3: Create Audit Action Documentation

1. Determine the next audit action ID by scanning `docs/tasks/audit-*.md` and incrementing the highest numeric suffix. If none exist, start at `0001`. Audit action IDs are **global** (not per-epic). A single `audit-NNNN` sequence spans all epics.
2. Create `docs/tasks/audit-<NNNN>-<slug>.md` using the template in `.agent/templates/audit-action-template.md`.
   - Link to the executive summary and the specific action plan item number.
   - Copy the Description, Files, and Severity from the action plan row.
3. Create the matching walkthrough file: `docs/walkthroughs/audit-<NNNN>-<slug>.md`.

### Step 4: Implement the Remediation

Follow Clean Architecture layer order when applicable:
1. **Domain** — Pure business logic changes
2. **Application** — Use case / handler changes
3. **Infrastructure** — External service / persistence changes
4. **Api** — Controller, middleware, configuration changes
5. **Frontend** — React components, hooks, schemas

Write tests alongside production code. Every fix should have a corresponding test that verifies the issue is resolved.

### Step 5: Run Quality Checks

Run the relevant builds and tests for the affected services:

```bash
# Backend
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

# Frontend (if applicable)
cd apps/web && npm run build && npm test
```

All checks must pass before proceeding. If a test fails, fix it before continuing.

### Step 6: Update the Walkthrough

Document in `docs/walkthroughs/audit-<NNNN>-<slug>.md`:
- Summary of the action taken
- Root cause analysis (why the issue existed)
- Decisions and trade-offs
- Files changed (with line references)
- Tests added or modified
- Verification steps
- Follow-ups (if any)

### Step 7: Update Executive Summary Status

In the executive summary's Prioritized Action Plan table, update the row's `Status` column:
- Change from `Not started` to `Done`.
- This allows the next `next audit action` invocation to skip completed items.

If this was the **last** `Not started` item in the action plan:
- Update the metadata `Status` field from `In Progress` to `Complete`.
- Inform the user that all actions for this epic's audit are now complete.

### Step 8: Mark Task Documentation as Done

Update the audit action document (`docs/tasks/audit-<NNNN>-<slug>.md`):
- Set `Status: DONE`
- Check all DOD checkboxes

### Step 9: Commit Changes

```bash
git add <specific-files>
git commit -m "fix(<scope>): <description> (#audit-<NNNN>)"
```

Use conventional commit format. The scope should match the affected area (e.g., `security`, `auth`, `api`).

### Step 10: Report to User

After completing the action:
1. Summarize what was done and which files were changed.
2. Report test results.
3. Tell the user how many action items remain per priority level.
4. Remind them to use `next audit action` to continue, or `pr` to create a pull request for this fix.

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Audit action task | `docs/tasks/audit-<NNNN>-<title>.md` | `docs/tasks/audit-0001-cookie-secure-flag.md` |
| Audit action walkthrough | `docs/walkthroughs/audit-<NNNN>-<title>.md` | `docs/walkthroughs/audit-0001-cookie-secure-flag.md` |
| Feature branch | `fix/audit-<NNNN>-<short-slug>` | `fix/audit-0001-cookie-secure-flag` |

## Priority Processing Order

Actions are processed in strict priority order:
1. **P0** items first (CRITICAL and HIGH security issues — must fix before production)
2. **P1** items next (MEDIUM security, missing critical tests, significant code quality)
3. **P2** items next (LOW security, test gaps, documentation fixes)
4. **P3** items last (Enhancements, refactoring, nice-to-have)

Within the same priority, items are processed in the order listed in the table.

## Retroactive Documentation

If multiple actions were implemented in a batch before documentation was created (e.g., during the audit session itself), create all task and walkthrough files after the fact referencing the same branch/PR, and commit them as a `docs(audit)` change. Use the same audit action ID sequence and format as if the workflow had been followed in real time.

## Related Documents

- [Audit Epic Workflow](audit-epic-workflow.md) — Generates the executive summary that this workflow consumes.
- [Audit Action Template](../templates/audit-action-template.md) — Template for individual action documents.
- [Executive Summary Template](../templates/audit-executive-summary-template.md) — Template for the audit report.
- [Walkthrough Template](../templates/walkthrough-template.md) — Template for walkthrough files.
- [Architecture Standards](architecture-standards.md) — Reference for architecture compliance.
- [Coding Standards](coding-standards.md) — Reference for code quality.
- [Testing Standards](testing-standards.md) — Reference for test coverage.
