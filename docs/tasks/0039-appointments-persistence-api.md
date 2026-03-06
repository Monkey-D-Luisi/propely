# Task 0039 — Appointments Persistence & API (5.2)

## Summary
Implemented the complete Application and Infrastructure layers for appointments-api: CQRS commands/queries, EF Core persistence, repositories, and REST API controller.

## Key Files
- `services/appointments-api/src/Propely.AppointmentsApi.Application/Appointments/` (commands, queries, DTOs, interfaces)
- `services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Persistence/` (AppDbContext, configurations, repositories)
- `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/AppointmentsController.cs`

## Endpoints
| Method | Path | Description |
|--------|------|-------------|
| GET | /api/appointments | List (paginated, filtered, sorted) |
| GET | /api/appointments/{id} | Get by ID |
| POST | /api/appointments | Create |
| PUT | /api/appointments/{id} | Update |
| DELETE | /api/appointments/{id} | Soft delete |
| PUT | /api/appointments/{id}/confirm | Confirm |
| PUT | /api/appointments/{id}/complete | Complete |
| PUT | /api/appointments/{id}/cancel | Cancel |
| PUT | /api/appointments/{id}/no-show | Mark no-show |

## Tests
33 new unit tests (handler + controller tests). Total: 201 passing.
