# Fast Track Workflow

## Overview

Fast track tasks (`ft-NNNN-*`) are used for urgent or meta-work that doesn't fit the standard backlog flow. Examples include:
- Documentation consolidation
- Urgent hotfixes
- Process improvements
- Agent governance updates

## When to Use Fast Track

| Use Fast Track | Use Standard Task |
|----------------|-------------------|
| Meta-work (docs, process, tooling) | Feature implementation |
| Urgent fixes outside planned backlog | Planned backlog items |
| One-off improvements | Recurring patterns |
| Agent/workflow updates | Product features |

## Naming Convention

```
ft-NNNN-<short-slug>.md
```

### How to Pick `NNNN`

1. Scan `docs/tasks/` for existing `ft-*-*.md` files
2. Extract the numeric portion
3. Use the next sequential number

## Workflow

### Step 1: Create Task File

Create `docs/tasks/ft-NNNN-<slug>.md` with:
- Clear goal
- Scope (in/out)
- Acceptance criteria
- Files to modify

### Step 2: Create Walkthrough File

Create `docs/walkthroughs/ft-NNNN-<slug>.md` with same filename.

### Step 3: Implement

Follow the same quality standards as regular tasks:
- Build must pass (if applicable)
- Tests must pass (if applicable)
- No secrets committed
- Walkthrough updated

### Step 4: Commit

Use conventional commit with `ft-` reference:

```bash
git commit -m "docs(agent): description of change (#ft-NNNN)"
```

### Step 5: Update Walkthrough

Document:
- What was done
- Decisions made
- Files changed
- Any follow-ups

## Related Documents

- [Autonomous Workflow](autonomous-workflow.md)
- [Code Review Workflow](code-review-workflow.md)
- [Task Template](../templates/task-template.md)
