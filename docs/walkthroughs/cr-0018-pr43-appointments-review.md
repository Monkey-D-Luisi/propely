# Walkthrough CR-0018 — PR #43 Appointments & Calendar Code Review

## Task Reference
- Task: `docs/tasks/cr-0018-pr43-appointments-review.md`
- PR: [#43](https://github.com/Monkey-D-Luisi/propely/pull/43)

## Review Sources
- Independent agent review (Phase A)
- 15 inline review comments (Copilot + Codex)
- 2 general reviews (Copilot + Codex)
- 1 issue comment (Gemini Code Assist)

## Fixes Applied

### 1. MUST_FIX: `useUpdateAppointmentStatus` — wrong endpoint (F-A1)
**Problem**: Hook called `PUT /api/appointments/${id}/status` which doesn't exist. The API exposes individual status endpoints: `/confirm`, `/complete`, `/cancel`, `/no-show`.

**Fix**: Rewrote the hook to map each target status to its correct endpoint:
- `Confirmed` → `PUT /api/appointments/${id}/confirm` (no body)
- `Completed` → `PUT /api/appointments/${id}/complete` (optional `{ notes }`)
- `Cancelled` → `PUT /api/appointments/${id}/cancel` (required `{ reason }`)
- `NoShow` → `PUT /api/appointments/${id}/no-show` (no body)

Updated tests accordingly.

### 2. SHOULD_FIX: Remove `Type` from `UpdateAppointmentApiRequest` (F-A4)
**Problem**: The `Type` field in the update DTO was silently ignored — not mapped to the command, and `Appointment.Update()` doesn't allow type changes (type is immutable after creation).

**Fix**: Removed `Type` property from `UpdateAppointmentApiRequest`.

### 3. SHOULD_FIX: Remove misleading `Delete` from `AppointmentRepository` (F-A5)
**Problem**: `Delete()` internally called `Update()` which is semantically confusing. Soft-delete is performed at the domain level via `SoftDelete()`, and the repository just persists the change.

**Fix**: Removed `Delete` method from `AppointmentRepository` and `IAppointmentRepository`. Updated `DeleteAppointmentCommandHandler` to call `_repository.Update()` directly after `SoftDelete()`.

### 4. SHOULD_FIX: `CalendarSyncOrchestrator` — fail operation when appointment missing (F-A6)
**Problem**: When appointment is null in `ProcessOperationAsync`, the method returned silently but the caller then marked the operation as Completed. A missing appointment means the sync failed.

**Fix**: Changed `ProcessOperationAsync` to return a `bool` indicating success. When appointment is null, returns `false`. Caller now marks operation as Failed with message "Appointment not found" instead of Completed.

### 5. NIT: `CalendarTokenExchangeService` graceful handling (F-A9)
**Problem**: Threw `NotImplementedException` producing a 500 ISE. Should indicate the feature is not yet implemented.

**Fix**: Changed to throw `NotSupportedException` with a clear message. The CalendarController's connect endpoint now catches `NotSupportedException` and returns 501 Not Implemented.

## Validation
- `dotnet test services/appointments-api/Propely.AppointmentsApi.sln` — all tests passing
- `cd apps/web && npm test` — all tests passing

## Process Deviations
None. Standard code review workflow executed end-to-end.
