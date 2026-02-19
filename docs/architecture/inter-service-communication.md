# Inter-Service Communication

This document describes how Propely's microservices communicate with each other, including the NuGet SDK client pattern, tenant propagation, resilience policies, and async messaging.

## Communication Patterns

Propely uses two communication patterns:

1. **Synchronous HTTP** via NuGet SDK clients (Refit) -- for request/response queries and commands
2. **Asynchronous messaging** via RabbitMQ -- for domain event propagation and eventual consistency

## NuGet SDK Client Pattern

Each service that needs to be called by other services publishes a `Propely.<Service>.Client` NuGet package containing:

- **Refit interface** (`IXxxApiClient`) with typed HTTP method definitions
- **Request/Response DTOs** matching the API contract
- **`IServiceCollection` extension** (`AddXxxApiClient()`) for DI registration
- **Configuration class** (`XxxApiClientOptions`) for base URL, timeouts, and resilience settings

### SDK Dependency Graph

```
orgs-api ──publishes──> Propely.OrgsApi.Client
                             │
                             ├── consumed by ai-api
                             ├── consumed by properties-api
                             ├── consumed by publishing-api
                             ├── consumed by contacts-api
                             └── consumed by appointments-api

ai-api ──publishes──> Propely.AiApi.Client
                             │
                             └── consumed by properties-api

properties-api ──publishes──> Propely.PropertiesApi.Client
                             │
                             ├── consumed by publishing-api
                             ├── consumed by contacts-api
                             └── consumed by appointments-api

contacts-api ──publishes──> Propely.ContactsApi.Client
                             │
                             └── consumed by appointments-api
```

### Registration Example

```csharp
// In the consuming service's Program.cs or DependencyInjection.cs:
services.AddOrgsApiClient(options =>
{
    options.BaseUrl = "http://localhost:5020";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
});
```

### Tenant Context Propagation

All inter-service calls must propagate the tenant context. This is handled automatically by `TenantDelegatingHandler`:

1. The handler reads the current tenant ID from `IHttpContextAccessor` (for web-request contexts) or from an explicit configuration (for background jobs)
2. It adds the `X-Tenant-Id` header to every outgoing HTTP request
3. The receiving service extracts the tenant ID from the header and applies it to EF Core query filters

```
Browser ──[Auth Cookie]──> orgs-api ──[X-Tenant-Id]──> properties-api
                                                            │
                                                    EF Core filters by tenant
```

### Resilience (Polly)

Every SDK client includes Polly policies:

| Policy | Configuration |
|--------|---------------|
| **Retry** | Exponential backoff, max 3 retries, triggers on 5xx / 408 / 429 |
| **Circuit Breaker** | Opens after 5 consecutive failures, half-open after 30 seconds |
| **Timeout** | 30 seconds per request (configurable) |

These policies prevent cascade failures when a downstream service is temporarily unavailable.

## Async Messaging (RabbitMQ)

### Domain Events

Services publish domain events to RabbitMQ using the transactional outbox pattern:

1. Domain entity raises an event (e.g., `PropertyStatusChangedV1`)
2. The event is persisted to the `OutboxMessages` table in the same database transaction as the entity change
3. A background `OutboxDispatcher` service polls for unpublished messages and publishes them to RabbitMQ
4. Consumer services subscribe to events they care about and process them asynchronously

### Event Flow Examples

```
properties-api:
  PropertyStatusChangedV1 ──> RabbitMQ ──> publishing-api (trigger XML feed regeneration)

contacts-api:
  LeadConvertedV1 ──> RabbitMQ ──> appointments-api (suggest booking a viewing)

orgs-api:
  MemberRemovedV1 ──> RabbitMQ ──> all services (clean up user-specific data)
```

### Event Naming Convention

Events follow the pattern: `<Entity><Action>V<version>`

Examples:
- `PropertyCreatedV1`
- `PropertyStatusChangedV1`
- `LeadReceivedV1`
- `LeadConvertedV1`
- `AppointmentScheduledV1`
- `PropertyPublishedV1`

### Outbox Pattern

The outbox pattern guarantees at-least-once delivery:

```
┌─────────────────────────────────────┐
│  Database Transaction               │
│  1. Save entity changes             │
│  2. Insert OutboxMessage record     │
│  3. Commit                          │
└──────────────┬──────────────────────┘
               │
┌──────────────v──────────────────────┐
│  OutboxDispatcher (background)      │
│  1. Poll for unpublished messages   │
│  2. Publish to RabbitMQ             │
│  3. Mark message as published       │
└─────────────────────────────────────┘
```

Consumers must be **idempotent** since messages may be delivered more than once.

## Authentication and Authorization

### External Requests (Browser to API)
- JWT tokens in HTTP-only cookies
- CSRF protection with HMAC-signed tokens
- orgs-api is the identity provider; other services validate tokens against orgs-api's signing key

### Internal Requests (Service to Service)
- Tenant context via `X-Tenant-Id` header
- Authorization token forwarding (the originating user's JWT is forwarded through SDK calls)
- Services validate permissions by calling `Propely.OrgsApi.Client` to check user roles and permission overrides

## Error Handling

### Synchronous Calls
- HTTP 4xx errors are surfaced to the caller as-is
- HTTP 5xx errors trigger retry policies (Polly)
- Circuit breaker prevents repeated calls to a failing service
- Timeout policy prevents indefinite waits

### Asynchronous Messages
- Failed message processing is retried with exponential backoff
- After max retries, messages are sent to a dead-letter queue (DLQ)
- DLQ messages require manual investigation and reprocessing
