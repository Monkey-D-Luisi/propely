# Walkthrough: 0035-contacts-api-sdk-client

## Task Reference
- Task: `docs/tasks/0035-contacts-api-sdk-client.md`
- Walkthrough: `docs/walkthroughs/0035-contacts-api-sdk-client.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the `Propely.ContactsApi.Client` NuGet SDK package providing typed Refit HTTP clients (`IContactsApiClient`, `ILeadsApiClient`) for cross-service communication with the contacts-api. Includes 14 model files, TenantDelegatingHandler for tenant context propagation, and Polly resilience policies (retry, circuit breaker, timeout). Follows the same pattern established by `Propely.PropertiesApi.Client`.

## Context
- Background: The contacts-api REST endpoints (task 0033) and lead conversion (task 0034) were complete. Other services (ai-api, appointments-api) need typed HTTP clients to call contacts-api.
- Problem statement: Without an SDK client, consuming services would need to manually construct HTTP requests, handle serialization, and implement resilience policies independently.
- Constraints: Must follow existing SDK client pattern (PropertiesApi.Client), Refit for interface-based HTTP, Polly for resilience, TenantDelegatingHandler for multi-tenancy.

## Decisions & Trade-offs
- **Decision: Retry + circuit breaker (unlike AiApi.Client which uses timeout-only)**
  - Options considered: (1) Timeout-only (like ai-api), (2) Retry + circuit breaker (like properties-api)
  - Why this choice: Contacts-api operations are idempotent (GET) or guarded by unique constraints (POST), making retries safe. Unlike AI inference calls, CRUD operations benefit from automatic retry on transient failures.
  - Consequences: 3 retries with exponential backoff + circuit breaker after 5 failures with 30s recovery.

- **Decision: Two separate Refit interfaces (IContactsApiClient, ILeadsApiClient)**
  - Options considered: (1) Single monolithic interface, (2) Two focused interfaces
  - Why this choice: Interface Segregation Principle. Consuming services may only need contacts or only leads.
  - Consequences: DI registration creates both, but consumers depend only on what they need.

- **Decision: 14 model files matching API response schemas**
  - Why: Models are DTOs that match the API's JSON response shapes. Using separate models (not sharing with the server) allows independent versioning of the client package.

## Implementation Notes
- Key changes:
  - `IContactsApiClient` with CRUD methods: GetContactsAsync, GetContactByIdAsync, CreateContactAsync, UpdateContactAsync, DeleteContactAsync
  - `ILeadsApiClient` with CRUD + workflow methods: GetLeadsAsync, GetLeadByIdAsync, CreateLeadAsync, UpdateLeadAsync, DeleteLeadAsync, AssignLeadAsync, ChangeLeadStatusAsync, ConvertLeadAsync
  - 14 model files in `Models/` directory with XML doc comments
  - `TenantDelegatingHandler` reads ITenantContext and adds X-Tenant-Id header
  - `ServiceCollectionExtensions.AddContactsApiClient()` registers both interfaces with Polly pipeline
  - Polly pipeline: retry (3x exponential backoff for 5xx/408) -> circuit breaker (5 failures, 30s) -> timeout (30s)
- Edge cases handled:
  - Null base URL throws ArgumentNullException at registration time
  - TenantDelegatingHandler skips header if ITenantContext returns null tenant
- Known limitations:
  - No package versioning strategy yet (manual version bumps)
  - No automated integration tests against running service

## Data / Schema / Migrations
- DB changes: None (SDK client package, no database)
- Migration strategy: N/A
- Backward compatibility: N/A (new package)

## Commands Run
```bash
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/ --filter "ServiceCollectionExtensions"
```

## Files Changed

**Created:**
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj` -- Project file (net10.0, Refit, Polly)
- `services/contacts-api/src/Propely.ContactsApi.Client/IContactsApiClient.cs` -- Refit interface for contacts CRUD
- `services/contacts-api/src/Propely.ContactsApi.Client/ILeadsApiClient.cs` -- Refit interface for leads CRUD + workflow
- `services/contacts-api/src/Propely.ContactsApi.Client/TenantDelegatingHandler.cs` -- X-Tenant-Id header propagation
- `services/contacts-api/src/Propely.ContactsApi.Client/ServiceCollectionExtensions.cs` -- DI registration with Polly
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ContactResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/CreateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/UpdateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ContactListResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/LeadResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/CreateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/UpdateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/AssignLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ChangeStatusRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ConvertLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ConvertLeadResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/LeadListResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/PropertyInterestResponse.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs` -- DI registration tests

**Modified:**
- `services/contacts-api/Propely.ContactsApi.sln` -- Added Client project

## Tests
### Unit
- What was added/updated: DI registration tests verifying IContactsApiClient, ILeadsApiClient, and TenantDelegatingHandler are correctly registered
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/ --filter "ServiceCollectionExtensions"`

### Integration
- What was added/updated: N/A (manual dev environment verification)
- How to run: N/A

### Manual
- What you verified: N/A
- Steps: N/A

## Observability
- Logs added/updated: Polly retry and circuit breaker events logged via Microsoft.Extensions.Logging
- Traces/metrics added/updated: OpenTelemetry auto-instrumentation for HTTP client calls

## Security
- Validation: N/A (client-side; server validates all input)
- AuthN/AuthZ impact: Client propagates tenant context via TenantDelegatingHandler; auth token propagation handled by existing infrastructure
- Sensitive data handling: No secrets in package; base URL injected at registration time

## Follow-ups / Backlog
- [ ] Consume client in ai-api for contact/lead NL actions (future epic)
- [ ] Consume client in appointments-api for contact linking (future epic)

## Checklist
- [x] Task scope matches `docs/tasks/0035-contacts-api-sdk-client.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
