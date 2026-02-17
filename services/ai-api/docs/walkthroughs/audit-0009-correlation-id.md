# Walkthrough: audit-0009-correlation-id

## Task Reference
- Task: `docs/tasks/audit-0009-correlation-id.md`
- Walkthrough: `docs/walkthroughs/audit-0009-correlation-id.md`
- Branch/PR: `audit-0009-correlation-id`
- Date: `2026-01-31`

## Summary
Adds correlation ID middleware for end-to-end request tracing. Each request gets a unique correlation ID (from header or generated) that is propagated to logs and response headers.

## Context
- Background: Executive summary identified need for observability improvements
- Problem statement: No request-scoped correlation for debugging
- Constraints: Must integrate with existing logging

## Decisions & Trade-offs
- **Decision:** Use `X-Correlation-ID` header name
  - Options considered: (1) X-Correlation-ID, (2) X-Request-ID, (3) Correlation-Id
  - Why this choice: Most common convention, widely recognized
  - Consequences / risks: None

## Implementation Notes
- Key changes:
  - Created CorrelationIdMiddleware
  - Created ICorrelationIdAccessor for DI access
  - Integrated with Serilog log enrichment

## Files Changed
- `src/SaasTemplate.AiApi.Api/Middleware/CorrelationIdMiddleware.cs` — New middleware
- `src/SaasTemplate.AiApi.Api/Services/CorrelationIdAccessor.cs` — Accessor service
- `src/SaasTemplate.AiApi.Api/Program.cs` — Registration and middleware

## Checklist
- [x] Task scope matches audit-0009
- [x] Tests updated and passing (90 tests total, 5 new)
- [x] No secrets committed
