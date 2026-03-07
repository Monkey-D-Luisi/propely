# Task 0046 -- Appointment Actions via Natural Language

**Epic:** P3 -- AI Action Engine (Task 3.6)
**Status:** DONE

## Goal

Implement 4 appointment action handlers (BookViewing, QueryAppointments, CancelAppointment, RescheduleAppointment) so that agents can manage appointments through natural language via the AI Action Engine.

## Acceptance Criteria

- [x] **AC1:** `BookViewingActionHandler` creates appointments via `IAppointmentsApiClient`, validates property_id + start_time, defaults 30-min duration
- [x] **AC2:** `QueryAppointmentsActionHandler` queries appointments with filter by status/type/property_id/date range
- [x] **AC3:** `CancelAppointmentActionHandler` cancels appointments with optional reason
- [x] **AC4:** `RescheduleAppointmentActionHandler` reschedules appointment preserving original duration
- [x] **AC5:** 4 tool definitions registered in `ToolDefinitions.cs` with OpenAI function calling schema
- [x] **AC6:** `ActionRouter` dispatches all 4 appointment action types to MediatR
- [x] **AC7:** AppointmentsApi SDK client registered in DI
- [x] **AC8:** Unit tests cover all handlers (happy path, validation, error handling)
- [x] **AC9:** All 354 unit tests pass

## Definition of Done

- [x] Code compiles
- [x] Tests pass
- [x] Walkthrough updated
- [x] Committed
