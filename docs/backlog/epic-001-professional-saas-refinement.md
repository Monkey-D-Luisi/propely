# Epic 001: Professional SaaS Template Refinement

## Overview

Transform the current working monorepo into a production-grade, exemplary SaaS template that can serve as the foundation for any SaaS product built by AI agents. The template must be professional, clean, maintainable, well-tested, and fully internationalized.

## Success Criteria

- All frontend strings externalized and available in EN + ES
- Automatic locale detection from browser
- All forms use react-hook-form + zod (no manual useState)
- Vitest + RTL test coverage on all frontend code
- Error boundary, custom error pages, consistent loading states
- Backend: pagination, soft delete, audit logging, search/filtering
- Dockerfiles for every service
- CI with SAST, dependency checking, coverage gates
- Organization settings, user profile, notifications, feature flags
- Zero `any` types, zero Spanish comments, zero `console.error`

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| i18n | next-intl | Best App Router support, server components, popular |
| Forms | react-hook-form + @hookform/resolvers/zod | Industry standard, reuses existing Zod schemas |
| Frontend testing | Vitest + React Testing Library | Fast, modern, great Next.js integration |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Phase 1: Foundation & Cleanup

#### Task 0001 - Code cleanup and consistency sweep
- **Status:** DONE
- **GitHub Issue:** #112
- **Dependencies:** None
- **File:** `docs/tasks/0001-code-cleanup.md`
- **Scope:** Remove Spanish comments, console.error, `any` types, hardcoded strings. Enforce consistent patterns across all frontend and backend code.

#### Task 0002 - Setup Vitest and React Testing Library
- **Status:** DONE
- **GitHub Issue:** #113
- **Dependencies:** None
- **File:** `docs/tasks/0002-vitest-setup.md`
- **Scope:** Install and configure Vitest + RTL + jsdom for the web app. Create test utilities, mocks, and a sample test to verify the setup works.

#### Task 0003 - Setup next-intl infrastructure
- **Status:** DONE
- **GitHub Issue:** #114
- **Dependencies:** None
- **File:** `docs/tasks/0003-next-intl-setup.md`
- **Scope:** Install next-intl, configure middleware for locale detection, create message files structure, setup provider in layout. No string extraction yet.

#### Task 0004 - Setup react-hook-form and create form primitives
- **Status:** DONE
- **GitHub Issue:** #115
- **Dependencies:** None
- **File:** `docs/tasks/0004-react-hook-form-setup.md`
- **Scope:** Install react-hook-form + @hookform/resolvers. Create reusable form primitives (FormField, FormError, FormSubmitButton) that integrate with the existing design system.

### Phase 2: Internationalization

#### Task 0005 - Extract all frontend strings to EN locale
- **Status:** DONE
- **GitHub Issue:** #116
- **Dependencies:** 0003
- **File:** `docs/tasks/0005-extract-strings-en.md`
- **Scope:** Extract every user-facing string from all components into EN message files organized by feature. Integrate next-intl's `useTranslations` into every component.

#### Task 0006 - Add ES translations and language switcher
- **Status:** DONE
- **GitHub Issue:** #117
- **Dependencies:** 0005
- **File:** `docs/tasks/0006-es-translations-switcher.md`
- **Scope:** Create ES translation files. Add locale detection from browser Accept-Language. Create language switcher component in AppHeader.

### Phase 3: UX/UI Professional

#### Task 0007 - Refactor auth forms with react-hook-form
- **Status:** DONE
- **GitHub Issue:** #118
- **Dependencies:** 0004, 0005
- **File:** `docs/tasks/0007-refactor-auth-forms.md`
- **Scope:** Rewrite LoginForm and RegisterForm using react-hook-form + zod + form primitives. Integrate i18n for all labels and error messages.

#### Task 0008 - Refactor org forms with react-hook-form
- **Status:** DONE
- **GitHub Issue:** #119
- **Dependencies:** 0004, 0005
- **File:** `docs/tasks/0008-refactor-org-forms.md`
- **Scope:** Rewrite InviteForm, CreateOrgDialog, and LeaveOrgButton confirmation using react-hook-form + zod + form primitives. Integrate i18n.

