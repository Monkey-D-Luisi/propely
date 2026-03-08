# Epic P5 -- Appointments & Calendar

## Overview

Enable real estate agents to schedule and manage appointments (property viewings, owner meetings, generic events) linked to properties and contacts. Includes bidirectional calendar synchronization with Google Calendar and Microsoft Outlook, a background sync engine with conflict resolution, and a full frontend calendar experience with drag-and-drop rescheduling and booking flows.

## Service Ownership

| Capability | Service |
|---|---|
| Appointment domain, persistence, API | `services/appointments-api` |
| Calendar sync integrations | `services/appointments-api` (Infrastructure layer) |
| Background sync worker | `services/appointments-api` (hosted service) |
| Frontend calendar & booking UI | `apps/web` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 5.1 | Appointment Domain Model | DONE | -- |
| 5.2 | Appointments Persistence & API | DONE | 5.1 |
| 5.3 | Google Calendar Integration | DONE | 5.2 |
| 5.4 | Outlook/Microsoft Graph Integration | DONE | 5.2 |
| 5.5 | Calendar Sync Engine | DONE | 5.3, 5.4 |
| 5.6 | Frontend Calendar View | DONE | 5.2 |
| 5.7 | Frontend Appointment Booking | DONE | 5.2, 5.6 |
| 5.8 | Appointments-API NuGet SDK Client | DONE | 5.2 |

---

## Task 5.1 -- Appointment Domain Model

**Status:** DONE
**Dependencies:** None

### Scope

**In scope:**
- `Appointment` aggregate root entity in `AppointmentsApi.Domain`
- Appointment types: `PropertyViewing`, `OwnerMeeting`, `Generic`
- Status: `Scheduled`, `Confirmed`, `Completed`, `Cancelled`, `NoShow`
- Links: `PropertyId` (optional for Generic), `ContactId` (optional), `AgentId` (required)
- Fields: title, description, start time (UTC), end time (UTC), location, type, status, all-day flag
- Recurrence: out of scope for initial version; single occurrences only
- Domain events: `AppointmentCreatedV1`, `AppointmentUpdatedV1`, `AppointmentCancelledV1`, `AppointmentCompletedV1`
- Business rules: end time must be after start time, cannot complete a cancelled appointment, cancellation requires reason
- `CalendarSync` value object: external calendar ID, provider, last synced timestamp (stored on appointment)
- Soft-delete support via `ISoftDeletable`

**Out of scope:**
- Persistence/EF Core (task 5.2)
- Calendar integration (tasks 5.3, 5.4)
- Recurring appointments

### Acceptance Criteria

- [ ] `Appointment` entity inherits from `Entity` and implements `ISoftDeletable`
- [ ] `Appointment` has required fields: `Title`, `StartTimeUtc`, `EndTimeUtc`, `AgentId`, `TenantId`, `Type`
- [ ] `Appointment` has optional fields: `Description`, `Location`, `PropertyId`, `ContactId`, `IsAllDay`, `CancellationReason`
- [ ] `AppointmentType` enum: `PropertyViewing`, `OwnerMeeting`, `Generic`
- [ ] `AppointmentStatus` enum: `Scheduled`, `Confirmed`, `Completed`, `Cancelled`, `NoShow`
- [ ] `Appointment.Create()` factory validates required fields, sets status to `Scheduled`, raises `AppointmentCreatedV1`
- [ ] `Appointment.Update()` validates fields and raises `AppointmentUpdatedV1`
- [ ] `Appointment.Confirm()` changes status to `Confirmed` (only from `Scheduled`)
- [ ] `Appointment.Complete()` changes status to `Completed` (only from `Scheduled` or `Confirmed`)
- [ ] `Appointment.Cancel(reason)` changes status to `Cancelled`, sets `CancellationReason`, raises `AppointmentCancelledV1`
- [ ] `Appointment.MarkNoShow()` changes status to `NoShow` (only from `Scheduled` or `Confirmed`)
- [ ] End time must be after start time; violation throws `DomainException`
- [ ] For `PropertyViewing` type, `PropertyId` is required; violation throws `DomainException`
- [ ] `CalendarSyncInfo` value object: `ExternalEventId`, `Provider` (enum: Google, Microsoft), `LastSyncedUtc`
- [ ] `Appointment.SetCalendarSync(CalendarSyncInfo)` stores sync metadata
- [ ] All domain events include `AppointmentId`, `AgentId`, `TenantId`, `OccurredAt`
- [ ] Unit tests cover all factory methods, status transitions, and validation rules

### Implementation Steps

1. Create `AppointmentType` enum in `Domain/Appointments/`
2. Create `AppointmentStatus` enum in `Domain/Appointments/`
3. Create `CalendarProvider` enum: `Google`, `Microsoft`
4. Create `CalendarSyncInfo` value object: `ExternalEventId`, `Provider`, `LastSyncedUtc`
5. Create `Appointment` aggregate root with private constructor, factory methods, and status transition logic
6. Implement status transition validation (state machine)
7. Create domain events in `Domain/Appointments/Events/`
8. Create `AppointmentValidationException` in `Domain/Appointments/Exceptions/`
9. Write comprehensive unit tests for all domain logic

