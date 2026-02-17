# Epic 012: Demo & Marketing

## Overview

Create demo assets for showcasing the SaaS template: enhanced seed data that populates a realistic demo environment, and a demo video/walkthrough that highlights key features.

## Success Criteria

- Seed data creates a realistic demo scenario (multiple orgs, users, roles, notifications, work items)
- Demo video (2-3 min) showcasing key features: auth, org management, notifications, i18n, feature flags
- Screenshots/GIFs available for README and marketing pages

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0063 - Enhanced seed data for demos
- **Status:** DONE
- **GitHub Issue:** #215
- **Dependencies:** 0058
- **File:** `docs/tasks/0063-enhanced-seed-data.md`
- **Scope:** Extend existing seed-dev.sh with richer data: multiple organizations, various member roles, pending invitations, notifications, feature flags in different states, work items with various statuses. Make seed data idempotent.
- **Old Issue:** #51 (extend existing)
- **Roadmap Phase:** D1

### Task 0064 - Demo video / walkthrough
- **Status:** PENDING
- **GitHub Issue:** #216
- **Dependencies:** 0063
- **File:** `docs/tasks/0064-demo-video.md`
- **Scope:** Create a 2-3 minute demo video or animated walkthrough showing: registration flow, org creation, member invitation, notification system, language switching, feature flags, org settings, user profile. Write script first, then record.
- **Old Issue:** #53
- **Roadmap Phase:** D2

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Demo | 0063-0064 | 1 | 1 |
| **Total** | **2** | **1** | **1** |

## Dependency Graph

```
0058 (README, epic 010) ──► 0063 (Seed data) ──► 0064 (Demo video, last task)
```
