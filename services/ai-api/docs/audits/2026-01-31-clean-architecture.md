# Clean Architecture Audit Report

- Date: 2026-01-31
- Scope: Repository clean architecture review (Domain, Application, Infrastructure, Presentation)
- References: [docs/architecture/vertical-slice.md](../architecture/vertical-slice.md), [docs/architecture/repo-structure.md](../architecture/repo-structure.md)

## Summary
- Layering rules: Pass
- Dependency direction: Pass
- DDD patterns: Partial (entity + invariants present; limited value objects/aggregate behaviors)
- CQRS separation: Pass (read model + cache-aside in query handler, with write-model fallback)
- SOLID: Partial (infrastructure services combine multiple responsibilities; event routing uses switch)

## Findings

### 1) Layering rules (Domain, Application, Infrastructure, Presentation)
**Status:** Pass

**Evidence:**
- Layered projects exist in `src/` for Domain, Application, Infrastructure, and API (Presentation).
- Project references follow the expected inward dependency chain: Application -> Domain, Infrastructure -> Application, API -> Infrastructure.

### 2) Dependency direction (inward dependencies only)
**Status:** Pass

**Evidence:**
- Domain has no project references.
- Application depends on Domain only.
- Infrastructure depends on Application (and transitively Domain).
- Presentation depends on Infrastructure.

### 3) DDD patterns (entities, value objects, aggregates, invariants)
**Status:** Partial

**Evidence:**
- `WorkItem` is a domain entity with behavior for creation, validation invariants (title/description), and raises a domain event on creation.
- Domain event metadata is captured via `IDomainEvent` and `WorkItemCreatedV1`.

**Gaps:**
- No explicit value objects (e.g., `WorkItemTitle`) or aggregate behaviors beyond creation are present. This is acceptable for the current slice, but future lifecycle rules (status transitions, updates) should live in the domain.
- Domain event `Producer` is hard-coded to `WorkItemApi`, which bakes a presentation detail into the domain event definition.

### 4) CQRS separation (commands vs queries)
**Status:** Pass

**Evidence:**
- Command handlers (mutations) and query handlers (reads) are separated in the Application layer.
- Query handler uses a read repository and cache-aside pattern, with an explicit read model (`WorkItemsRead`) fed by the projector.

**Notes:**
- The read repository falls back to the write model when the read model is not projected yet. This preserves correctness but relaxes strict read/write separation.

### 5) SOLID review
**Status:** Partial

**Evidence:**
- `WorkItemProjectorService` owns connection setup, queue declaration, consumption loop, and error handling in a single type (SRP risk).
- `WorkItemEventProjector` handles deserialization, idempotency tracking, persistence, and cache invalidation in one class, which can make changes harder to isolate.
- Event routing relies on a `switch` statement with string event types, requiring modification for each new event (OCP pressure).

## Prioritized Remediation Steps
1. **High:** Split `WorkItemProjectorService` into smaller components (connection/queue setup, consumer loop, dispatcher) and compose them in the hosted service to improve testability and SRP compliance.
2. **Medium:** Refactor `WorkItemEventProjector` into dedicated collaborators (deserializer, projector per event, idempotency tracker, cache invalidator) and use a registration-based dispatcher instead of a switch.
3. **Medium:** Move `Producer` metadata out of the domain event (or inject it at envelope/publishing time) to keep the domain model free of presentation concerns.
4. **Low:** Introduce value objects for `Title`/`Description` when domain rules expand, and add explicit domain methods for state transitions to avoid anemic behavior.

## Evidence Index (key files)
- Domain entity and invariants: [`src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`](../../src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs)
- Domain event definition: [`src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`](../../src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs)
- Command handler: [`src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs`](../../src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs)
- Query handler: [`src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs`](../../src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs)
- Read model repository: [`src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs)
- Read model entity: [`src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs)
- Projector service: [`src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs)
- Event projector: [`src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs)