#### Task 0009 - Error boundary and custom error pages
- **Status:** DONE
- **GitHub Issue:** #120
- **Dependencies:** 0005
- **File:** `docs/tasks/0009-error-boundary-pages.md`
- **Scope:** Create global ErrorBoundary component, custom not-found.tsx and error.tsx pages at app level. All strings from i18n.

#### Task 0010 - Standardize loading states and skeleton components
- **Status:** DONE
- **GitHub Issue:** #121
- **Dependencies:** 0005
- **File:** `docs/tasks/0010-loading-skeletons.md`
- **Scope:** Create reusable Skeleton primitives (SkeletonText, SkeletonCard, SkeletonTable). Add loading.tsx files for each route segment. Ensure consistent loading experience.

#### Task 0011 - Accessibility audit and improvements
- **Status:** DONE
- **GitHub Issue:** #122
- **Dependencies:** 0007, 0008, 0009
- **File:** `docs/tasks/0011-accessibility.md`
- **Scope:** Focus management on route transitions, skip-to-content link, ARIA live regions for toasts, keyboard navigation for modals/dialogs, proper heading hierarchy.

### Phase 4: Frontend Testing

#### Task 0012 - Tests for schemas, utilities, and API client
- **Status:** DONE
- **GitHub Issue:** #123
- **Dependencies:** 0002
- **File:** `docs/tasks/0012-tests-schemas-utils.md`
- **Scope:** Write tests for all Zod schemas in lib/schemas.ts, the apiFetch utility, CSRF helpers, and any other utility functions.

#### Task 0013 - Tests for custom hooks
- **Status:** DONE
- **GitHub Issue:** #124
- **Dependencies:** 0002, 0007, 0008
- **File:** `docs/tasks/0013-tests-hooks.md`
- **Scope:** Write tests for useCurrentUser, useMyOrgs, useMembers, useCreateOrg, useInviteMember, useUpdateRole, useLeaveOrg using renderHook with mocked apiFetch.

#### Task 0014 - Tests for UI components
- **Status:** DONE
- **GitHub Issue:** #125
- **Dependencies:** 0002, 0007, 0008, 0009
- **File:** `docs/tasks/0014-tests-components.md`
- **Scope:** Write tests for LoginForm, RegisterForm, InviteForm, MembersTable, MembersManager, LeaveOrgButton, AppHeader, ErrorBoundary, Toast.

### Phase 5: Backend Robustness

#### Task 0015 - Pagination and search/filtering on list endpoints
- **Status:** DONE
- **GitHub Issue:** #126
- **Dependencies:** None
- **File:** `docs/tasks/0015-backend-pagination-search.md`
- **Scope:** Add cursor or offset pagination to GetMembers, GetMyOrgs. Add search by name/email on members. Return pagination metadata in responses. Update frontend to consume pagination.

#### Task 0016 - Soft delete pattern
- **Status:** DONE
- **GitHub Issue:** #127
- **Dependencies:** None
- **File:** `docs/tasks/0016-soft-delete.md`
- **Scope:** Add IsDeleted + DeletedAtUtc to entities. Add global query filter in EF Core. Create undelete capability. Apply to organizations and memberships.

#### Task 0017 - Audit logging infrastructure
- **Status:** DONE
- **GitHub Issue:** #128
- **Dependencies:** None
- **File:** `docs/tasks/0017-audit-logging.md`
- **Scope:** Create AuditLog entity. Capture who/what/when for mutations (create, update, delete). Store in dedicated audit_logs table. Add correlation with request correlation ID.

#### Task 0018 - Global exception handler and standardized error responses
- **Status:** DONE
- **GitHub Issue:** #129
- **Dependencies:** None
- **File:** `docs/tasks/0018-global-exception-handler.md`
- **Scope:** Create ExceptionHandlerMiddleware. Define ProblemDetails-based error response format (RFC 7807). Map domain exceptions to HTTP status codes. Remove per-controller try/catch.

#### Task 0019 - Backend test coverage improvement
- **Status:** DONE
- **GitHub Issue:** #130
- **Dependencies:** 0015, 0016, 0017, 0018
- **File:** `docs/tasks/0019-backend-tests.md`
- **Scope:** Add unit tests for all command/query handlers. Add integration tests for all API endpoints. Target: Domain >90%, Application >80%, Infrastructure >60%.

### Phase 6: DevOps & Production Readiness

