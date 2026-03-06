# Task 0043 — Appointments-API NuGet SDK Client (5.8)

## Summary
Created the Propely.AppointmentsApi.Client NuGet package with Refit interfaces, request/response DTOs, TenantDelegatingHandler, and Polly resilience policies.

## Key Files
- `services/appointments-api/src/Propely.AppointmentsApi.Client/IAppointmentsApiClient.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/ServiceCollectionExtensions.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/Models/`

## Design Decisions
- Client DTOs use string-based enums (standalone NuGet, no domain reference).
- Polly resilience: exponential backoff retry (3 attempts), circuit breaker (5 throughput).
- TenantDelegatingHandler propagates X-Tenant-Id header.

## Tests
3 DI registration tests. Total: 277 passing.
