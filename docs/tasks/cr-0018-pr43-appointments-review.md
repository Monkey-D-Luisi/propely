# CR-0018 — PR #43 Appointments & Calendar Code Review

## PR Metadata
- **PR**: [#43](https://github.com/Monkey-D-Luisi/propely/pull/43)
- **Branch**: `feat/P5-appointments` → `main`
- **Scope**: Full appointments-api microservice + frontend calendar UI
- **Diff size**: ~9349 lines across ~90 files
- **Automated reviewers**: Copilot, Codex, Gemini Code Assist

## Changed Files
Backend: Domain, Application, Infrastructure, API, Client (appointments-api)
Frontend: CalendarView, AppointmentForm, AppointmentDetailPanel, hooks, page, i18n
Docs: roadmap, backlog, task/walkthrough docs

---

## Section 1: Agent Review Findings

### F-A1 — `useUpdateAppointmentStatus` calls non-existent endpoint
- **Severity**: MUST_FIX
- **Category**: API-to-UI contract parity
- **File**: `apps/web/src/hooks/useUpdateAppointmentStatus.ts:12`
- **Description**: Hook sends `PUT /api/appointments/${id}/status` but the API has no `/status` route. Actual endpoints are `/confirm`, `/complete`, `/cancel`, `/no-show`. The Cancel endpoint requires `{ reason }` body, Complete accepts `{ notes }`.
- **Fix**: Rewrite hook to route to the correct status-specific endpoints.

### F-A2 — `AppointmentRepository.GetByIdAsync` doesn't filter soft-deleted records
- **Severity**: MUST_FIX
- **Category**: Data & Persistence
- **File**: `...Infrastructure/Persistence/Repositories/AppointmentRepository.cs:21`
- **Description**: EF Core global query filter `HasQueryFilter(a => !a.IsDeleted)` on the `AppointmentConfiguration` already handles this. The write repository uses `FirstOrDefaultAsync` which applies the global filter. This is actually NOT a bug.
- **Resolution**: FALSE_POSITIVE — EF Core global query filters apply to all queries including tracked ones unless explicitly ignored with `IgnoreQueryFilters()`.

### F-A3 — `CalendarConnectionRepository.GetByAgentAndProviderAsync` and soft-delete blocking reconnect
- **Severity**: SHOULD_FIX
- **Category**: Data & Persistence
- **File**: `...Infrastructure/Persistence/Repositories/CalendarConnectionRepository.cs:25-34`
- **Description**: Same as F-A2 — global query filter on CalendarConnection already filters `is_deleted = false`. The unique index also has `HasFilter("is_deleted = false")`. A soft-deleted connection won't block reconnection.
- **Resolution**: FALSE_POSITIVE — EF Core global query filter handles this correctly.

### F-A4 — `UpdateAppointmentApiRequest.Type` field silently ignored
- **Severity**: SHOULD_FIX
- **Category**: API-to-UI contract parity
- **File**: `...Api/Dtos/AppointmentRequests.cs:23`; `...Api/Controllers/AppointmentsController.cs:125-138`
- **Description**: The `UpdateAppointmentApiRequest` has a `Type` property, but the controller doesn't map it to `UpdateAppointmentCommand`, and `Appointment.Update()` doesn't accept a type parameter. The Type is immutable after creation (by design). The DTO should not expose the field.
- **Fix**: Remove `Type` from `UpdateAppointmentApiRequest`.

### F-A5 — `AppointmentRepository.Delete` is semantically misleading
- **Severity**: SHOULD_FIX
- **Category**: Code Quality
- **File**: `...Infrastructure/Persistence/Repositories/AppointmentRepository.cs:36-39`
- **Description**: `Delete()` just calls `_context.Appointments.Update()` because soft-delete is done at the domain level by `SoftDelete()`. The handler calls `appointment.SoftDelete()` then `_repository.Delete(appointment)`. Using `Update()` internally is correct (persists the IsDeleted flag change), but `Delete` calling `Update` is confusing.
- **Fix**: Remove `Delete` method; have `DeleteAppointmentCommandHandler` call `.Update()` directly after `SoftDelete()`, which is what the method actually does.

### F-A6 — `CalendarSyncOrchestrator.ProcessOperationAsync` returns silently when appointment is null
- **Severity**: SHOULD_FIX
- **Category**: Code Quality
- **File**: `...Infrastructure/Calendar/CalendarSyncOrchestrator.cs:154-159`
- **Description**: When appointment is null, the method returns silently but the caller marks the operation as `Complete()` (line 117). A missing appointment means the operation did NOT succeed — it should be marked as Failed.
- **Fix**: Throw or return a flag so the caller can mark the operation as Failed instead of Completed.

### F-A7 — Sort keys `start`/`end` don't match domain property names
- **Severity**: NIT
- **Category**: Code Quality
- **File**: `...Infrastructure/Persistence/Repositories/AppointmentReadRepository.cs:115-116`
- **Description**: `"start"` and `"end"` map to `StartTimeUtc` and `EndTimeUtc` respectively. The frontend likely sends `startTimeUtc` or `start` — the shorthand is fine as a user-facing API convention. Not a bug.
- **Resolution**: FALSE_POSITIVE — Short sort parametrs are a valid API convention.

### F-A8 — Hardcoded hex colors in CalendarView.tsx
- **Severity**: NIT
- **Category**: Frontend
- **File**: `apps/web/src/components/appointments/CalendarView.tsx:17-19, 96-173`
- **Description**: FullCalendar requires CSS custom properties and direct hex color values for event rendering. Tailwind semantic classes can't be applied to FullCalendar's internal rendering. The inline `<style>` block is the standard FullCalendar theming approach.
- **Resolution**: FALSE_POSITIVE — FullCalendar requires hex values for event BG/border/text colors and CSS variables. Cannot use Tailwind semantic classes here.

### F-A9 — `CalendarTokenExchangeService` throws `NotImplementedException`
- **Severity**: NIT
- **Category**: Code Quality
- **File**: `...Infrastructure/Calendar/CalendarTokenExchangeService.cs:30`
- **Description**: Throws 500 ISE instead of 501 Not Implemented. However, the CalendarController catches exceptions and the endpoint exists explicitly as a stub. A user calling the connect endpoint will get a 500 instead of 501.
- **Fix**: Use `NotSupportedException` instead (conventional for "stub" implementations), but more importantly the CalendarController should handle this gracefully.

---

## Section 2: Review Comment Threads

### RC-1 (Copilot) — `useUpdateAppointmentStatus` wrong endpoint
- **Source**: Copilot inline review
- **Classification**: MUST_FIX (agrees with F-A1)
- **Action**: Fix in F-A1 resolution

### RC-2 (Codex) — Write repository returns soft-deleted records
- **Source**: Codex review
- **Classification**: FALSE_POSITIVE (see F-A2)
- **Rationale**: EF Core global query filter applies to all LINQ queries

### RC-3 (Codex) — Soft-deleted calendar connections block reconnect
- **Source**: Codex review
- **Classification**: FALSE_POSITIVE (see F-A3)
- **Rationale**: Query filter + partial unique index handle this

### RC-4 (Copilot) — `Type` field in `UpdateAppointmentApiRequest` silently ignored
- **Source**: Copilot inline review
- **Classification**: SHOULD_FIX (agrees with F-A4)
- **Action**: Fix in F-A4 resolution

### RC-5 (Copilot) — CalendarConnection unique index missing TenantId
- **Source**: Copilot inline review
- **Classification**: FALSE_POSITIVE
- **Rationale**: The business rule is one connection per agent per provider (agent IDs are globally unique GUIDs). TenantId is in the query filter, not the unique constraint. This is correct.

### RC-6 (Copilot) — `AppointmentRepository.Delete` calls `Update`
- **Source**: Copilot inline review
- **Classification**: SHOULD_FIX (agrees with F-A5)
- **Action**: Fix in F-A5 resolution

### RC-7 (Codex) — Sort key mismatch
- **Source**: Codex review
- **Classification**: FALSE_POSITIVE (see F-A7)

### RC-8 (Copilot) — CalendarSyncOrchestrator marks missing appointment as completed
- **Source**: Copilot inline review
- **Classification**: SHOULD_FIX (agrees with F-A6)
- **Action**: Fix in F-A6 resolution

### RC-9 (Codex) — CalendarTokenExchangeService throws 500
- **Source**: Codex review
- **Classification**: NIT (see F-A9)
- **Action**: Fix in F-A9 resolution

### RC-10 (Codex) — Data Protection key ring persistence
- **Source**: Codex review
- **Classification**: OUT_OF_SCOPE
- **Rationale**: Key ring persistence is a DevOps/deployment concern. Handled by Docker volume mounts in production. Not a code fix.

### RC-11 (Copilot) — Hardcoded hex colors in CalendarView
- **Source**: Copilot inline review
- **Classification**: FALSE_POSITIVE (see F-A8)

### RC-12 (Gemini) — Spanish translations
- **Source**: Gemini Code Assist
- **Classification**: FALSE_POSITIVE
- **Rationale**: App supports i18n (English + Spanish). es.json is required.

---

## Resolution Plan

### MUST_FIX
- [x] F-A1/RC-1: Rewrite `useUpdateAppointmentStatus` to route to correct status endpoints

### SHOULD_FIX
- [x] F-A4/RC-4: Remove `Type` from `UpdateAppointmentApiRequest`
- [x] F-A5/RC-6: Remove misleading `Delete` method from `AppointmentRepository`, update handler
- [x] F-A6/RC-8: Fix `CalendarSyncOrchestrator` to fail operations when appointment is missing

### NIT
- [x] F-A9/RC-9: Replace `NotImplementedException` with graceful handling

### FALSE_POSITIVE (no action)
- F-A2/RC-2: Write repo soft-delete — EF Core global filter handles this
- F-A3/RC-3: Calendar reconnect — query filter + partial unique index handles this
- F-A7/RC-7: Sort key shorthand — valid API convention
- F-A8/RC-11: Hex colors — FullCalendar requirement
- RC-5: Index missing TenantId — agent GUIDs are globally unique
- RC-12: Spanish translations — required for i18n

### OUT_OF_SCOPE (no action)
- RC-10: Data Protection key ring — deployment concern