### Files to Create/Modify

**Create:**
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Appointment.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/AppointmentType.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/AppointmentStatus.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarProvider.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarSyncInfo.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Events/AppointmentCreatedV1.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Events/AppointmentUpdatedV1.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Events/AppointmentCancelledV1.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Events/AppointmentCompletedV1.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Exceptions/AppointmentValidationException.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Domain.Tests/Appointments/AppointmentTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Domain.Tests/Appointments/AppointmentStatusTransitionTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Appointment.Create()` with valid data raises event, sets status Scheduled | xUnit + FluentAssertions |
| Unit | `Appointment.Create()` with end before start throws | xUnit |
| Unit | `Appointment.Create()` PropertyViewing without PropertyId throws | xUnit |
| Unit | `Appointment.Confirm()` from Scheduled succeeds | xUnit |
| Unit | `Appointment.Confirm()` from Cancelled throws | xUnit |
| Unit | `Appointment.Complete()` from Confirmed succeeds | xUnit |
| Unit | `Appointment.Complete()` from Cancelled throws | xUnit |
| Unit | `Appointment.Cancel(reason)` sets reason, raises event | xUnit |
| Unit | `Appointment.Cancel()` without reason throws | xUnit |
| Unit | `Appointment.MarkNoShow()` from Scheduled succeeds | xUnit |
| Unit | `Appointment.MarkNoShow()` from Completed throws | xUnit |
| Unit | All valid/invalid status transitions (parameterized matrix) | xUnit `[Theory]` |
| Unit | `CalendarSyncInfo` stores correct provider and external ID | xUnit |

### Security & Privacy

- Appointment data may contain location information; tenant-scoped via `TenantId`
- Contact-linked appointments reference PII indirectly; no PII stored on the appointment itself
- Calendar sync metadata (external IDs) is not PII but should be tenant-scoped

### TDD Reminder

Write parameterized status transition tests for all combinations (5x5 matrix) first. Define valid and invalid transitions. Then implement the state machine in the `Appointment` aggregate.

---

## Task 5.2 -- Appointments Persistence & API

**Status:** PENDING
**Dependencies:** 5.1 (domain model must exist)

### Scope

**In scope:**
- EF Core configuration for `Appointment` entity
- Database migration for appointments table
- Repository interfaces and implementations
- CRUD API endpoints: `GET /api/appointments`, `GET /api/appointments/{id}`, `POST /api/appointments`, `PUT /api/appointments/{id}`, `DELETE /api/appointments/{id}`
- Status change endpoints: `PUT /api/appointments/{id}/confirm`, `PUT /api/appointments/{id}/complete`, `PUT /api/appointments/{id}/cancel`, `PUT /api/appointments/{id}/no-show`
- List filtering: by agent, date range, property, contact, type, status
- Tenant query filter
- Authorization: agent sees own appointments, admin sees all within tenant

**Out of scope:**
- Calendar sync (tasks 5.3, 5.4, 5.5)
- Frontend (tasks 5.6, 5.7)
- Availability checking / conflict detection (future enhancement)

### Acceptance Criteria

- [ ] `AppointmentConfiguration` maps `Appointment` to `appointments` table with correct columns, indexes
- [ ] `CalendarSyncInfo` is mapped as an owned entity
- [ ] Index on `AgentId` + `StartTimeUtc` for efficient agent schedule queries
- [ ] Index on `PropertyId` for property-level appointment listing
- [ ] Index on `ContactId` for contact-level appointment listing
- [ ] `IAppointmentRepository` and `IAppointmentReadRepository` interfaces in Application layer
- [ ] Repository implementations with tenant filtering
- [ ] `GET /api/appointments` supports: `?page=1&pageSize=20&agentId=guid&from=date&to=date&propertyId=guid&contactId=guid&type=PropertyViewing&status=Scheduled&sortBy=startTimeUtc`
- [ ] `POST /api/appointments` creates appointment, returns 201 with location header
- [ ] `PUT /api/appointments/{id}` updates mutable fields (title, description, times, location)
- [ ] `DELETE /api/appointments/{id}` soft-deletes
- [ ] Status change endpoints validate transitions and return updated appointment
- [ ] `PUT /api/appointments/{id}/cancel` requires `{ "reason": "..." }` in body
- [ ] Authorization: agents see/modify own appointments only; admins see all in tenant
- [ ] All endpoints require authentication
- [ ] Migration runs cleanly
- [ ] Integration tests cover all CRUD and status operations

### Implementation Steps

1. Create `IAppointmentRepository` interface in `Application/Appointments/Interfaces/`
2. Create `IAppointmentReadRepository` interface
3. Create DTOs: `AppointmentDto`, `AppointmentListItemDto`, `CreateAppointmentRequest`, `UpdateAppointmentRequest`, `CancelAppointmentRequest`
4. Create MediatR commands: `CreateAppointmentCommand`, `UpdateAppointmentCommand`, `DeleteAppointmentCommand`, `ConfirmAppointmentCommand`, `CompleteAppointmentCommand`, `CancelAppointmentCommand`, `MarkNoShowCommand`
5. Create MediatR queries: `GetAppointmentByIdQuery`, `ListAppointmentsQuery`
6. Create command/query handlers with validators
7. Create `AppointmentConfiguration` EF Core configuration
8. Create repository implementations
9. Add database migration
10. Create `AppointmentsController` in API layer
11. Register new services in DI
12. Write unit tests for handlers and validators
13. Write integration tests for all endpoints

### Files to Create/Modify

**Create:**
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Interfaces/IAppointmentRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Interfaces/IAppointmentReadRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Dtos/AppointmentDto.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Dtos/AppointmentListItemDto.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Dtos/CreateAppointmentRequest.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Dtos/UpdateAppointmentRequest.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Dtos/CancelAppointmentRequest.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CreateAppointment/CreateAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CreateAppointment/CreateAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CreateAppointment/CreateAppointmentCommandValidator.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/UpdateAppointment/UpdateAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/UpdateAppointment/UpdateAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/DeleteAppointment/DeleteAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/DeleteAppointment/DeleteAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/ConfirmAppointment/ConfirmAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/ConfirmAppointment/ConfirmAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CompleteAppointment/CompleteAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CompleteAppointment/CompleteAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CancelAppointment/CancelAppointmentCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/CancelAppointment/CancelAppointmentCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/MarkNoShow/MarkNoShowCommand.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Commands/MarkNoShow/MarkNoShowCommandHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Queries/GetAppointmentById/GetAppointmentByIdQuery.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Queries/GetAppointmentById/GetAppointmentByIdQueryHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Queries/ListAppointments/ListAppointmentsQuery.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Queries/ListAppointments/ListAppointmentsQueryHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Queries/ListAppointments/ListAppointmentsQueryValidator.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Configurations/AppointmentConfiguration.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Repositories/AppointmentRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Repositories/AppointmentReadRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/AppointmentsController.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Application.Tests/Appointments/Commands/CreateAppointmentCommandHandlerTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Application.Tests/Appointments/Commands/CancelAppointmentCommandHandlerTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Api.Tests/Controllers/AppointmentsControllerTests.cs`

