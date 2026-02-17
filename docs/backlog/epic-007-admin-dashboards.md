# Epic 007: Admin Dashboards & UI Completion

## Overview

Build admin UIs for features that have backend support but no frontend: feature flag management, audit log viewer, and work items CRUD pages for the AI API. The feature flag admin UI is MVP priority; audit log and work items UI are v1.0.

## Success Criteria

- Admins can view, create, edit, and toggle feature flags from a UI
- Admins can view audit log entries with filtering and export (v1.0)
- Users can manage work items (AI API) from the frontend (v1.0)
- Users can create work items from natural language via AI smart input parsing (v1.0)
- All UIs use existing component patterns (react-hook-form, i18n, skeletons)

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0047 - Feature flag admin management UI
- **Status:** DONE
- **GitHub Issue:** #199
- **Dependencies:** None
- **File:** `docs/tasks/0047-feature-flag-admin-ui.md`
- **Scope:** Create /admin/feature-flags page. List all flags with toggle switches. Add create/edit flag dialog. Show flag status (enabled/disabled), source (config/db). Restrict to admin users. Add i18n strings.
- **Old Issue:** #66
- **Milestone:** MVP

### Task 0065 - Design system definition (Stitch + Tailwind)
- **Status:** DONE
- **GitHub Issue:** #250
- **Dependencies:** 0050, 0059, 0053 (Phase A complete)
- **File:** `docs/tasks/0065-design-system.md`
- **Scope:** Use Stitch MCP to explore design directions on existing screens. Select color palette, typography, spacing, shadows. Update Tailwind config and CSS variables. Apply to existing screens. Document conventions.
- **Milestone:** v1.0
- **Roadmap Phase:** 0

### Task 0048 - Audit log viewer UI + export
- **Status:** DONE
- **GitHub Issue:** #200
- **Dependencies:** 0049
- **File:** `docs/tasks/0048-audit-log-ui.md`
- **Scope:** Create /admin/audit-log page. List audit entries with pagination. Add filters (date range, user, action type, entity). Add CSV/JSON export. Restrict to admin users. Add i18n strings.
- **Old Issue:** #63
- **Milestone:** v1.0

### Task 0049 - Work items frontend (AI API CRUD pages)
- **Status:** DONE
- **GitHub Issue:** #201
- **Dependencies:** 0065
- **File:** `docs/tasks/0049-work-items-frontend.md`
- **Scope:** Create /work-items pages: list (with pagination, filtering), create, edit, detail view. Connect to AI API endpoints (port 5010). Use react-hook-form + zod. Add AI smart input parsing: new `POST /v1/work-items/parse` backend endpoint using `IOpenAiService`, "Smart Fill" UI on create form. Add i18n strings.
- **Old Issues:** #20, #22
- **Milestone:** v1.0

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Admin UIs (MVP) | 0047 | 1 | 0 |
| Design System (v1.0) | 0065 | 1 | 0 |
| Admin UIs (v1.0) | 0048-0049 | 2 | 0 |
| **Total** | **4** | **4** | **0** |

## Dependency Graph

```
0047 (Feature flag UI) ── standalone (MVP)
0050, 0059, 0053 (Phase A, other epics) ──► 0065 (Design system) ──► 0049 (Work items + AI) ──► 0048 (Audit log)
```
