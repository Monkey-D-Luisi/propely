# Audit Action: audit-0010-event-metadata

## Metadata
- ID: audit-0010
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-31
- Priority: P3
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Expand event consumer DTO to require metadata + document retry/DLQ policy"
- Dependencies: audit-0007 (projector refactor)
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0010-event-metadata.md`

## Goal
Expand event consumer DTOs to require standard metadata fields and document the retry/DLQ policy for message handling.

## Context
Current event DTOs have basic metadata (EventId, EventType) but lack standard fields for observability (timestamp, correlation ID, version). The DLQ policy is implemented but not documented.

## Scope
### In scope
- Add metadata fields to event DTOs (Timestamp, CorrelationId, Version)
- Create base event envelope record
- Document retry/DLQ policy in architecture docs
- Validate metadata presence in projector

### Out of scope
- Changing outbound event publishing format
- Adding retry logic (DLQ is already configured)

## Requirements
- R1: Event envelope must include: EventId, EventType, Timestamp, CorrelationId (optional), Version
- R2: Projector should log metadata for traceability
- R3: DLQ policy documented in architecture docs

## Acceptance Criteria
- [x] Event envelope with standard metadata created
- [x] Projector validates and logs metadata
- [x] DLQ policy documented
- [x] Build passes
- [x] All 93 tests pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (93 tests)
- [x] Walkthrough updated
