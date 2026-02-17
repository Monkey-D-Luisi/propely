# Task: 0063 - Enhanced Seed Data for Demos

## Metadata
- ID: 0063
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #215
- Epic: `docs/backlog/epic-012-demo-marketing.md`
- Old Issue: #51 (extend existing)
- Milestone: v1.0

## Goal
Extend the existing seed script with richer demo data: multiple organizations, various member roles, pending invitations, notifications, feature flags in different states, and work items with various statuses.

## Context
The project has an existing `scripts/seed-dev.sh` (and `.ps1`) that creates basic development data. For demos and screenshots, we need a richer dataset that showcases all features of the template.

### Current Seed Script
- Creates basic users and organizations
- Minimal data, just enough for development

## Scope
### In scope
- Extend seed script with comprehensive demo data
- Multiple organizations (3-5) with different sizes
- Users with various roles (Owner, Admin, Member)
- Pending invitations
- Notifications (unread and read)
- Feature flags in various states (enabled, disabled)
- Work items with all statuses (New, InProgress, Done, Cancelled)
- Make seed data idempotent (safe to run multiple times)
- Realistic names and descriptions (not "test1", "test2")

### Out of scope
- Subscription/billing seed data (depends on billing implementation)
- Audit log seed data (generated automatically by mutations)
- Production seed data

## Requirements
- R1: Running seed script creates a rich demo environment
- R2: Seed script is idempotent (can run multiple times safely)
- R3: Data is realistic (real-looking names, descriptions, dates)
- R4: All features are showcased (orgs, members, notifications, flags, work items)
- R5: Works on fresh and existing databases

## Acceptance Criteria
- AC1: Seed script creates 3+ organizations
- AC2: Each org has members with different roles
- AC3: Notifications exist (unread)
- AC4: Feature flags in mixed states
- AC5: Work items in all statuses
- AC6: Script is idempotent
- AC7: Script runs successfully on clean DB

## Implementation Steps

1. **Audit existing seed script** to understand current approach
2. **Design demo dataset**:
   - Org "Acme Corp" (5 members, Owner + Admin + 3 Members)
   - Org "Startup Labs" (3 members)
   - Org "Solo Dev" (1 member, Owner only)
   - Pending invitation in Acme Corp
   - 5 notifications for primary user
   - 3 feature flags (1 enabled, 1 disabled, 1 config-only)
   - 10 work items across statuses
3. **Extend seed script** with API calls or direct DB inserts
4. **Add idempotency checks** (skip if data exists)
5. **Test on fresh and existing databases**

## Files to Create / Modify

### Create
- `scripts/seed-demo.sh` (rich demo seed script, bash)
- `scripts/seed-demo.ps1` (rich demo seed script, PowerShell)
- `docs/walkthroughs/0063-enhanced-seed-data.md`

### Modify
- `docs/tasks/0063-enhanced-seed-data.md` (status)
- `docs/backlog/epic-012-demo-marketing.md` (status)
- `docs/roadmap-v1.md` (status)

## Testing Plan
- Run seed on fresh DB -> verify all data created
- Run seed again -> verify no duplicates or errors
- Manual: Browse UI and verify rich data visible

## Definition of Done Checklist
- [x] Rich demo data created
- [x] Script is idempotent
- [x] All features showcased
- [x] Walkthrough updated
