# Task 0038 — Appointment Domain Model (5.1)

## Summary
Implemented the `Appointment` aggregate root entity with full status state machine, domain events, and value objects in the `AppointmentsApi.Domain` layer.

## Key Files
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Appointment.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/AppointmentStatus.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/AppointmentType.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarProvider.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/CalendarSyncInfo.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Domain/Appointments/Events/`

## Design Decisions
- Status state machine: Scheduled can transition to Confirmed/Completed/Cancelled/NoShow. Confirmed can transition to Completed/Cancelled/NoShow. Completed, Cancelled, NoShow are terminal.
- PropertyViewing type requires non-null PropertyId; OwnerMeeting and Generic do not.
- CalendarSyncInfo is a value object (not owned entity in the domain sense) tracking external calendar event linkage per provider.
- Cancel requires a reason string; Complete optionally accepts notes.

## Tests
65 unit tests covering all state transitions, validations, and edge cases.
