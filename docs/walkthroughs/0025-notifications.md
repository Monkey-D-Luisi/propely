# Walkthrough: 0025-notifications

## Task Reference
- Task: `docs/tasks/0025-notifications.md`
- Walkthrough: `docs/walkthroughs/0025-notifications.md`
- Branch/PR: `feat/0025-notifications`
- Date: `2026-02-07`

## Summary
Implemented an in-app notification system with backend storage and frontend polling. Notifications are triggered by accept-invitation (notifies org admins/owners) and update-member-role (notifies affected user) events. The frontend displays a bell icon with unread count badge and a dropdown listing recent notifications with mark-as-read functionality.

## Context
- Background: Users had no awareness of events that occurred while offline (e.g., someone joining their org, their role changing). In-app notifications provide this visibility and establish a pattern for future notification types.
- Problem statement: No notification mechanism existed in the application.
- Constraints: Clean Architecture layers respected. Polling-based (30s interval) rather than WebSocket. English-only repo content. All i18n strings provided in EN and ES.

## Decisions & Trade-offs
- **Polling over WebSocket:** Task scope explicitly excluded real-time notifications. A 30-second polling interval was chosen as a simple, reliable approach that avoids WebSocket infrastructure complexity. The hook uses `setInterval` with cleanup on unmount.

- **Notifications triggered inline in command handlers:** Rather than using domain events or a separate service bus, notifications are created directly in `AcceptInvitationCommandHandler` and `UpdateMemberRoleCommandHandler`. This keeps the implementation simple and avoids introducing new infrastructure. The notification is persisted in the same `SaveChangesAsync` call as the original operation, ensuring transactional consistency.

- **Bulk mark-all-as-read with ExecuteUpdateAsync:** Instead of loading all notifications into memory and marking each one, `MarkAllAsReadAsync` uses EF Core's `ExecuteUpdateAsync` for a single SQL `UPDATE` statement. This is more efficient for users with many notifications.

- **Separate CreateNotificationCommand exists but not used by triggers:** A `CreateNotificationCommand` + handler was created for potential future use (e.g., admin-triggered notifications), but the current triggers create `Notification` entities directly for transactional consistency.

- **NotificationBell rendered only for authenticated users:** The `NotificationBell` component is placed inside the `user ?` conditional in `AppHeader`, so the hook only starts polling when the user is logged in.

## Changes

### Backend - Domain Layer
| File | Action | Purpose |
|------|--------|---------|
| `Domain/Notifications/Notification.cs` | Created | Entity with Create factory and MarkAsRead method |
| `Domain/Notifications/NotificationType.cs` | Created | Enum: InvitationAccepted, RoleChanged |

### Backend - Application Layer
| File | Action | Purpose |
|------|--------|---------|
| `Application/Notifications/Interfaces/INotificationRepository.cs` | Created | Repository interface with Add, AddRange, GetById, GetByUserIdPaged, GetUnreadCount, MarkAllAsRead |
| `Application/Notifications/Queries/GetNotifications/` | Created | Query + handler returning paginated DTOs + unread count |
| `Application/Notifications/Commands/MarkNotificationRead/` | Created | Command + handler with ownership validation |
| `Application/Notifications/Commands/MarkAllNotificationsRead/` | Created | Command + handler delegating to repo bulk update |
| `Application/Notifications/Commands/CreateNotification/` | Created | Command + handler for programmatic notification creation |
| `Application/Organizations/Commands/AcceptInvitation/AcceptInvitationCommandHandler.cs` | Modified | Added notification trigger for org admins/owners |
| `Application/Organizations/Commands/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs` | Modified | Added notification trigger for affected user on role change |

### Backend - Infrastructure Layer
| File | Action | Purpose |
|------|--------|---------|
| `Infrastructure/Persistence/Configurations/NotificationConfiguration.cs` | Created | EF Core config: snake_case columns, indexes on user_id, (user_id, is_read), created_at_utc |
| `Infrastructure/Persistence/Repositories/NotificationRepository.cs` | Created | Repository with ExecuteUpdateAsync for bulk mark-read |
| `Infrastructure/Persistence/AppDbContext.cs` | Modified | Added Notifications DbSet and configuration |
| `Infrastructure/DependencyInjection.cs` | Modified | Registered INotificationRepository |
| `Infrastructure/Persistence/Migrations/AddNotifications` | Created | EF Core migration |

### Backend - API Layer
| File | Action | Purpose |
|------|--------|---------|
| `Api/Controllers/NotificationsController.cs` | Created | GET /notifications, PATCH /{id}/read, PATCH /read-all |

### Frontend
| File | Action | Purpose |
|------|--------|---------|
| `hooks/notifications.ts` | Created | useNotifications hook with 30s polling, markAsRead, markAllAsRead |
| `components/layout/NotificationBell.tsx` | Created | Bell icon button with unread count badge, dropdown toggle |
| `components/layout/NotificationDropdown.tsx` | Created | Dropdown list with notification items, mark-read, empty state |
| `components/layout/AppHeader.tsx` | Modified | Added NotificationBell to authenticated user section |
| `lib/schemas.ts` | Modified | Added NotificationSchema, NotificationsResponseSchema |
| `messages/en.json` | Modified | Added notifications namespace |
| `messages/es.json` | Modified | Added notifications namespace (Spanish) |

### Tests
| File | Action | Purpose |
|------|--------|---------|
| `UnitTests/.../MarkNotificationReadCommandHandlerTests.cs` | Created | 4 tests: valid, not found, wrong user, already read |
| `UnitTests/.../MarkAllNotificationsReadCommandHandlerTests.cs` | Created | 1 test: calls mark-all + save |
| `UnitTests/.../GetNotificationsQueryHandlerTests.cs` | Created | 3 tests: returns data, clamps params, maps DTOs |
| `UnitTests/.../AcceptInvitationCommandHandlerTests.cs` | Modified | Added GetByOrgIdAsync mock for notification trigger |
| `UnitTests/.../UpdateMemberRoleCommandHandlerTests.cs` | Modified | Added new dependencies to constructor |
| `IntegrationTests/Api/NotificationEndpointTests.cs` | Created | 8 tests: CRUD, auth, pagination, role-change trigger |
| `__tests__/NotificationBell.test.tsx` | Created | 7 tests: render, badge, toggle, dropdown, 99+ cap |
| `__tests__/NotificationDropdown.test.tsx` | Created | 8 tests: empty state, render, mark-read, mark-all |
| `__tests__/AppHeader.test.tsx` | Modified | Added useNotifications mock |

## Verification
- `dotnet build`: 0 errors
- `dotnet test`: 204 passing (120 unit + 79 integration + 5 architecture)
- `npm run build`: success
- `npx vitest run`: 221 passing (19 test files)