**Modify:**
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<Appointment>`)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/DependencyInjection.cs` (register repositories)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | All command validators (required fields, date validation, enum values) | xUnit + FluentAssertions |
| Unit | All command handlers (create, update, delete, status changes) | xUnit, mock repositories |
| Unit | Query handlers with filters (date range, agent, property) | xUnit, mock read repositories |
| Integration | Appointment CRUD via HTTP endpoints | `WebApplicationFactory`, in-memory DB |
| Integration | Status change endpoints with valid/invalid transitions | `WebApplicationFactory` |
| Integration | Authorization: agent cannot see other agent's appointments | `WebApplicationFactory` |
| Integration | Tenant isolation | `WebApplicationFactory` |
| Manual | Create and manage appointments through API | Dev environment |

### Security & Privacy

- Tenant isolation via global query filter on `TenantId`
- Agent-level authorization: agents see own appointments, admins see all
- Location field may contain addresses; do not log
- All times stored in UTC; client responsible for timezone display

### TDD Reminder

Write integration tests for each endpoint with expected request/response pairs first. Then implement the full stack from controller to persistence.

---

## Task 5.3 -- Google Calendar Integration

**Status:** PENDING
**Dependencies:** 5.2 (appointment persistence and API must exist)

### Scope

**In scope:**
- Google OAuth2 flow for calendar access (read/write scope)
- `ICalendarSyncService` interface in Application layer (provider-agnostic)
- `GoogleCalendarSyncService` implementation in Infrastructure layer
- Create/update/delete events in Google Calendar when appointments change
- Push notifications (webhooks) from Google Calendar for external changes
- Store OAuth tokens securely (encrypted in database)
- Token refresh handling
- Map Propely appointment fields to Google Calendar event fields

**Out of scope:**
- Microsoft/Outlook integration (task 5.4)
- Background sync engine (task 5.5)
- Multiple calendar support per user (one calendar per agent)

### Acceptance Criteria