#### Task 0020 - Dockerfiles for all services
- **Status:** DONE
- **GitHub Issue:** #131
- **Dependencies:** None
- **File:** `docs/tasks/0020-dockerfiles.md`
- **Scope:** Create multi-stage Dockerfiles for ai-api, orgs-api, and web. Add docker-compose.production.yml. Optimize image sizes with .dockerignore.

#### Task 0021 - CI improvements (SAST, dependency checking, coverage gates)
- **Status:** DONE
- **GitHub Issue:** #132
- **Dependencies:** 0019
- **File:** `docs/tasks/0021-ci-improvements.md`
- **Scope:** Add CodeQL/Semgrep for SAST. Add Dependabot or Renovate. Add coverage gates (fail CI if below threshold). Add badge to README.

#### Task 0022 - Migration automation and improved health checks
- **Status:** DONE
- **GitHub Issue:** #133
- **Dependencies:** 0020
- **File:** `docs/tasks/0022-migration-health-checks.md`
- **Scope:** Auto-apply EF migrations on service startup (with locking). Add readiness + liveness health check endpoints. Add dependency health checks (DB, Redis, RabbitMQ).

### Phase 7: SaaS Features

#### Task 0023 - Organization settings page
- **Status:** DONE
- **GitHub Issue:** #134
- **Dependencies:** 0005, 0004, 0015
- **File:** `docs/tasks/0023-org-settings.md`
- **Scope:** Add org settings page (rename, description). Backend: PATCH /orgs/:id endpoint. Frontend: settings form with react-hook-form + i18n. Only owners/admins can access.

#### Task 0024 - User profile page
- **Status:** DONE
- **GitHub Issue:** #135
- **Dependencies:** 0005, 0004
- **File:** `docs/tasks/0024-user-profile.md`
- **Scope:** Add user profile page (edit name, change password). Backend: PATCH /auth/me, PUT /auth/password endpoints. Frontend: profile form with react-hook-form + i18n.

#### Task 0025 - In-app notification system
- **Status:** DONE
- **GitHub Issue:** #136
- **Dependencies:** 0005, 0017
- **File:** `docs/tasks/0025-notifications.md`
- **Scope:** Backend: Notification entity, endpoints (list, mark-read). Frontend: notification bell in AppHeader, notification dropdown. Triggered by invitations, role changes. Uses i18n.

#### Task 0026 - Feature flags infrastructure
- **Status:** DONE
- **GitHub Issue:** #137
- **Dependencies:** None
- **File:** `docs/tasks/0026-feature-flags.md`
- **Scope:** Create feature flag system. Backend: FeatureFlag entity, admin endpoint to toggle. Frontend: useFeatureFlag hook, FeatureGate component. Config-driven with DB override.

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| 1. Foundation | 0001-0004 | 4 | 0 |
| 2. i18n | 0005-0006 | 2 | 0 |
| 3. UX/UI | 0007-0011 | 5 | 0 |
| 4. Frontend Testing | 0012-0014 | 3 | 0 |
| 5. Backend | 0015-0019 | 5 | 0 |
| 6. DevOps | 0020-0022 | 3 | 0 |
| 7. SaaS Features | 0023-0026 | 4 | 0 |
| **Total** | **26** | **26** | **0** |

## Dependency Graph

```
Phase 1 (parallel):
  0001 ──────────────────────────────────────────────────────┐
  0002 ──────────────────────────────────► 0012, 0013, 0014  │
  0003 ──► 0005 ──► 0006                                    │
  0004 ──┬─► 0007 ──┐                                       │
         └─► 0008 ──┤                                       │
                    ├──► 0011                                │
  0005 ──┬─► 0007  │                                        │
         ├─► 0008  │                                        │
         ├─► 0009 ─┘                                        │
         └─► 0010                                           │
                                                            │
Phase 5 (parallel, no frontend deps):                       │
  0015 ──┐                                                  │
  0016 ──┤                                                  │
  0017 ──┼──► 0019                                          │
  0018 ──┘                                                  │
                                                            │
Phase 6:                                                    │
  0020 ──► 0022                                             │
  0019 ──► 0021                                             │
                                                            │
Phase 7:                                                    │
  0005 + 0004 + 0015 ──► 0023                               │
  0005 + 0004 ──► 0024                                      │
  0005 + 0017 ──► 0025                                      │
  0026 (no deps)                                            │
```
