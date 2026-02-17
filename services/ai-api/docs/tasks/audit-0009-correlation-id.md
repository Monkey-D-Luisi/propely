# Audit Action: audit-0009-correlation-id

## Metadata
- ID: audit-0009
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-31
- Priority: P3
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Add correlation ID middleware and log enrichment"
- Dependencies: None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0009-correlation-id.md`

## Goal
Add correlation ID middleware to enable end-to-end request tracing across logs and services.

## Context
The executive summary identified that correlation ID middleware and log enrichment would enable end-to-end observability. This is essential for debugging distributed operations and tracing requests through the system.

## Scope
### In scope
- Create correlation ID middleware
- Extract correlation ID from incoming header or generate new one
- Add correlation ID to response headers
- Enrich logs with correlation ID
- Add correlation ID to outgoing HTTP calls (if applicable)

### Out of scope
- Distributed tracing integration (OpenTelemetry already handles trace IDs)
- Database query correlation

## Requirements
- R1: Middleware should extract `X-Correlation-ID` header if present
- R2: If header not present, generate a new GUID
- R3: Add correlation ID to response headers
- R4: Enrich all logs with correlation ID during request scope
- R5: Correlation ID should be accessible via DI

## Acceptance Criteria
- [x] Correlation ID middleware created
- [x] Correlation ID extracted or generated per request
- [x] Response includes correlation ID header
- [x] Logs enriched with correlation ID
- [x] Build passes
- [x] All 90 tests pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (90 tests)
- [x] Walkthrough updated
