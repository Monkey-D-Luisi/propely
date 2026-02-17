# Project Backlog

## Overview

This directory contains the product backlog organized by epics. Each epic represents a major feature or capability that spans multiple implementation tasks.

## Current Epics

| Epic | Status | Description |
|------|--------|-------------|
| [Work Item Management](work-item-management-epic.md) | In Progress | First vertical slice implementing full CRUD for Work Items |
| [Work Item Management V2](work-item-management-v2-epic.md) | Pending | Advanced features: Full CRUD, Filtering, Authentication |

## Backlog Structure

Each epic file contains:
- **Overview**: Business context and goals
- **Tasks**: Numbered implementation tasks (0002, 0003, etc.)
- **Dependencies**: Task ordering and prerequisites
- **Acceptance Criteria**: Per-task success criteria

## Task Lifecycle

```
PENDING → IN_PROGRESS → DONE
```

### Status Definitions

| Status | Meaning |
|--------|---------|
| `PENDING` | Task is defined but not started |
| `IN_PROGRESS` | Agent is actively working on this task |
| `DONE` | Task completed, all acceptance criteria met |
| `BLOCKED` | Task cannot proceed due to external dependency |

## How the Agent Uses This Backlog

When the command **"next task"** is given:

1. Agent reads this backlog to find the next `PENDING` task
2. Verifies all dependencies are `DONE`
3. Creates task file in `docs/tasks/NNNN-*.md`
4. Creates walkthrough file in `docs/walkthroughs/NNNN-*.md`
5. Implements the task
6. Updates task status to `DONE` in the epic file

## Adding New Epics

1. Create `docs/backlog/<epic-name>-epic.md`
2. Follow the structure in existing epics
3. Number tasks sequentially (continue from last used number)
4. Update this README with the new epic

## Related Documentation

- [Vertical Slice Definition](../architecture/vertical-slice.md) — Technical architecture for Work Items
- [Autonomous Workflow](../../../../.agent/rules/autonomous-workflow.md) — How the agent processes tasks
- [Task Template](../../../../.agent/templates/task-template.md) — Standard task file format
