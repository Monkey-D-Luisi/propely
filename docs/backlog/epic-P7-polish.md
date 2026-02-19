# Epic P7 -- Polish & Launch Readiness

## Overview

Finalize Propely for production launch by building a consolidated dashboard with analytics, implementing a cross-service notification system (email + in-app), creating a guided onboarding wizard for new agencies, and conducting a comprehensive performance and security audit. This phase takes the feature-complete platform from Phases P0--P6 and adds the operational polish, observability, and hardening required for real-world production use.

**Target:** A production-ready platform with actionable dashboards, reliable notifications, a frictionless first-run experience, and verified performance/security/accessibility baselines.

## Service Ownership

| Capability | Service |
|---|---|
| Dashboard analytics aggregation API | `services/orgs-api` |
| Cross-service data fetching (properties, contacts, appointments) | `services/orgs-api` via NuGet SDK clients |
| Notification domain, persistence, delivery | `services/orgs-api` |
| Email delivery | `services/orgs-api` (Infrastructure layer, SMTP/Mailhog) |
| Onboarding wizard API | `services/orgs-api` |
| Frontend dashboard, notifications, onboarding | `apps/web` |
| Load testing scripts | `tests/load/` (k6) |
| Security & accessibility audit artifacts | `docs/audits/` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 7.1 | Dashboard & Analytics | PENDING | 2.3, 4.3, 5.2 |
| 7.2 | Notification System | PENDING | 1.4, 2.3, 4.3, 5.2 |
| 7.3 | Onboarding Flow | PENDING | 1.2, 1.6, 2.3 |
| 7.4 | Performance & Security Audit | PENDING | All P0--P6 tasks |

---

## Task 7.1 -- Dashboard & Analytics

**Status:** PENDING
**Dependencies:** 2.3 (Properties API), 4.3 (Contacts & Leads API), 5.2 (Appointments API)

### Goal

Provide agents and admins with a single dashboard that surfaces actionable metrics: property statistics by status, lead pipeline summary, and upcoming appointment overview. The dashboard aggregates data from multiple backend services (properties, contacts, appointments) via their NuGet SDK clients, exposing a unified analytics API consumed by the frontend.

### Scope

**In scope:**
- Dashboard analytics API endpoints in `orgs-api` (aggregation layer)
- Property statistics: count by status (Draft, Active, Reserved, Sold, Rented, Archived), count by type, count by operation type, average days-on-market for active listings
- Lead pipeline summary: count by status (New, Contacted, Qualified, Converted, Lost), conversion rate, average time-to-conversion, leads received this week/month
- Appointment calendar overview: today's appointments, this week's count, upcoming 5 appointments, overdue (past + still Scheduled) count
- Agent dashboard: scoped to the agent's own data (my properties, my leads, my appointments)
- Admin/owner dashboard: branch-wide metrics, per-agent breakdown table
- KPI cards, summary charts (bar chart for property status distribution, funnel for lead pipeline)
- Data fetched from properties-api, contacts-api, and appointments API via NuGet SDK clients with caching (Redis, 5-minute TTL)
- Stitch MCP design for agent dashboard and admin dashboard pages
- i18n support (en, es)

**Out of scope:**
- Historical trend charts (time-series analytics requiring dedicated data warehouse)
- Revenue/financial metrics (no billing data in MVP)
- Real-time WebSocket updates (polling with cache is sufficient for launch)
- Custom report builder
- Export to PDF/Excel

### Acceptance Criteria

- [ ] `GET /api/dashboard/agent` returns property stats, lead pipeline summary, and upcoming appointments scoped to the authenticated agent
- [ ] `GET /api/dashboard/admin` returns branch-wide metrics with per-agent breakdown (admin/owner only; agents receive 403)
- [ ] Property stats include: `totalByStatus` (object with status keys and counts), `totalByType`, `totalByOperation`, `averageDaysOnMarket`
- [ ] Lead pipeline includes: `totalByStatus`, `conversionRate` (percentage), `averageTimeToConversion` (days), `leadsThisWeek`, `leadsThisMonth`
- [ ] Appointment overview includes: `todayCount`, `thisWeekCount`, `upcomingAppointments` (next 5, with title/type/start/propertyAddress), `overdueCount`
- [ ] Dashboard data is cached in Redis with 5-minute TTL; cache key includes tenant ID and agent ID
- [ ] Frontend agent dashboard page renders KPI cards, property status bar chart, lead funnel, and upcoming appointments list
- [ ] Frontend admin dashboard page renders branch-wide KPIs and a per-agent performance table (properties count, leads count, conversion rate)
- [ ] Stitch designs exist for both agent and admin dashboard pages; implementation matches pixel-for-pixel
- [ ] All dashboard API endpoints and frontend components have tests (unit + integration)

### Implementation Steps

