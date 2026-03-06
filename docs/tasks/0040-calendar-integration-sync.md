# Task 0040/0041/0042 — Calendar Integration & Sync Engine (5.3, 5.4, 5.5)

## Summary
Implemented calendar integration infrastructure: Google Calendar and Microsoft Outlook sync services (stub implementations), CalendarConnection domain entity with OAuth token management, sync operation tracking, a background CalendarSyncWorker, and the CalendarSyncOrchestrator with conflict resolution.

## Key Files
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarConnection.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/SyncOperation.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Calendar/` (all sync services)
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/CalendarController.cs`

## Design Decisions
- ICalendarSyncService is provider-agnostic; GoogleCalendarSyncService and MicrosoftCalendarSyncService are stub implementations (full OAuth requires API keys).
- CalendarSyncOrchestrator handles outbound (Propely→external) and inbound (external→Propely) sync.
- SyncOperation tracks individual sync attempts with retry count (max 3).
- Conflict resolution: last-write-wins based on UpdatedAt timestamp.
- CalendarSyncWorker runs every 5 minutes (configurable via CalendarConfiguration).
- Tokens encrypted via ASP.NET Data Protection API.

## Endpoints
| Method | Path | Description |
|--------|------|-------------|
| GET | /api/calendar/status | List calendar connections |
| POST | /api/calendar/{provider}/connect | Connect calendar |
| DELETE | /api/calendar/{provider}/disconnect | Disconnect |
| POST | /api/calendar/{provider}/webhook | Webhook receiver |

## Tests
73 new unit tests. Total: 274 passing.
