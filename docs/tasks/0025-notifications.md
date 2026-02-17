# Task: 0025 - In-App Notification System

## Metadata
- ID: 0025
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0025-notifications.md`

## Goal
Implement an in-app notification system. Backend stores notifications triggered by actions (invitations, role changes). Frontend shows a notification bell in the header with unread count and a dropdown listing recent notifications.

## Context
Users currently have no way to know about events that happened when they weren't on the page (e.g., being invited to an org, role changed). In-app notifications solve this and serve as a pattern for future notification types.

## Scope
### In scope
- Backend: `Notification` entity (Id, UserId, Type, Title, Body, IsRead, CreatedAtUtc, Metadata JSON)
- Backend: `GET /notifications` - list notifications (paginated)
- Backend: `PATCH /notifications/{id}/read` - mark as read
- Backend: `PATCH /notifications/read-all` - mark all as read
- Backend: Trigger notifications from: AcceptInvitation (notify org admins), UpdateMemberRole (notify affected user)
- Frontend: NotificationBell component in AppHeader
- Frontend: NotificationDropdown with list of notifications
- Frontend: Unread count badge
- Frontend: Mark as read on click
- Frontend: i18n for notification messages
- EF Core migration

### Out of scope
- Real-time notifications (WebSocket/SSE) - polling is sufficient for now
- Push notifications (browser/mobile)
- Email notification preferences
- Notification settings page

## Requirements
- R1: Notifications are created server-side when relevant events occur
- R2: Each notification has a type that determines how the frontend renders it
- R3: Unread count shows in header bell icon
- R4: Clicking a notification marks it as read
- R5: "Mark all as read" button available
- R6: Notifications list is paginated (newest first)
- R7: Frontend polls for new notifications every 30 seconds

## Acceptance Criteria
- AC1: Accepting an invitation creates a notification for org admins
- AC2: Changing a member's role creates a notification for that user
- AC3: `GET /notifications` returns paginated list
- AC4: NotificationBell shows unread count
- AC5: Clicking notification marks it as read and decrements count
- AC6: All notification text is i18n
- AC7: `dotnet build` + `dotnet test` pass
- AC8: `npm run build` + `npm run lint` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
### Backend
1. Create Notification entity in Domain
2. Create INotificationRepository in Application
3. Create NotificationRepository in Infrastructure
4. Create GetNotifications query + handler
5. Create MarkNotificationRead command + handler
6. Create MarkAllNotificationsRead command + handler
7. Trigger notifications from AcceptInvitationHandler and UpdateMemberRoleHandler
8. Add endpoints to a new NotificationsController
9. EF migration

### Frontend
10. Create useNotifications hook (fetch + polling)
11. Create NotificationBell component
12. Create NotificationDropdown component
13. Add to AppHeader
14. Add i18n strings

## Files to Create / Modify
### Backend
- `Domain/Notifications/Notification.cs` (create)
- `Application/Notifications/` (create - queries, commands, interfaces)
- `Infrastructure/Persistence/Configurations/NotificationConfiguration.cs` (create)
- `Infrastructure/Persistence/Repositories/NotificationRepository.cs` (create)
- `Api/Controllers/NotificationsController.cs` (create)
- Existing handlers (modify - trigger notifications)

### Frontend
- `apps/web/src/hooks/notifications.ts` (create)
- `apps/web/src/components/layout/NotificationBell.tsx` (create)
- `apps/web/src/components/layout/NotificationDropdown.tsx` (create)
- `apps/web/src/components/layout/AppHeader.tsx` (modify)
- `apps/web/messages/en.json` (modify)
- `apps/web/messages/es.json` (modify)

## Testing Plan
- Unit tests: Notification handlers
- Integration tests: Notification API endpoints
- Frontend tests: NotificationBell and dropdown components

## Security & Privacy
- Users can only see their own notifications
- Authorization enforced on all endpoints

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
