# Batch Audit Actions Workflow

## Overview

This workflow processes **all** remaining audit actions from an epic's executive summary in a single session. It wraps the existing `next audit action` workflow in a loop, creating one commit per action, all on a shared feature branch. When every item is done it creates a single pull request containing all fixes.

## Trigger Command

**Command:** `batch audit actions <NNN>`

Example: `batch audit actions 004`

## Workflow Steps

### Step 0: Sync with Main

```bash
git checkout main && git pull origin main
```

### Step 1: Locate the Executive Summary

1. Read `docs/audits/epic-<NNN>-executive-summary.md`.
2. If the file does not exist, inform the user and suggest running `audit epic <NNN>` first.
3. Parse the **Prioritized Action Plan** table.
4. Collect all rows where `Status` = `Not started` (respecting dependency order).
5. If no items remain, inform the user and stop.

### Step 2: Create a Shared Feature Branch

Create a single branch for the entire batch:

```bash
git checkout -b fix/audit-epic-<NNN>-batch
```

Example: `fix/audit-epic-004-batch`

### Step 3: Loop — Process Each Action

For each `Not started` action item, in priority/table order and respecting dependencies:

#### 3a. Determine the next audit action ID

Scan `docs/tasks/audit-*.md` and increment the highest numeric suffix (global sequence across all epics). If none exist, start at `0001`.

#### 3b. Create audit action documentation

1. Create `docs/tasks/audit-<NNNN>-<slug>.md` using `.agent/templates/audit-action-template.md`.
2. Create `docs/walkthroughs/audit-<NNNN>-<slug>.md`.

#### 3c. Implement the remediation

Follow Clean Architecture layer order:
1. Domain
2. Application
3. Infrastructure
4. Api
5. Frontend

Write tests alongside production code.

#### 3d. Run quality checks

```bash
# Backend (if affected)
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

# Frontend (if affected)
cd apps/web && npm run build && npm test
```

All checks must pass before proceeding. If a test fails, fix it before continuing.

#### 3e. Update documentation

1. Fill in the walkthrough with summary, decisions, files changed, tests, verification.
2. Mark audit action task as `DONE` with all DOD checkboxes checked.
3. Update the executive summary action plan row: `Not started` → `Done`.
4. If this was the **last** item, update executive summary metadata `Status` → `Complete`.

#### 3f. Commit

```bash
git add <specific-files>
git commit -m "fix(<scope>): <description> (#audit-<NNNN>)"
```

One commit per action item. This gives a clean, reviewable history in the PR.

#### 3g. Continue loop

Return to **3a** for the next `Not started` item.

### Step 4: Push and Create Pull Request

After all items are committed:

```bash
git push -u origin fix/audit-epic-<NNN>-batch
```

Create a PR following the PR template at `.github/PULL_REQUEST_TEMPLATE.md`:

- **Title:** `fix(audit): address all audit findings for epic <NNN>`
- **Body:** List every audit action item processed, the audit action IDs, a combined testing section, and a summary of what was fixed.
- Use `gh pr create`.

### Step 5: Report to User

1. Print a summary table showing each action item, its audit ID, and its commit hash.
2. Report total test results (backend + frontend).
3. Print the PR URL.

## Error Handling

### Build/Test Failure During Loop
1. Fix the failure before continuing.
2. Amend the current action's commit if the fix is trivial, or add a follow-up fix commit.
3. Document the issue in the action's walkthrough.

### Blocked Action (Unmet Dependency)
1. Skip the blocked item and continue with the next eligible one.
2. Return to the skipped item after its dependency is resolved later in the loop.
3. If the dependency cannot be resolved within the batch, leave the item as `Not started` and note it in the PR body.

## Related Documents

- [Audit Action Workflow](audit-action-workflow.md) — Single-action workflow this batch wraps.
- [Audit Epic Workflow](audit-epic-workflow.md) — Generates the executive summary.
- [PR Workflow](pr-workflow.md) — PR creation process.
- [PR Template](../../.github/PULL_REQUEST_TEMPLATE.md) — PR body template.