1. Add NuGet SDK client references to `orgs-api`: `Propely.PropertiesApi.Client`, `Propely.ContactsApi.Client` (appointments are already in orgs-api)
2. Create `Application/Dashboard/Queries/GetAgentDashboardQuery.cs` and handler
3. Create `Application/Dashboard/Queries/GetAdminDashboardQuery.cs` and handler
4. Create DTOs: `AgentDashboardDto`, `AdminDashboardDto`, `PropertyStatsDto`, `LeadPipelineSummaryDto`, `AppointmentOverviewDto`, `AgentPerformanceDto`
5. Implement dashboard query handlers: call SDK clients in parallel (`Task.WhenAll`), aggregate results, apply Redis caching
6. Create `DashboardController` with `GET /api/dashboard/agent` and `GET /api/dashboard/admin` endpoints
7. Add authorization: agent endpoint requires authentication; admin endpoint requires admin/owner role
8. Create Stitch design for agent dashboard page
9. Create Stitch design for admin dashboard page
10. Download Stitch HTML to `.stitch-html/agent-dashboard.html` and `.stitch-html/admin-dashboard.html`
11. Create `useDashboard` hook for data fetching with SWR/React Query
12. Create `DashboardPage` component with KPI cards
13. Create `PropertyStatusChart` component (bar chart using a lightweight chart library)
14. Create `LeadFunnelChart` component
15. Create `UpcomingAppointmentsList` component
16. Create `AgentPerformanceTable` component (admin dashboard)
17. Add i18n keys for dashboard labels and metric names
18. Write unit tests for query handlers with mocked SDK clients
19. Write integration tests for dashboard endpoints
20. Write frontend component tests

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Queries/GetAgentDashboard/GetAgentDashboardQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Queries/GetAgentDashboard/GetAgentDashboardQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Queries/GetAdminDashboard/GetAdminDashboardQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Queries/GetAdminDashboard/GetAdminDashboardQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/AgentDashboardDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/AdminDashboardDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/PropertyStatsDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/LeadPipelineSummaryDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/AppointmentOverviewDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Dashboard/Dtos/AgentPerformanceDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/DashboardController.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Dashboard/GetAgentDashboardQueryHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Dashboard/GetAdminDashboardQueryHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Api.Tests/Controllers/DashboardControllerTests.cs`
- `apps/web/src/app/[locale]/(dashboard)/dashboard/page.tsx`
- `apps/web/src/components/dashboard/KpiCard.tsx`
- `apps/web/src/components/dashboard/PropertyStatusChart.tsx`
- `apps/web/src/components/dashboard/LeadFunnelChart.tsx`
- `apps/web/src/components/dashboard/UpcomingAppointmentsList.tsx`
- `apps/web/src/components/dashboard/AgentPerformanceTable.tsx`
- `apps/web/src/hooks/useDashboard.ts`
- `apps/web/src/components/dashboard/__tests__/KpiCard.test.tsx`
- `apps/web/src/components/dashboard/__tests__/PropertyStatusChart.test.tsx`
- `apps/web/src/components/dashboard/__tests__/LeadFunnelChart.test.tsx`
- `apps/web/src/components/dashboard/__tests__/UpcomingAppointmentsList.test.tsx`
- `apps/web/src/components/dashboard/__tests__/AgentPerformanceTable.test.tsx`
- `.stitch-html/agent-dashboard.html`
- `.stitch-html/admin-dashboard.html`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Api/Propely.OrgsApi.Api.csproj` (add SDK client NuGet references)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (register SDK clients and Redis caching)
- `apps/web/src/components/layout/DashboardSidebar.tsx` (add Dashboard nav item as default landing)
- `apps/web/src/messages/en.json` (add dashboard i18n keys)
- `apps/web/src/messages/es.json` (add dashboard i18n keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `GetAgentDashboardQueryHandler` aggregates data from mocked SDK clients | xUnit + FluentAssertions, mock SDK clients |
| Unit | `GetAdminDashboardQueryHandler` includes per-agent breakdown | xUnit, mock SDK clients |
| Unit | Redis caching: second call within TTL returns cached result | xUnit, mock `IDistributedCache` |
| Unit | Agent endpoint returns only own data; admin endpoint returns branch-wide | xUnit, mock identity |
| Integration | `GET /api/dashboard/agent` returns correct DTO shape | `WebApplicationFactory` |
| Integration | `GET /api/dashboard/admin` returns 403 for non-admin users | `WebApplicationFactory` |
| Unit | `KpiCard` renders value, label, and trend indicator | Vitest + RTL |
| Unit | `PropertyStatusChart` renders bars for each status | Vitest + RTL |
| Unit | `LeadFunnelChart` renders funnel stages | Vitest + RTL |
| Unit | `UpcomingAppointmentsList` renders appointment items with links | Vitest + RTL |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- Dashboard endpoints require authentication; admin endpoint requires admin/owner role
- Redis cache keys are tenant-scoped to prevent cross-tenant data leakage
- SDK client calls propagate `X-Tenant-Id` header via `TenantDelegatingHandler`
- Dashboard does not expose raw entity IDs externally; uses summary counts only
- Per-agent performance data (admin dashboard) is visible only to admin/owner roles

### TDD Reminder

Write query handler unit tests with mocked SDK clients first (define expected DTO shapes and aggregation logic). Then implement the handlers. For frontend, write component render tests with mock data before building the components.

---

## Task 7.2 -- Notification System

**Status:** PENDING
**Dependencies:** 1.4 (Permission system), 2.3 (Properties API), 4.3 (Contacts & Leads API), 5.2 (Appointments API)

### Goal

Implement an in-app and email notification system that alerts users about key events: new lead received, appointment reminders, property status changes, and permission changes. Notifications are stored in-database for the in-app bell/inbox experience and optionally dispatched as emails. Users can configure per-category notification preferences.

### Scope

**In scope:**
- `Notification` domain entity in orgs-api: id, recipientUserId, tenantId, category, title, body, link (deep link to relevant page), isRead, createdAt
- Notification categories: `NewLeadReceived`, `AppointmentReminder`, `PropertyStatusChanged`, `PermissionChanged`
- MediatR event handlers that create notifications in response to domain events:
  - `LeadReceivedV1` -> notify assigned agent (or branch admins if unassigned)
  - `AppointmentScheduledV1` / `AppointmentUpdatedV1` -> notify involved agent and contact (if contact has user account)
  - `PropertyStatusChangedV1` -> notify assigned agent and branch admins
  - Permission override created/removed -> notify affected user
- Email delivery via SMTP (Mailhog in dev, configurable provider in production)
- Email templates: HTML + plain text, branded with Propely logo, i18n (en, es)
- `NotificationPreference` entity: per-user, per-category toggle for in-app and email independently
- API endpoints: list notifications (paginated, filtered by read/unread), mark as read, mark all as read, get unread count, get/update preferences
- Frontend: notification bell icon in header with unread badge, notification dropdown/panel, notification preferences page
- Appointment reminder: background worker that creates reminder notifications 1 hour before appointment start time (configurable)
- Stitch MCP design for notification dropdown and preferences page

**Out of scope:**
- Push notifications (mobile/browser push via FCM/APNs)
- SMS notifications
- Real-time WebSocket delivery (polling with 30-second interval is sufficient for launch)
- Notification grouping/digest (daily summary email)
- Third-party notification services (SendGrid, etc.) -- direct SMTP is sufficient

### Acceptance Criteria

- [ ] `Notification` entity persists with: `Id`, `RecipientUserId`, `TenantId`, `Category`, `Title`, `Body`, `Link`, `IsRead`, `CreatedAtUtc`
- [ ] `NotificationPreference` entity stores per-user, per-category preferences: `InAppEnabled` (default: true), `EmailEnabled` (default: true)
- [ ] When a `LeadReceivedV1` event fires, the assigned agent receives an in-app notification with a deep link to the lead detail page
- [ ] When an appointment is scheduled, the assigned agent receives an in-app notification with a deep link to the calendar
- [ ] The `AppointmentReminderWorker` background service creates reminder notifications 1 hour before appointments and sends email reminders
- [ ] When a property status changes, the assigned agent and branch admins receive notifications
- [ ] When a permission override is created or removed, the affected user receives a notification
- [ ] `GET /api/notifications?page=1&pageSize=20&isRead=false` returns paginated notifications for the authenticated user
- [ ] `PUT /api/notifications/{id}/read` and `PUT /api/notifications/read-all` mark notifications as read
- [ ] Frontend notification bell in the header shows unread count badge; clicking opens a dropdown with recent notifications
- [ ] All notification event handlers, API endpoints, and frontend components have tests

### Implementation Steps

1. Create `Notification` entity in `Domain/Notifications/`
2. Create `NotificationCategory` enum: `NewLeadReceived`, `AppointmentReminder`, `PropertyStatusChanged`, `PermissionChanged`
3. Create `NotificationPreference` entity in `Domain/Notifications/`
4. Create EF Core configurations and migration for `notifications` and `notification_preferences` tables
5. Create `INotificationRepository` and `INotificationPreferenceRepository` interfaces in Application layer
6. Create repository implementations in Infrastructure layer
7. Create `INotificationSender` interface in Application layer with methods: `SendInAppAsync`, `SendEmailAsync`
8. Create `NotificationSender` implementation in Infrastructure that persists notification and optionally sends email
9. Create `IEmailService` interface and `SmtpEmailService` implementation (using `MailKit` or `System.Net.Mail`)
10. Create email templates in `Infrastructure/Notifications/Templates/`: `lead-received.html`, `appointment-reminder.html`, `property-status-changed.html`, `permission-changed.html`
11. Create MediatR event handlers:
    - `LeadReceivedNotificationHandler` (listens for `LeadReceivedV1`)
    - `AppointmentNotificationHandler` (listens for `AppointmentCreatedV1`, `AppointmentUpdatedV1`)
    - `PropertyStatusChangedNotificationHandler` (listens for `PropertyStatusChangedV1`)
    - `PermissionChangedNotificationHandler` (listens for permission override events)
12. Create `AppointmentReminderWorker` background service: queries appointments starting within the next hour that have not yet been reminded, creates notification + sends email
13. Create MediatR commands/queries: `ListNotificationsQuery`, `GetUnreadCountQuery`, `MarkNotificationReadCommand`, `MarkAllNotificationsReadCommand`, `GetNotificationPreferencesQuery`, `UpdateNotificationPreferencesCommand`
14. Create `NotificationsController` API endpoints
15. Create Stitch design for notification dropdown and preferences page
16. Download Stitch HTML to `.stitch-html/`
17. Create `useNotifications` hook (poll for unread count every 30 seconds)
18. Create `NotificationBell` component (header icon with badge)
19. Create `NotificationDropdown` component (recent notifications list)
20. Create `NotificationPreferencesPage` component
21. Add i18n keys for notification titles, bodies, and preferences labels
22. Write unit tests for all event handlers and command/query handlers
23. Write integration tests for API endpoints
24. Write frontend component tests

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Notifications/Notification.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Notifications/NotificationCategory.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Notifications/NotificationPreference.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Interfaces/INotificationRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Interfaces/INotificationPreferenceRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Interfaces/INotificationSender.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Interfaces/IEmailService.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Dtos/NotificationDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Dtos/NotificationPreferenceDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/ListNotifications/ListNotificationsQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/ListNotifications/ListNotificationsQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/GetUnreadCount/GetUnreadCountQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/GetUnreadCount/GetUnreadCountQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/GetNotificationPreferences/GetNotificationPreferencesQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Queries/GetNotificationPreferences/GetNotificationPreferencesQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/MarkNotificationRead/MarkNotificationReadCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/MarkNotificationRead/MarkNotificationReadCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/MarkAllNotificationsRead/MarkAllNotificationsReadCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/MarkAllNotificationsRead/MarkAllNotificationsReadCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/UpdateNotificationPreferences/UpdateNotificationPreferencesCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/Commands/UpdateNotificationPreferences/UpdateNotificationPreferencesCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/EventHandlers/LeadReceivedNotificationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/EventHandlers/AppointmentNotificationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/EventHandlers/PropertyStatusChangedNotificationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Notifications/EventHandlers/PermissionChangedNotificationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/NotificationSender.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/SmtpEmailService.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/Templates/lead-received.html`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/Templates/appointment-reminder.html`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/Templates/property-status-changed.html`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/Templates/permission-changed.html`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Notifications/AppointmentReminderWorker.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/NotificationPreferenceConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/NotificationRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/NotificationPreferenceRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/NotificationsController.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/EventHandlers/LeadReceivedNotificationHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/EventHandlers/AppointmentNotificationHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/EventHandlers/PropertyStatusChangedNotificationHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/EventHandlers/PermissionChangedNotificationHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/Commands/MarkNotificationReadCommandHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Notifications/Queries/ListNotificationsQueryHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Api.Tests/Controllers/NotificationsControllerTests.cs`
- `apps/web/src/components/notifications/NotificationBell.tsx`
- `apps/web/src/components/notifications/NotificationDropdown.tsx`
- `apps/web/src/components/notifications/NotificationItem.tsx`
- `apps/web/src/app/[locale]/(dashboard)/settings/notifications/page.tsx`
- `apps/web/src/hooks/useNotifications.ts`
- `apps/web/src/hooks/useNotificationPreferences.ts`
- `apps/web/src/components/notifications/__tests__/NotificationBell.test.tsx`
- `apps/web/src/components/notifications/__tests__/NotificationDropdown.test.tsx`
- `apps/web/src/components/notifications/__tests__/NotificationItem.test.tsx`
- `.stitch-html/notification-dropdown.html`
- `.stitch-html/notification-preferences.html`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<Notification>`, `DbSet<NotificationPreference>`)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (register notification services, email service, reminder worker)
- `apps/web/src/components/layout/DashboardHeader.tsx` (add `NotificationBell` to header)
- `apps/web/src/messages/en.json` (add notification i18n keys)
- `apps/web/src/messages/es.json` (add notification i18n keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `LeadReceivedNotificationHandler` creates notification for assigned agent | xUnit + FluentAssertions, mock `INotificationSender` |
| Unit | `AppointmentNotificationHandler` creates notification for agent | xUnit, mock `INotificationSender` |
| Unit | `PropertyStatusChangedNotificationHandler` notifies agent and admins | xUnit, mock `INotificationSender` |
| Unit | `PermissionChangedNotificationHandler` notifies affected user | xUnit, mock `INotificationSender` |
| Unit | `NotificationSender` respects user preferences (skips email if disabled) | xUnit, mock `IEmailService` |
| Unit | `AppointmentReminderWorker` finds appointments within 1-hour window | xUnit, mock repository |
| Unit | `MarkNotificationReadCommandHandler` sets `IsRead = true` | xUnit, mock repository |
| Unit | `ListNotificationsQueryHandler` filters by read/unread and paginates | xUnit, mock repository |
| Integration | `GET /api/notifications` returns correct notifications for authenticated user | `WebApplicationFactory` |
| Integration | `PUT /api/notifications/{id}/read` marks notification as read | `WebApplicationFactory` |
| Unit | `NotificationBell` shows unread count badge | Vitest + RTL |
| Unit | `NotificationDropdown` renders notification items with links | Vitest + RTL |
| Unit | `NotificationItem` renders category icon, title, time ago, read/unread styling | Vitest + RTL |
| Manual | Trigger events and verify notifications appear in bell dropdown | Dev environment |
| Manual | Verify emails arrive in Mailhog for each notification category | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- Notifications are scoped to `RecipientUserId` + `TenantId`; users cannot see notifications for other users or tenants
- Email templates must not include sensitive data beyond what is necessary (e.g., lead name, property reference, but not full contact details)
- SMTP credentials stored in environment variables only; never logged
- `AppointmentReminderWorker` runs with system-level access but creates notifications scoped to individual users
- Notification API endpoints require authentication; no public access
- Rate limit notification creation to prevent event storms from flooding user inboxes (max 100 notifications per user per hour)

### TDD Reminder

Write event handler tests first: given a domain event, assert that the correct notification is created with the correct recipient, title, body, and link. Then implement the handlers. For the frontend, write component tests for the bell badge and dropdown rendering before building the components.

---

## Task 7.3 -- Onboarding Flow

**Status:** PENDING
**Dependencies:** 1.2 (Agency Persistence & API), 1.6 (Frontend Agency Management), 2.3 (Property CRUD API)

### Goal

Create a guided onboarding wizard for new agencies that walks the agency owner through the essential first steps: creating the agency, setting up the first branch, inviting team members, and creating the first property listing. The wizard reduces time-to-value by providing a structured, frictionless first-run experience instead of dropping new users into an empty dashboard.

### Scope

**In scope:**
- Onboarding wizard with 4 steps:
  1. **Create Agency** -- agency name, logo upload, contact email, phone
  2. **Create First Branch** -- branch name, address, phone, timezone
  3. **Invite Agents** -- email invitation form (1-5 agents), role assignment (agent/admin), skip option
  4. **Create First Property** -- simplified property form (type, operation, address, price, title, 1 photo), skip option
- `OnboardingStatus` tracking entity: records which steps have been completed per agency
- API endpoints: `GET /api/onboarding/status`, `POST /api/onboarding/complete-step`
- Wizard is shown automatically after first login when `OnboardingStatus` is incomplete
- Each step calls existing API endpoints (agency creation, branch creation, invite member, create property) -- the wizard is a UI orchestration layer, not new business logic
- Skip option on steps 3 and 4 (inviting agents and creating property are optional during onboarding)
- Progress indicator (step 1 of 4, step 2 of 4, etc.)
- Completion celebration screen with links to key pages (dashboard, properties, team settings)
- Stitch MCP design for each wizard step and the completion screen
- i18n support (en, es)

**Out of scope:**
- Billing/subscription setup during onboarding (separate flow)
- Portal configuration (publishing setup is Phase 6)
- Calendar integration setup
- Interactive product tour (tooltips/highlights on existing pages)
- Onboarding analytics (tracking drop-off rates)

### Acceptance Criteria

- [ ] `OnboardingStatus` entity tracks: `AgencyId`, `AgencyCreated`, `FirstBranchCreated`, `AgentsInvited`, `FirstPropertyCreated`, `CompletedAtUtc`
- [ ] `GET /api/onboarding/status` returns the current onboarding state for the authenticated user's agency
- [ ] `POST /api/onboarding/complete-step` updates the onboarding status for the specified step
- [ ] After first login, if onboarding is incomplete, the user is redirected to `/onboarding` instead of the dashboard
- [ ] Step 1 (Create Agency) calls the existing agency creation API and advances to step 2 on success
- [ ] Step 2 (Create First Branch) calls the existing branch creation API and advances to step 3
- [ ] Step 3 (Invite Agents) sends email invitations via the existing invite API; "Skip" button advances to step 4
- [ ] Step 4 (Create First Property) uses a simplified property creation form; "Skip" button advances to completion
- [ ] Completion screen shows a celebration message and links to dashboard, properties, and team settings
- [ ] Stitch designs exist for all 4 wizard steps and the completion screen; implementation matches pixel-for-pixel

### Implementation Steps

1. Create `OnboardingStatus` entity in `Domain/Onboarding/`
2. Create EF Core configuration and migration for `onboarding_status` table
3. Create `IOnboardingRepository` interface in Application layer
4. Create repository implementation in Infrastructure layer
5. Create MediatR queries and commands: `GetOnboardingStatusQuery`, `CompleteOnboardingStepCommand`
6. Create `OnboardingController` with status and complete-step endpoints
7. Add middleware or page-level check: if user has agency with incomplete onboarding, redirect to `/onboarding`
8. Create Stitch designs for: onboarding step 1 (agency), step 2 (branch), step 3 (invite), step 4 (property), completion screen
9. Download Stitch HTML to `.stitch-html/onboarding-step-*.html` and `.stitch-html/onboarding-complete.html`
10. Create `OnboardingWizard` page component with step routing
11. Create `OnboardingStepIndicator` component (progress bar with step labels)
12. Create `CreateAgencyStep` component (reuses agency creation form fields)
13. Create `CreateBranchStep` component (reuses branch creation form fields)
14. Create `InviteAgentsStep` component (email list input with role selector, skip button)
15. Create `CreatePropertyStep` component (simplified property form, skip button)
16. Create `OnboardingCompleteStep` component (celebration screen with navigation links)
17. Add `useOnboardingStatus` hook for fetching and updating onboarding state
18. Add i18n keys for onboarding wizard labels and instructions
19. Write unit tests for command/query handlers
20. Write integration tests for onboarding endpoints
21. Write frontend component tests for each wizard step

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Onboarding/OnboardingStatus.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Interfaces/IOnboardingRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Dtos/OnboardingStatusDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Queries/GetOnboardingStatus/GetOnboardingStatusQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Queries/GetOnboardingStatus/GetOnboardingStatusQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Commands/CompleteOnboardingStep/CompleteOnboardingStepCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Commands/CompleteOnboardingStep/CompleteOnboardingStepCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Onboarding/Commands/CompleteOnboardingStep/CompleteOnboardingStepCommandValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/OnboardingStatusConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/OnboardingRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/OnboardingController.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Onboarding/GetOnboardingStatusQueryHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Application.Tests/Onboarding/CompleteOnboardingStepCommandHandlerTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Api.Tests/Controllers/OnboardingControllerTests.cs`
- `apps/web/src/app/[locale]/onboarding/page.tsx`
- `apps/web/src/app/[locale]/onboarding/layout.tsx`
- `apps/web/src/components/onboarding/OnboardingWizard.tsx`
- `apps/web/src/components/onboarding/OnboardingStepIndicator.tsx`
- `apps/web/src/components/onboarding/CreateAgencyStep.tsx`
- `apps/web/src/components/onboarding/CreateBranchStep.tsx`
- `apps/web/src/components/onboarding/InviteAgentsStep.tsx`
- `apps/web/src/components/onboarding/CreatePropertyStep.tsx`
- `apps/web/src/components/onboarding/OnboardingCompleteStep.tsx`
- `apps/web/src/hooks/useOnboardingStatus.ts`
- `apps/web/src/components/onboarding/__tests__/OnboardingWizard.test.tsx`
- `apps/web/src/components/onboarding/__tests__/OnboardingStepIndicator.test.tsx`
- `apps/web/src/components/onboarding/__tests__/CreateAgencyStep.test.tsx`
- `apps/web/src/components/onboarding/__tests__/CreateBranchStep.test.tsx`
- `apps/web/src/components/onboarding/__tests__/InviteAgentsStep.test.tsx`
- `apps/web/src/components/onboarding/__tests__/CreatePropertyStep.test.tsx`
- `apps/web/src/components/onboarding/__tests__/OnboardingCompleteStep.test.tsx`
- `.stitch-html/onboarding-step-1-agency.html`
- `.stitch-html/onboarding-step-2-branch.html`
- `.stitch-html/onboarding-step-3-invite.html`
- `.stitch-html/onboarding-step-4-property.html`
- `.stitch-html/onboarding-complete.html`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<OnboardingStatus>`)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (register onboarding repository)
- `apps/web/src/middleware.ts` (add onboarding redirect logic for incomplete onboarding)
- `apps/web/src/messages/en.json` (add onboarding i18n keys)
- `apps/web/src/messages/es.json` (add onboarding i18n keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `GetOnboardingStatusQueryHandler` returns correct status for agency | xUnit + FluentAssertions, mock repository |
| Unit | `CompleteOnboardingStepCommandHandler` updates the correct step flag | xUnit, mock repository |
| Unit | `CompleteOnboardingStepCommandValidator` rejects invalid step names | xUnit |
| Unit | Onboarding status is created on first access if none exists | xUnit, mock repository |
| Integration | `GET /api/onboarding/status` returns onboarding state | `WebApplicationFactory` |
| Integration | `POST /api/onboarding/complete-step` updates status | `WebApplicationFactory` |
| Integration | Non-owner users cannot access onboarding endpoints (403) | `WebApplicationFactory` |
| Unit | `OnboardingWizard` renders correct step based on status | Vitest + RTL |
| Unit | `OnboardingStepIndicator` highlights current step, marks completed steps | Vitest + RTL |
| Unit | Each step component validates inputs and calls API on submit | Vitest + RTL, mock fetch |
| Unit | Skip button on steps 3 and 4 advances without API call | Vitest + RTL |
| Unit | `OnboardingCompleteStep` renders links to dashboard, properties, and team settings | Vitest + RTL |
| Manual | Full wizard flow from first login to completion | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- Onboarding endpoints restricted to agency owners only; agents and viewers cannot modify onboarding status
- Onboarding status is scoped to `AgencyId`; no cross-agency access
- Invitation emails (step 3) must not include sensitive agency data beyond the agency name and inviter name
- Onboarding redirect must not create an infinite loop; unauthenticated users go to login, not onboarding
- Simplified property creation (step 4) still enforces all domain validation rules

### TDD Reminder

Write handler tests first: given an agency with partially completed onboarding, assert the correct status is returned and the correct step can be completed. For frontend, write wizard step-rendering tests and navigation tests (next step, skip, back) before implementing the components.

---

## Task 7.4 -- Performance & Security Audit

**Status:** PENDING
**Dependencies:** All P0--P6 tasks (the complete application must be feature-complete before audit)

### Goal

Conduct a comprehensive performance, security, and accessibility audit of the entire Propely platform to establish production-readiness baselines. This includes load testing all API services with k6, reviewing the application against the OWASP Top 10, and verifying WCAG 2.1 AA accessibility compliance across all frontend pages. All findings are documented with remediation actions tracked to completion.

### Scope

**In scope:**
- **Load testing (k6):**
  - Write k6 scripts for critical API paths: authentication, property CRUD, property listing with filters, lead creation, appointment CRUD, dashboard aggregation
  - Establish performance baselines: P50, P95, P99 latency; throughput (requests/second); error rate
  - Test with simulated multi-tenant load (50 concurrent users across 10 tenants)
  - Identify bottlenecks: slow queries (enable EF Core query logging), N+1 problems, missing indexes, inefficient aggregation
  - Document results and remediate critical issues (P95 latency > 500ms for CRUD, > 1s for aggregation)
- **OWASP Top 10 review:**
  - A01: Broken Access Control -- verify tenant isolation, role-based authorization, permission enforcement across all endpoints
  - A02: Cryptographic Failures -- verify token encryption, password hashing, HTTPS enforcement, no secrets in code
  - A03: Injection -- verify parameterized queries (EF Core), input validation (FluentValidation), no raw SQL
  - A04: Insecure Design -- review API rate limiting, account lockout, CORS policy
  - A05: Security Misconfiguration -- review HTTP headers (HSTS, CSP, X-Frame-Options), error handling (no stack traces in production)
  - A06: Vulnerable Components -- run `dotnet list package --vulnerable`, `npm audit`
  - A07: Authentication Failures -- review JWT configuration, token expiry, refresh token rotation
  - A08: Data Integrity Failures -- review deserialization safety, CI/CD pipeline integrity
  - A09: Logging Failures -- verify security events are logged (failed auth, authorization violations, data access)
  - A10: SSRF -- verify outgoing HTTP calls (SDK clients, calendar sync) do not allow user-controlled URLs
- **Accessibility (WCAG 2.1 AA):**
  - Automated scan of all pages with axe-core
  - Manual keyboard navigation test for all interactive flows
  - Screen reader testing for critical flows (login, property creation, lead management)
  - Color contrast verification against WCAG AA ratios (4.5:1 normal text, 3:1 large text)
  - Focus management: visible focus indicators, logical tab order, no focus traps
  - ARIA labels on all interactive elements, form inputs, and status indicators
- Remediation: fix all critical and high-severity findings; document accepted risks for medium/low
- Final audit report documenting all findings, remediations, and accepted risks

**Out of scope:**
- Penetration testing by external security firm (future engagement)
- SOC 2 or ISO 27001 compliance documentation
- Mobile app performance testing (web only)
- Stress testing (finding breaking point) -- load testing focuses on expected production load
- Performance optimization of third-party services (Google Calendar API, OpenAI API)

### Acceptance Criteria

- [ ] k6 load test scripts exist for: authentication, property CRUD, property list, lead creation, appointment CRUD, and dashboard endpoints
- [ ] Load test results documented with P50/P95/P99 latencies; all critical paths meet SLA (CRUD P95 < 500ms, aggregation P95 < 1s, error rate < 1%)
- [ ] All identified performance bottlenecks are remediated (missing indexes added, N+1 queries fixed, slow queries optimized)
- [ ] OWASP Top 10 checklist completed for all 6 backend services; all critical findings remediated
- [ ] Tenant isolation verified: no endpoint returns data from a different tenant (tested with cross-tenant request scripts)
- [ ] `dotnet list package --vulnerable` returns zero critical/high vulnerabilities; `npm audit` returns zero critical/high
- [ ] axe-core automated scan passes with zero critical/serious violations on all pages
- [ ] Keyboard navigation works for all interactive flows: login, property CRUD, lead pipeline, calendar, onboarding wizard
- [ ] Security headers configured: `Strict-Transport-Security`, `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`
- [ ] Final audit report (`docs/audits/p7-launch-readiness-audit.md`) documents all findings, remediations, and accepted risks

### Implementation Steps

1. **Load testing setup:**
   a. Install k6 and create `tests/load/` directory
   b. Create `tests/load/config.js` with base configuration (VUs, duration, thresholds)
   c. Create `tests/load/helpers/auth.js` helper for obtaining JWT tokens
   d. Create `tests/load/helpers/data-generators.js` for generating test data
2. **Write k6 scripts:**
   a. `tests/load/scenarios/auth-flow.js` -- login, token refresh
   b. `tests/load/scenarios/property-crud.js` -- create, read, update, list with filters
   c. `tests/load/scenarios/lead-creation.js` -- create leads for properties
   d. `tests/load/scenarios/appointment-crud.js` -- create, list, status changes
   e. `tests/load/scenarios/dashboard.js` -- agent and admin dashboard endpoints
   f. `tests/load/run-all.js` -- orchestrates all scenarios with 50 concurrent VUs across 10 tenants
3. **Execute load tests** against local Docker Compose stack, capture results
4. **Analyze results:** identify endpoints exceeding SLA thresholds
5. **Remediate performance issues:**
   a. Enable EF Core query logging, identify slow queries
   b. Add missing database indexes
   c. Fix N+1 queries with `.Include()` or projection
   d. Optimize dashboard aggregation queries
   e. Re-run load tests to verify improvements
6. **OWASP review:**
   a. Create `docs/audits/owasp-checklist.md` with checklist for each A01--A10 item
   b. Review each service systematically against the checklist
   c. Test tenant isolation: create script that attempts cross-tenant API access
   d. Run `dotnet list package --vulnerable` on all solutions
   e. Run `npm audit` on web frontend
   f. Review HTTP security headers in API responses
   g. Fix identified vulnerabilities (update packages, add headers, fix authorization gaps)
7. **Accessibility audit:**
   a. Install `@axe-core/cli` or integrate axe-core into Playwright/Cypress
   b. Create accessibility test script that crawls all authenticated pages
   c. Run automated scan, capture results
   d. Manual keyboard navigation test: tab through all forms, modals, dropdowns
   e. Screen reader test: use NVDA or VoiceOver for login, property creation, lead management
   f. Fix violations: add ARIA labels, fix contrast ratios, add focus indicators, fix tab order
   g. Re-run automated scan to verify fixes
8. **Security headers:**
   a. Add security headers middleware to all .NET services
   b. Add `Content-Security-Policy` header to Next.js (`next.config.js`)
   c. Verify headers with `securityheaders.com` or `curl -I`
9. **Document results:**
   a. Create `docs/audits/p7-launch-readiness-audit.md` with sections: Executive Summary, Load Test Results, OWASP Findings, Accessibility Findings, Accepted Risks, Remediation Log
10. **Final verification:** re-run all load tests, OWASP checks, and accessibility scans after remediation

### Files to Create/Modify

**Create:**
- `tests/load/config.js`
- `tests/load/helpers/auth.js`
- `tests/load/helpers/data-generators.js`
- `tests/load/scenarios/auth-flow.js`
- `tests/load/scenarios/property-crud.js`
- `tests/load/scenarios/lead-creation.js`
- `tests/load/scenarios/appointment-crud.js`
- `tests/load/scenarios/dashboard.js`
- `tests/load/run-all.js`
- `docs/audits/owasp-checklist.md`
- `docs/audits/p7-launch-readiness-audit.md`
- `tests/accessibility/axe-scan.ts` (or `.js`)
- `tests/accessibility/keyboard-navigation.md` (manual test script)

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Api/Program.cs` (add security headers middleware)
- `services/ai-api/src/Propely.AiApi.Api/Program.cs` (add security headers middleware)
- `services/properties-api/src/Propely.PropertiesApi.Api/Program.cs` (add security headers middleware)
- `services/contacts-api/src/Propely.ContactsApi.Api/Program.cs` (add security headers middleware)
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Program.cs` (add security headers middleware)
- `services/publishing-api/src/Propely.PublishingApi.Api/Program.cs` (add security headers middleware)
- `apps/web/next.config.js` (add security headers)
- Various `.csproj` files (update vulnerable packages)
- `apps/web/package.json` (update vulnerable packages)
- Various `.cs` and `.tsx` files (fix N+1 queries, add ARIA labels, fix accessibility issues)
- EF Core migration files (add missing indexes identified during load testing)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Load | Authentication flow under 50 concurrent users | k6 with JWT token generation |
| Load | Property CRUD P95 latency < 500ms | k6, 50 VUs, 5-minute sustained load |
| Load | Property list with filters P95 latency < 500ms | k6, paginated queries with varied filters |
| Load | Dashboard aggregation P95 latency < 1s | k6, agent + admin endpoints |
| Security | Tenant isolation: cross-tenant requests return 403 or empty | k6 script with mismatched tenant headers |
| Security | SQL injection: parameterized queries only (EF Core audit) | Code review + automated check |
| Security | Vulnerable packages: zero critical/high | `dotnet list package --vulnerable`, `npm audit` |
| Security | Security headers present on all responses | `curl -I` against each service |
| Accessibility | axe-core scan: zero critical/serious violations | axe-core CLI or Playwright integration |
| Accessibility | Keyboard navigation: all flows completable via keyboard | Manual testing with checklist |
| Accessibility | Color contrast: all text meets WCAG AA ratios | axe-core + manual spot-check |
| Accessibility | Screen reader: critical flows narrated correctly | NVDA/VoiceOver manual testing |

### Security & Privacy

- Load test data must use synthetic/generated data, not real customer data
- k6 scripts must not contain hardcoded credentials; use environment variables for auth
- OWASP audit findings classified as Critical/High must be remediated before launch; no exceptions
- Vulnerability scan results (`npm audit`, `dotnet list package --vulnerable`) must not be committed to the repo if they contain exploitable details; only the summary report is committed
- Accessibility audit documents may reference page URLs but must not include screenshots containing PII test data

### TDD Reminder

For performance remediation: write a failing performance assertion (e.g., query takes > 500ms), then optimize (add index, fix N+1) until the assertion passes. For security fixes: write an integration test that attempts the attack vector (e.g., cross-tenant access) and asserts rejection. For accessibility: add axe-core assertions to existing component tests where violations are found.

---

## Completion Criteria

Phase 7 is complete when:

1. Agent and admin dashboards display accurate, aggregated KPIs from properties, contacts/leads, and appointments
2. In-app and email notifications fire for all key events (new lead, appointment reminder, property status change, permission change) and users can configure preferences
3. New agency owners complete the onboarding wizard successfully (agency -> branch -> invite -> property)
4. All API endpoints meet P95 latency SLAs under simulated production load (50 concurrent users)
5. OWASP Top 10 review is complete with zero critical/high findings unresolved
6. WCAG 2.1 AA accessibility scan passes with zero critical/serious violations
7. Security headers are configured on all services
8. Zero critical/high vulnerable dependencies in both .NET and npm packages
9. Final audit report is published at `docs/audits/p7-launch-readiness-audit.md`
10. All tests pass across all services and the frontend
