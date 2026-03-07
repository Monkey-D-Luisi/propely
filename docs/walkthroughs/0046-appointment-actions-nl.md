# Walkthrough 0046 -- Appointment Actions via Natural Language

**Task:** [0046-appointment-actions-nl](../tasks/0046-appointment-actions-nl.md)
**Epic:** P3 -- AI Action Engine (Task 3.6)

## Summary

Implemented 4 appointment action handlers following the established IActionHandler pattern. Each handler uses the AppointmentsApi SDK client (Refit) to execute appointment operations on behalf of the user.

## Decisions

- **30-minute default duration:** BookViewing defaults to 30 minutes if no end_time provided, matching real estate viewing norms.
- **Duration preservation on reschedule:** RescheduleAppointmentActionHandler fetches the existing appointment to compute original duration, then applies it to the new start time.
- **Type = PropertyViewing:** BookViewing hardcodes type as "PropertyViewing" since it's specifically for property viewings.
- **Default cancel reason:** CancelAppointment defaults reason to "Cancelled via voice/text command" if none provided.

## Files Created

| File | Description |
|------|-------------|
| `src/Application/Actions/Commands/AppointmentActions/BookViewingActionCommand.cs` | MediatR command record |
| `src/Application/Actions/Commands/AppointmentActions/QueryAppointmentsActionCommand.cs` | MediatR command record |
| `src/Application/Actions/Commands/AppointmentActions/CancelAppointmentActionCommand.cs` | MediatR command record |
| `src/Application/Actions/Commands/AppointmentActions/RescheduleAppointmentActionCommand.cs` | MediatR command record |
| `src/Application/Actions/Handlers/AppointmentActions/BookViewingActionHandler.cs` | Handler: creates appointment via SDK |
| `src/Application/Actions/Handlers/AppointmentActions/QueryAppointmentsActionHandler.cs` | Handler: queries appointments with filters |
| `src/Application/Actions/Handlers/AppointmentActions/CancelAppointmentActionHandler.cs` | Handler: cancels appointment via SDK |
| `src/Application/Actions/Handlers/AppointmentActions/RescheduleAppointmentActionHandler.cs` | Handler: reschedules via fetch + update |
| `tests/UnitTests/.../AppointmentActions/BookViewingActionHandlerTests.cs` | 7 tests |
| `tests/UnitTests/.../AppointmentActions/QueryAppointmentsActionHandlerTests.cs` | 4 tests |
| `tests/UnitTests/.../AppointmentActions/CancelAppointmentActionHandlerTests.cs` | 3 tests |
| `tests/UnitTests/.../AppointmentActions/RescheduleAppointmentActionHandlerTests.cs` | 7 tests |

## Files Modified

| File | Change |
|------|--------|
| `src/Application/Propely.AiApi.Application.csproj` | Added AppointmentsApi.Client project reference |
| `src/Infrastructure/Propely.AiApi.Infrastructure.csproj` | Added AppointmentsApi.Client project reference |
| `src/Infrastructure/AI/ToolDefinitions.cs` | Added 4 appointment tool definitions, updated All list + FunctionNameToActionType |
| `src/Infrastructure/AI/ActionRouter.cs` | Added 4 appointment switch cases, removed placeholders |
| `src/Infrastructure/DependencyInjection.cs` | Registered AppointmentsApi SDK client |
| `tests/UnitTests/.../ToolDefinitionsTests.cs` | Updated count 16→20, added 4 InlineData |
| `tests/UnitTests/.../ActionRouterTests.cs` | Replaced placeholder test with 2 dispatch tests |

## Commands Run

```bash
dotnet build services/ai-api/Propely.AiApi.sln           # 0 errors
dotnet test services/ai-api/tests/Propely.AiApi.UnitTests  # 354 passed
```

## Tests

- 21 new appointment handler tests (BookViewing: 7, Query: 4, Cancel: 3, Reschedule: 7)
- 2 new ActionRouter dispatch tests (BookViewing, CancelAppointment)
- 4 new ToolDefinitions InlineData entries
- Previous 331 tests still pass → Total: 354

## Checklist

- [x] Implementation complete
- [x] Tests passing
- [x] Walkthrough accurate
- [x] Committed
