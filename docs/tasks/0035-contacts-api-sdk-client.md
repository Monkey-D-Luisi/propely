# Task: 0035-contacts-api-sdk-client

## Metadata
- ID: 0035
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0035-contacts-api-sdk-client.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.5)

## Goal
Create the Propely.ContactsApi.Client NuGet SDK package for cross-service communication, providing typed Refit HTTP clients for contacts and leads API endpoints with Polly resilience policies and tenant context propagation.

## Context
The contacts and leads API (task 0033) and lead conversion endpoint (task 0034) are complete. Other services (ai-api for NL contact/lead actions, appointments-api for contact linking) need a typed SDK client to call the contacts-api. This follows the same pattern established by `Propely.PropertiesApi.Client` and `Propely.AiApi.Client`.

## Scope
### In scope
- `Propely.ContactsApi.Client` class library project (net10.0, Refit + Polly)
- `IContactsApiClient` Refit interface: CRUD operations for contacts
- `ILeadsApiClient` Refit interface: CRUD, assign, status change, convert
- 14 model files matching API response schemas (ContactResponse, LeadResponse, etc.)
- `TenantDelegatingHandler` for X-Tenant-Id header propagation
- `ServiceCollectionExtensions.AddContactsApiClient()` with Polly retry + circuit breaker policies
- Registration of all interfaces via DI extension method
- Unit tests for DI registration

### Out of scope
- Publishing the package to NuGet.org (CI/CD concern)
- Authentication token management (existing infrastructure handles this)
- Integration tests against running contacts-api (manual dev verification)

## Requirements
- R1: `IContactsApiClient` has methods for: GetContactsAsync, GetContactByIdAsync, CreateContactAsync, UpdateContactAsync, DeleteContactAsync
- R2: `ILeadsApiClient` has methods for: GetLeadsAsync, GetLeadByIdAsync, CreateLeadAsync, DeleteLeadAsync, AssignLeadAsync, ChangeLeadStatusAsync, ConvertLeadAsync
- R3: All DTOs have XML doc comments
- R4: `AddContactsApiClient(this IServiceCollection, Action<ContactsApiClientOptions> configure)` registers both Refit interfaces
- R5: Retry policy: 3 retries with exponential backoff for 5xx and 408
- R6: Circuit breaker: minimum throughput 5 requests sampled, break after 50% failure ratio, 30s recovery
- R7: Timeout: 30 seconds per request
- R8: TenantDelegatingHandler propagates X-Tenant-Id header from `IHttpContextAccessor`

## Acceptance Criteria
- AC1: `IContactsApiClient` Refit interface has CRUD methods with correct route attributes
- AC2: `ILeadsApiClient` Refit interface has CRUD + assign + status + convert methods
- AC3: All 14 model files build without warnings and have XML doc comments
- AC4: `AddContactsApiClient()` registers IContactsApiClient and ILeadsApiClient in DI
- AC5: TenantDelegatingHandler registered as transient
- AC6: Polly retry policy configured (3 retries, exponential backoff)
- AC7: Polly circuit breaker configured (5 failures, 30s break)
- AC8: Polly timeout configured (30 seconds)
- AC9: Package builds without warnings
- AC10: DI registration unit tests pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Follow the same pattern as Propely.PropertiesApi.Client.

## Proposed Approach (high-level)
Follow the established SDK client pattern from PropertiesApi.Client: create model DTOs, Refit interfaces, TenantDelegatingHandler, and DI registration extension with Polly policies. Write DI registration tests first, then implement.

## Implementation Steps
1. Create `Propely.ContactsApi.Client.csproj` with Refit and Polly dependencies
2. Create contact model DTOs: ContactResponse, CreateContactRequest, UpdateContactRequest, ContactListResponse
3. Create lead model DTOs: LeadResponse, CreateLeadRequest, UpdateLeadRequest, AssignLeadRequest, ChangeStatusRequest, ConvertLeadRequest, ConvertLeadResponse, LeadListResponse
4. Create shared model DTOs: PagedResponse, PropertyInterestResponse
5. Create `IContactsApiClient` Refit interface with route attributes
6. Create `ILeadsApiClient` Refit interface with route attributes
7. Create `TenantDelegatingHandler` for X-Tenant-Id propagation
8. Create `ServiceCollectionExtensions.AddContactsApiClient()` with Polly policies
9. Add project reference to contacts-api solution
10. Write DI registration unit tests

## Files to Create / Modify
**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj`
- `services/contacts-api/src/Propely.ContactsApi.Client/IContactsApiClient.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/ILeadsApiClient.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/TenantDelegatingHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/ServiceCollectionExtensions.cs`
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
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs`

**Modify:**
- `services/contacts-api/Propely.ContactsApi.sln` -- Add Client project to solution

## Testing Plan
- Unit tests:
  - AddContactsApiClient registers IContactsApiClient in DI
  - AddContactsApiClient registers ILeadsApiClient in DI
  - AddContactsApiClient registers TenantDelegatingHandler as transient
  - AddContactsApiClient with custom base URL applies configuration
  - DTO serialization round-trip (System.Text.Json)
- Integration tests: Manual verification against running contacts-api in dev environment
- Manual verification: N/A (DI tests sufficient)

## Security & Privacy
- Client transmits PII (contact/lead data); ensure HTTPS in production
- No secrets stored in the client package
- Base URL configured at registration time, not hardcoded
- TenantDelegatingHandler ensures tenant context propagation

## Observability
- Logs: Polly retry/circuit breaker events logged via Microsoft.Extensions.Logging
- Metrics: N/A
- Traces: OpenTelemetry auto-instrumentation for HTTP client calls

## Rollback Plan
Remove the Client project from the solution and revert. No database changes.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes (no warnings)
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