- [ ] `ICalendarSyncService` interface defined with: `CreateEventAsync`, `UpdateEventAsync`, `DeleteEventAsync`, `GetChangesAsync`, `SetupWebhookAsync`
- [ ] `GoogleCalendarSyncService` implements `ICalendarSyncService`
- [ ] OAuth2 flow: `GET /api/calendar/google/connect` redirects to Google consent screen
- [ ] OAuth2 callback: `GET /api/calendar/google/callback` exchanges code for tokens, stores encrypted
- [ ] `GET /api/calendar/google/status` returns connection status for current agent
- [ ] `DELETE /api/calendar/google/disconnect` revokes tokens and removes connection
- [ ] When an appointment is created/updated/deleted, corresponding Google Calendar event is created/updated/deleted
- [ ] Google webhook endpoint: `POST /api/calendar/google/webhook` receives push notifications
- [ ] Webhook processes external changes (events created/modified/deleted in Google Calendar)
- [ ] OAuth tokens are encrypted at rest using data protection API
- [ ] Token refresh is handled transparently (refresh before expiry)
- [ ] Field mapping: Title -> Summary, Description -> Description, StartTimeUtc/EndTimeUtc -> start/end, Location -> location
- [ ] AppointmentType is added to event description as metadata
- [ ] Unit tests for service with mocked Google API client
- [ ] Integration test for OAuth flow with mocked Google endpoints

### Implementation Steps

1. Define `ICalendarSyncService` interface in `Application/Appointments/Interfaces/`
2. Define `CalendarConnection` entity in Domain (agentId, provider, encrypted tokens, last synced)
3. Create `ICalendarConnectionRepository` interface
4. Install `Google.Apis.Calendar.v3` NuGet package
5. Create `GoogleCalendarSyncService` in `Infrastructure/Calendar/`
6. Implement OAuth2 flow with Google Identity
7. Create `CalendarConnectionConfiguration` EF Core configuration
8. Create `CalendarConnectionRepository` implementation
9. Implement `CreateEventAsync` mapping appointment to Google event
10. Implement `UpdateEventAsync` with change detection
11. Implement `DeleteEventAsync` (cancel vs. delete based on status)
12. Implement `GetChangesAsync` using Google's sync tokens
13. Implement `SetupWebhookAsync` using Google push notifications API
14. Create `GoogleCalendarWebhookController` for incoming webhooks
15. Create `CalendarController` for OAuth flow endpoints
16. Encrypt/decrypt OAuth tokens using `IDataProtector`
17. Add migration for `calendar_connections` table
18. Write unit tests with mocked Google API
19. Write integration tests for OAuth flow

### Files to Create/Modify

**Create:**
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Interfaces/ICalendarSyncService.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Interfaces/ICalendarConnectionRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarConnection.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/GoogleCalendarSyncService.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/GoogleCalendarMapper.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/CalendarTokenEncryption.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Configurations/CalendarConnectionConfiguration.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Repositories/CalendarConnectionRepository.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/CalendarController.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/GoogleCalendarWebhookController.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/GoogleCalendarSyncServiceTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/GoogleCalendarMapperTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Api.Tests/Controllers/CalendarControllerTests.cs`

**Modify:**
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<CalendarConnection>`)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/DependencyInjection.cs` (register calendar services)
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Propely.AppointmentsApi.Api.csproj` (add Google Calendar NuGet)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `GoogleCalendarMapper` maps appointment to Google event correctly | xUnit + FluentAssertions |
| Unit | `GoogleCalendarSyncService.CreateEventAsync` calls Google API with correct params | xUnit, mock Google client |
| Unit | Token refresh is triggered when token is near expiry | xUnit, mock token store |
| Unit | `CalendarTokenEncryption` encrypts and decrypts correctly | xUnit, `IDataProtector` |
| Integration | OAuth connect flow returns redirect URL | `WebApplicationFactory`, mock Google OAuth |
| Integration | OAuth callback stores encrypted tokens | `WebApplicationFactory` |
| Integration | Webhook endpoint processes push notification | `WebApplicationFactory` |
| Manual | Connect Google Calendar, create appointment, verify event appears | Dev environment |

### Security & Privacy

- OAuth tokens are highly sensitive; MUST be encrypted at rest using ASP.NET Data Protection
- Google API credentials (client ID, client secret) stored in environment variables only
- Webhook endpoint must validate Google's channel token to prevent spoofing
- Tokens must be scoped to minimum required permissions (`calendar.events` scope only)
- Token revocation on disconnect must call Google's revoke endpoint
- Never log OAuth tokens, even at debug level

### TDD Reminder

Write mapper tests first (appointment -> Google event mapping). Then write service tests with mocked Google client. Test token encryption/decryption. Finally, test the OAuth flow.

---

## Task 5.4 -- Outlook/Microsoft Graph Integration

**Status:** PENDING
**Dependencies:** 5.2 (appointment persistence and API must exist)

### Scope

**In scope:**
- Microsoft OAuth2 flow for calendar access via Microsoft Graph API
- `MicrosoftCalendarSyncService` implementation of `ICalendarSyncService` (same interface as Google, task 5.3)
- Create/update/delete events in Outlook Calendar when appointments change
- Microsoft Graph change notifications (webhooks) for external changes
- Store OAuth tokens securely (same encryption as Google)
- Token refresh handling
- Map Propely appointment fields to Outlook Calendar event fields

**Out of scope:**
- Google Calendar integration (task 5.3)
- Background sync engine (task 5.5)
- Shared/delegated mailbox calendars

