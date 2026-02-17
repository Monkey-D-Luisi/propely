# Audit Action: audit-0007-projector-refactor

## Metadata
- ID: audit-0007
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-31
- Priority: P2
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Refactor WorkItemProjectorService into focused components"
- Dependencies: None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0007-projector-refactor.md`
  - Clean Architecture audit: `docs/audits/2026-01-30-clean-architecture.md` (Finding 5)

## Goal
Refactor the WorkItemProjectorService "god class" into focused, single-responsibility components to improve maintainability and testability.

## Context
The Clean Architecture audit (Finding 5) identified that `WorkItemProjectorService` manages too many responsibilities:
- RabbitMQ connection and channel management
- Queue/exchange declaration
- Message consumption
- Event deserialization and routing
- Read model projection
- Idempotency tracking
- Cache invalidation

This violates the Single Responsibility Principle and makes the class difficult to test and maintain.

## Scope
### In scope
- Extract RabbitMQ consumer infrastructure into reusable base/component
- Create event handler interface for projecting events
- Create focused WorkItem event projector
- Maintain existing functionality and behavior

### Out of scope
- Changing message formats or routing
- Adding new event types
- Modifying DLQ behavior

## Requirements
- R1: Each new component should have a single, clear responsibility
- R2: RabbitMQ infrastructure should be reusable for future consumers
- R3: Event projection logic should be testable in isolation
- R4: Existing functionality must be preserved

## Acceptance Criteria
- [x] RabbitMQ consumer extracted to reusable component
- [x] Event projector interface created
- [x] WorkItem projector implements focused projection logic
- [x] All 79 tests pass
- [x] No change in runtime behavior

## New Components
1. `IEventProjector` - Interface for projecting events to read model
2. `WorkItemEventProjector` - Focused projector for WorkItem events
3. Refactored `WorkItemProjectorService` - Orchestrator using above components

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (79 tests)
- [x] Walkthrough updated
