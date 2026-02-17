# Clean Architecture Audit Report

- Date: 2026-01-30
- Scope: Repository clean architecture review (Domain, Application, Infrastructure, Presentation)
- References: [docs/architecture/vertical-slice.md](../architecture/vertical-slice.md), [docs/architecture/repo-structure.md](../architecture/repo-structure.md)

## Summary
- Layering rules: Pass
- Dependency direction: Pass
- DDD patterns: Partial (entity + invariants present; limited value objects/aggregates)
- CQRS separation: Partial (query reads from write model; read model exists but not used by query handler)
- SOLID: Partial (one god-class risk in infrastructure projector service)

## Findings

### 1) Layering rules (Domain, Application, Infrastructure, Presentation)
**Status:** Pass

**Evidence:**
- Layered projects exist in `src/` for Domain, Application, Infrastructure, and API (Presentation). The project references follow the expected inward dependency chain (outer layers reference inner layers): Application -> Domain, Infrastructure -> Application, API -> Infrastructure. This aligns with the repository structure guidance. (See references in repo-structure and project files.)

### 2) Dependency direction (inward dependencies only)
**Status:** Pass

**Evidence:**
- Domain project has no project references, avoiding dependencies on outer layers.
- Application depends only on Domain.
- Infrastructure depends on Application (which depends on Domain), and API depends on Infrastructure.

### 3) DDD patterns (entities, value objects, aggregates, invariants)
**Status:** Partial

**Evidence:**
- WorkItem is a domain entity with creation behavior and invariants enforced in the factory (title/description validation), and it raises a domain event on creation.
- A domain event record is modeled with explicit metadata and typed data payload.

**Gaps:**
- No explicit value objects or aggregate boundaries beyond the `WorkItem` entity are present. This is acceptable for the current slice, but as complexity grows, value objects (e.g., Title) and aggregate behaviors should be considered.

### 4) CQRS separation (commands vs queries)
**Status:** Partial

**Evidence:**
- Command and query handlers are separated in the Application layer.
- A read model (`WorkItemRead`) and a projector exist in Infrastructure, but `GetWorkItemByIdQueryHandler` reads via `IWorkItemRepository`, which is backed by the write model DbSet (`WorkItems`) rather than the read model DbSet (`WorkItemsRead`).

**Impact:**
- Read/write separation is not fully realized, which can undermine CQRS benefits and may cause read-side scaling or projection concerns later.

### 5) SOLID review
**Status:** Partial

**Evidence:**
- `WorkItemProjectorService` manages RabbitMQ setup, message consumption, event dispatching, data projection, idempotency tracking, and cache invalidation in a single class. This is a Single Responsibility Principle (SRP) risk and a potential god class.

**Impact:**
- Increased coupling and reduced testability/maintainability in the infrastructure messaging pipeline.

## Recommendations
1. Introduce a read repository (e.g., `IWorkItemReadRepository`) backed by `WorkItemsRead` and update `GetWorkItemByIdQueryHandler` to use the read model explicitly.
2. Split `WorkItemProjectorService` responsibilities into smaller components (e.g., RabbitMQ connection/consumer, event deserializer, projector, cache invalidator) and compose them in the hosted service.
3. Consider value objects for WorkItem title/description if domain rules expand (keeps invariants closer to data types).

## Evidence Index (key files)
- Domain entity and invariants: [`src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`](../../src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs)
- Domain event definition: [`src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`](../../src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs)
- Command handler: [`src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs`](../../src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs)
- Query handler: [`src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs`](../../src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs)
- Write model repository: [`src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs)
- Read model entity: [`src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs)
- Projector service: [`src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`](../../src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs)