### Acceptance Criteria

- [ ] `MicrosoftCalendarSyncService` implements `ICalendarSyncService`
- [ ] OAuth2 flow: `GET /api/calendar/microsoft/connect` redirects to Microsoft consent screen
- [ ] OAuth2 callback: `GET /api/calendar/microsoft/callback` exchanges code for tokens, stores encrypted
- [ ] `GET /api/calendar/microsoft/status` returns connection status
- [ ] `DELETE /api/calendar/microsoft/disconnect` revokes tokens
- [ ] Appointment CRUD syncs to Outlook Calendar events
- [ ] Microsoft Graph webhook: `POST /api/calendar/microsoft/webhook` receives change notifications
- [ ] Webhook validates subscription via validation token handshake
- [ ] OAuth tokens encrypted at rest
- [ ] Token refresh handled transparently
- [ ] Field mapping: Title -> Subject, Description -> Body (HTML), StartTimeUtc/EndTimeUtc -> start/end (with timezone), Location -> Location.DisplayName
- [ ] Unit tests for service with mocked Graph client
- [ ] Integration test for OAuth flow with mocked Microsoft endpoints

### Implementation Steps

1. Install `Microsoft.Graph` and `Microsoft.Identity.Web` NuGet packages
2. Create `MicrosoftCalendarSyncService` in `Infrastructure/Calendar/`
3. Create `MicrosoftCalendarMapper` for field mapping
4. Implement OAuth2 flow with Microsoft Identity platform
5. Implement `CreateEventAsync` mapping appointment to Outlook event
6. Implement `UpdateEventAsync` with change detection
7. Implement `DeleteEventAsync`
8. Implement `GetChangesAsync` using Microsoft Graph delta queries
9. Implement webhook subscription management (Graph change notifications require renewal every 3 days max)
10. Create `MicrosoftCalendarWebhookController` with validation token handshake
11. Add Microsoft connect/disconnect/status endpoints to `CalendarController`
12. Reuse `CalendarConnection` entity and repository (provider = Microsoft)
13. Write unit tests with mocked Graph client
14. Write integration tests for OAuth flow

### Files to Create/Modify

**Create:**
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/MicrosoftCalendarSyncService.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/MicrosoftCalendarMapper.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/MicrosoftCalendarWebhookController.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/MicrosoftCalendarSyncServiceTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/MicrosoftCalendarMapperTests.cs`

**Modify:**
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/CalendarController.cs` (add Microsoft endpoints)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/DependencyInjection.cs` (register Microsoft service)
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Propely.AppointmentsApi.Api.csproj` (add Microsoft Graph NuGet)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `MicrosoftCalendarMapper` maps appointment to Outlook event | xUnit + FluentAssertions |
| Unit | `MicrosoftCalendarSyncService` methods call Graph API correctly | xUnit, mock Graph client |
| Unit | Webhook validation token handshake | xUnit |
| Unit | Token refresh triggers correctly | xUnit, mock token store |
| Integration | OAuth connect flow | `WebApplicationFactory`, mock Microsoft OAuth |
| Integration | Webhook endpoint processes change notification | `WebApplicationFactory` |
| Manual | Connect Outlook, create appointment, verify event appears | Dev environment |

### Security & Privacy

- Same security considerations as Google Calendar (task 5.3)
- Microsoft Graph tokens scoped to `Calendars.ReadWrite` only
- Webhook subscription renewal must be automated (max 3-day lifetime)
- Microsoft app registration credentials in environment variables only
- Validate webhook notification origin via client state token

### TDD Reminder

Write mapper tests first. Then service tests with mocked Graph client. Follow same pattern as Google Calendar tests for consistency.

---

## Task 5.5 -- Calendar Sync Engine

**Status:** PENDING
**Dependencies:** 5.3, 5.4 (both calendar integrations must implement `ICalendarSyncService`)

### Scope

**In scope:**
- Background hosted service (`CalendarSyncWorker`) that runs periodic sync
- Listen for `AppointmentCreatedV1`, `AppointmentUpdatedV1`, `AppointmentCancelledV1` domain events and push changes to connected calendars
- Pull changes from external calendars periodically (fallback for missed webhooks)
- Rate limiting to respect Google (quota) and Microsoft (throttling) API limits
- Retry with exponential backoff for transient failures
- Conflict resolution: last-write-wins based on `UpdatedAt` timestamp comparison
- Sync status tracking per agent per provider
- Dead letter handling for permanently failed sync operations

**Out of scope:**
- Real-time bidirectional sync (webhooks handle near-real-time; this is the periodic fallback)
- Conflict resolution UI (always automatic)
- Multi-calendar support per provider

### Acceptance Criteria

