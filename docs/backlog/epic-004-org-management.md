# Epic 004: Org & Member Management Completion

## Overview

Complete the organization and member management CRUD operations. The current implementation is missing: leave organization, delete organization (soft delete), and remove member endpoints and UI. These are essential operations for any multi-org SaaS application.

## Success Criteria

- Users can leave an organization (unless they are the last owner)
- Organization owners can delete an organization (soft delete)
- Admins/owners can remove a member from an organization
- All operations have proper authorization checks
- Frontend UI for all three operations
- All strings internationalized (EN + ES)
- Audit log entries created for all mutations

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0036 - Leave organization endpoint + UI
- **Status:** DONE
- **GitHub Issue:** #188
- **Dependencies:** None
- **File:** `docs/tasks/0036-leave-org.md`
- **Scope:** Add DELETE /orgs/{orgId}/members/me endpoint. Prevent last owner from leaving. Update frontend LeaveOrgButton. Add confirmation dialog. Create audit log entry. Add i18n strings.

### Task 0037 - Delete organization (soft delete) endpoint + UI
- **Status:** DONE
- **GitHub Issue:** #189
- **Dependencies:** None
- **File:** `docs/tasks/0037-delete-org.md`
- **Scope:** Add DELETE /orgs/{orgId} endpoint (owner only). Soft-delete org and cascade to memberships. Add delete button on org settings page with confirmation. Create audit log entry. Add i18n strings.

### Task 0038 - Remove member endpoint + UI
- **Status:** DONE
- **GitHub Issue:** #190
- **Dependencies:** None
- **File:** `docs/tasks/0038-remove-member.md`
- **Scope:** Add DELETE /orgs/{orgId}/members/{userId} endpoint (admin/owner only). Prevent removing the last owner. Add remove button to MembersTable. Create audit log entry and notification. Add i18n strings.

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Org Management | 0036-0038 | 3 | 0 |
| **Total** | **3** | **3** | **0** |

## Dependency Graph

```
0036 (Leave org) ── standalone
0037 (Delete org) ── standalone
0038 (Remove member) ── standalone
```