- [ ] `CalendarSyncWorker` hosted service runs every 5 minutes (configurable)
- [ ] On `AppointmentCreatedV1`: creates event in all connected calendars for the agent
- [ ] On `AppointmentUpdatedV1`: updates event in all connected calendars
- [ ] On `AppointmentCancelledV1`: cancels/deletes event in connected calendars
- [ ] Periodic pull: fetches changes from Google/Microsoft since last sync token
- [ ] External changes create/update/cancel appointments in Propely
- [ ] Conflict resolution: if both sides changed, last-write-wins based on `UpdatedAt` comparison
- [ ] Rate limiting: max 10 requests/second to Google, max 4 requests/second to Microsoft Graph
- [ ] Retry: exponential backoff with jitter, max 3 retries, then dead-letter
- [ ] Dead-lettered sync operations logged with details for manual review
- [ ] Sync status per agent: `LastSyncedUtc`, `LastSyncError`, `SyncState` (Active/Error/Disabled)
- [ ] If OAuth token is expired and refresh fails, set sync state to `Error` and notify agent
- [ ] Structured logging for sync operations (start, success, failure, conflict)
- [ ] Telemetry metrics: sync duration, events synced, conflicts resolved, errors
- [ ] Unit tests for sync logic with mocked services
- [ ] Integration test for event-driven sync flow

### Implementation Steps

1. Create `CalendarSyncWorker` as `BackgroundService` in Infrastructure layer
2. Create `ICalendarSyncOrchestrator` interface in Application layer
3. Create `CalendarSyncOrchestrator` implementation that coordinates sync for all agents
4. Create MediatR event handlers for `AppointmentCreatedV1`, `AppointmentUpdatedV1`, `AppointmentCancelledV1` that trigger immediate sync
5. Implement rate limiter using `System.Threading.RateLimiting` (per provider)
6. Implement retry logic with Polly
7. Create `SyncOperation` entity for tracking individual sync attempts
8. Implement conflict resolution: compare `UpdatedAt` timestamps, apply last-write-wins
9. Create dead-letter mechanism for permanently failed operations
10. Add sync status fields to `CalendarConnection` entity
11. Add telemetry counters and histograms
12. Create migration for sync tracking tables
13. Write unit tests for orchestrator and conflict resolution
14. Write integration test for event-driven flow

### Files to Create/Modify

**Create:**
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/Interfaces/ICalendarSyncOrchestrator.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/EventHandlers/AppointmentCreatedSyncHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/EventHandlers/AppointmentUpdatedSyncHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/EventHandlers/AppointmentCancelledSyncHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/CalendarSyncWorker.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/CalendarSyncOrchestrator.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/CalendarRateLimiter.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/SyncConflictResolver.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/SyncOperation.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/Configurations/SyncOperationConfiguration.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/CalendarSyncOrchestratorTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Infrastructure.Tests/Calendar/SyncConflictResolverTests.cs`
- `services/appointments-api/tests/Propely.AppointmentsApi.Application.Tests/Appointments/EventHandlers/AppointmentCreatedSyncHandlerTests.cs`

**Modify:**
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarConnection.cs` (add sync status fields)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<SyncOperation>`)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/DependencyInjection.cs` (register sync services and worker)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `SyncConflictResolver` applies last-write-wins correctly | xUnit + FluentAssertions |
| Unit | `CalendarSyncOrchestrator` syncs to all connected providers | xUnit, mock `ICalendarSyncService` |
| Unit | Event handler triggers immediate sync | xUnit, mock orchestrator |
| Unit | Rate limiter throttles requests correctly | xUnit, time-based assertions |
| Unit | Retry logic retries on transient failure, dead-letters on permanent | xUnit, Polly test helpers |
| Integration | Appointment creation triggers sync to mocked calendar | `WebApplicationFactory` |
| Manual | Create appointment with Google Calendar connected, verify sync | Dev environment |

### Security & Privacy

- Sync worker runs with system-level access; must still respect tenant boundaries
- Rate limiting prevents DoS against external calendar APIs
- Dead-lettered operations may contain appointment IDs but not PII content
- Structured logs must not include OAuth tokens or calendar event details

### TDD Reminder

Write conflict resolution tests first with various timestamp scenarios. Then write orchestrator tests. Finally implement the background worker.

---

## Task 5.6 -- Frontend Calendar View

**Status:** PENDING
**Dependencies:** 5.2 (appointment API must be available)

### Scope

**In scope:**
- Calendar page at `/[locale]/(dashboard)/calendar`
- FullCalendar integration with month, week, and day views
- Events colored by appointment type (PropertyViewing = blue, OwnerMeeting = orange, Generic = gray)
- Click event to open detail panel
- Drag-and-drop to reschedule (updates start/end times via API)
- Today button, date navigation, view toggle
- Filter by appointment type and agent (admin view)
- Mini calendar for quick date navigation
- Stitch design for calendar page
- i18n support (en, es) including date/time localization
- Responsive design

**Out of scope:**
- Appointment booking (task 5.7)
- Calendar sync settings UI (future enhancement)
- Recurring event display
- Resource/room booking view

### Acceptance Criteria

- [ ] Calendar page displays events from appointment API
- [ ] Month view shows event dots/bars on dates; week/day views show time blocks
- [ ] View toggle: Month, Week, Day
- [ ] Events colored by type: PropertyViewing (#3B82F6 blue), OwnerMeeting (#F59E0B amber), Generic (#6B7280 gray)
- [ ] Clicking an event opens a detail slide-over with all appointment fields
- [ ] Detail slide-over shows: title, type badge, status badge, date/time, location, property link, contact link, agent
- [ ] Drag-and-drop an event to reschedule: calls `PUT /api/appointments/{id}` with new times
- [ ] Drag-and-drop shows confirmation toast on success, reverts on failure
- [ ] Event resize (week/day views) to change duration
- [ ] Today button, previous/next navigation arrows, date title
- [ ] Filter toolbar: appointment type multi-select, agent dropdown (admin only)
- [ ] Mini calendar sidebar for quick date jumping
- [ ] Loading spinner during initial data fetch
- [ ] Events refetch when navigating to new date ranges
- [ ] Stitch design exists for calendar page layout
- [ ] All custom components tested with Vitest + RTL
- [ ] Responsive: full calendar on desktop, simplified list view on mobile (<768px)
- [ ] Dates and times localized using `next-intl` formatters

### Implementation Steps

1. Create Stitch design for calendar page
2. Download Stitch HTML to `.stitch-html/`
3. Install `@fullcalendar/react`, `@fullcalendar/daygrid`, `@fullcalendar/timegrid`, `@fullcalendar/interaction`
4. Create `useAppointments` hook (fetch appointments for date range)
5. Create `CalendarPage` component with FullCalendar wrapper
6. Create `AppointmentDetailPanel` slide-over component
7. Create `AppointmentTypeBadge` component
8. Create `AppointmentStatusBadge` component
9. Create `CalendarFilters` component
10. Create `MiniCalendar` component
11. Implement drag-and-drop rescheduling with API call
12. Implement event resize handling
13. Implement mobile responsive layout (list view fallback)
14. Add routes and navigation
15. Add i18n keys with date/time formatting
16. Write component tests
17. Verify against Stitch design

### Files to Create/Modify

**Create:**
- `apps/web/src/app/[locale]/(dashboard)/calendar/page.tsx`
- `apps/web/src/components/calendar/CalendarView.tsx`
- `apps/web/src/components/calendar/AppointmentDetailPanel.tsx`
- `apps/web/src/components/calendar/AppointmentTypeBadge.tsx`
- `apps/web/src/components/calendar/AppointmentStatusBadge.tsx`
- `apps/web/src/components/calendar/CalendarFilters.tsx`
- `apps/web/src/components/calendar/MiniCalendar.tsx`
- `apps/web/src/components/calendar/MobileCalendarList.tsx`
- `apps/web/src/hooks/useAppointments.ts`
- `apps/web/src/hooks/useUpdateAppointment.ts`
- `apps/web/src/components/calendar/__tests__/CalendarView.test.tsx`
- `apps/web/src/components/calendar/__tests__/AppointmentDetailPanel.test.tsx`
- `apps/web/src/components/calendar/__tests__/CalendarFilters.test.tsx`
- `.stitch-html/calendar-page.html`

**Modify:**
- `apps/web/package.json` (add FullCalendar packages)
- `apps/web/src/messages/en.json` (add calendar i18n keys)
- `apps/web/src/messages/es.json` (add calendar i18n keys)
- `apps/web/src/components/layout/DashboardSidebar.tsx` (add Calendar nav item)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `CalendarView` renders events on correct dates | Vitest + RTL, mock FullCalendar |
| Unit | `AppointmentDetailPanel` displays all fields | Vitest + RTL |
| Unit | `AppointmentTypeBadge` renders correct color per type | Vitest + RTL |
| Unit | `CalendarFilters` emits filter changes | Vitest + RTL |
| Unit | Drag-and-drop triggers update API call | Vitest + RTL |
| Unit | `useAppointments` fetches correct date range | Vitest, mock fetch |
| Manual | Full calendar interaction: navigate, view events, reschedule | Dev environment |
| Manual | Visual comparison against Stitch design | Dev environment |
| Manual | Mobile layout responsiveness test | Dev environment, device emulator |

### Security & Privacy

- Appointment data displayed only within authenticated context
- Date range queries scoped to tenant
- No appointment data cached in browser storage

### TDD Reminder

Write component render tests and interaction tests (drag-and-drop, filter changes) first. Then implement components and integrate FullCalendar.

---

## Task 5.7 -- Frontend Appointment Booking

**Status:** PENDING
**Dependencies:** 5.2 (appointment API), 5.6 (calendar view for context)

### Scope

**In scope:**
- Booking modal accessible from: property detail page, contact detail page, calendar page (click empty slot)
- Date/time picker with available time slots
- Appointment type selector
- Property selector (pre-filled if from property detail)
- Contact selector (pre-filled if from contact detail)
- Location field with optional Google Maps autocomplete
- Notes/description textarea
- Form validation matching backend rules
- Success confirmation with option to open in calendar
- Stitch design for booking modal

**Out of scope:**
- Availability checking against external calendars (future enhancement)
- Client/contact self-service booking (agent-only for now)
- Recurring appointment creation
- Reminders/notification setup during booking

### Acceptance Criteria

- [ ] "Book Appointment" button appears on property detail, contact detail, and calendar page
- [ ] Clicking empty time slot on calendar opens booking modal pre-filled with selected date/time
- [ ] Booking modal includes: type dropdown, title, date picker, time range picker, property search/select, contact search/select, location, description
- [ ] Type selector: PropertyViewing, OwnerMeeting, Generic
- [ ] When type is PropertyViewing, property field is required (validation)
- [ ] Date picker shows calendar with today highlighted, past dates disabled
- [ ] Time range picker with 15-minute increments, validates end > start
- [ ] Property selector with search-as-you-type (debounced)
- [ ] Contact selector with search-as-you-type (debounced)
- [ ] Location field with text input (Google Maps autocomplete optional enhancement)
- [ ] Form validation errors displayed inline per field
- [ ] Submit calls `POST /api/appointments` and shows success toast
- [ ] Success state shows: "Appointment booked" with "View in Calendar" link
- [ ] Cancel/close dismisses modal without saving
- [ ] Loading state during submit
- [ ] Stitch design exists for booking modal
- [ ] All components tested with Vitest + RTL

### Implementation Steps

1. Create Stitch design for booking modal
2. Download Stitch HTML to `.stitch-html/`
3. Create `useCreateAppointment` hook (mutation)
4. Create `usePropertySearch` hook (debounced search for property selector)
5. Create `useContactSearch` hook (debounced search for contact selector)
6. Create `appointmentSchema` Zod validation schema
7. Create `AppointmentBookingModal` component
8. Create `AppointmentTypeSelector` component
9. Create `DateTimePicker` component (date + start/end time with 15-min increments)
10. Create `PropertySearchSelect` component
11. Create `ContactSearchSelect` component
12. Integrate booking trigger on property detail page
13. Integrate booking trigger on contact detail page
14. Integrate booking trigger on calendar page (click empty slot)
15. Add i18n keys
16. Write component tests
17. Verify against Stitch design

### Files to Create/Modify

**Create:**
- `apps/web/src/components/appointments/AppointmentBookingModal.tsx`
- `apps/web/src/components/appointments/AppointmentTypeSelector.tsx`
- `apps/web/src/components/appointments/DateTimePicker.tsx`
- `apps/web/src/components/appointments/PropertySearchSelect.tsx`
- `apps/web/src/components/appointments/ContactSearchSelect.tsx`
- `apps/web/src/hooks/useCreateAppointment.ts`
- `apps/web/src/hooks/usePropertySearch.ts`
- `apps/web/src/hooks/useContactSearch.ts`
- `apps/web/src/schemas/appointmentSchema.ts`
- `apps/web/src/components/appointments/__tests__/AppointmentBookingModal.test.tsx`
- `apps/web/src/components/appointments/__tests__/DateTimePicker.test.tsx`
- `apps/web/src/components/appointments/__tests__/PropertySearchSelect.test.tsx`
- `.stitch-html/appointment-booking-modal.html`

**Modify:**
- `apps/web/src/app/[locale]/(dashboard)/properties/[id]/page.tsx` (add Book Appointment button)
- `apps/web/src/app/[locale]/(dashboard)/contacts/[id]/page.tsx` (add Book Appointment button)
- `apps/web/src/app/[locale]/(dashboard)/calendar/page.tsx` (add click-to-book on empty slots)
- `apps/web/src/messages/en.json` (add booking i18n keys)
- `apps/web/src/messages/es.json` (add booking i18n keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `appointmentSchema` validates required fields, end > start, type-specific rules | Vitest |
| Unit | `AppointmentBookingModal` renders all fields, validates, submits | Vitest + RTL |
| Unit | `DateTimePicker` shows 15-min increments, disables past dates | Vitest + RTL |
| Unit | `PropertySearchSelect` debounces search, displays results | Vitest + RTL, mock fetch |
| Unit | `ContactSearchSelect` debounces search, displays results | Vitest + RTL, mock fetch |
| Unit | `AppointmentTypeSelector` shows all types, highlights selected | Vitest + RTL |
| Unit | Pre-fill works from property context, contact context, calendar context | Vitest + RTL |
| Manual | Full booking flow from each entry point | Dev environment |
| Manual | Visual comparison against Stitch design | Dev environment |

### Security & Privacy

- Booking is agent-only; no public-facing booking form
- Property and contact search endpoints must respect tenant isolation
- No sensitive data stored in form state beyond the current session

### TDD Reminder

Write schema validation tests first (all field combinations). Then component render and interaction tests. Implement components to pass the tests.
