# Code Review: cr-0018 — Global Exception Handler PR #158

## PR Metadata
- PR: #158
- Branch: `feat/0018-global-exception-handler` → `main`
- CI: All checks passed (Detect Changes, AI API Build & Test, Orgs API Build & Test)
- Files changed: 31 (+866, -126)

## Review Sources
- Inline review comments: 10
- General reviews: 3 (gemini-code-assist, chatgpt-codex-connector, copilot)
- Issue comments: 1 (gemini-code-assist summary — no actionable feedback)

## Deduplication (10 inline → 6 unique issues)
- Comments #5 (copilot, orgs-api) and #7 (copilot, ai-api) are identical → Issue B
- Comments #6 (copilot, orgs-api) and #8 (copilot, ai-api) are identical → Issue E
- Comments #9 and #10 (copilot, both controllers) are similar → Issue F
- Comment #1 (gemini) and gemini review body are the same suggestion → Issue A

## Comment Resolution Plan

### MUST_FIX

- [ ] **B: Add HasStarted + RequestAborted guards in HandleExceptionAsync** (codex #2775971648, copilot #2775990397, copilot #2775990431)
  - If response has already started, writing ProblemDetails throws `InvalidOperationException`, hiding the original error.
  - If client disconnected, `WriteAsync` observes `RequestAborted` and throws `OperationCanceledException`.
  - Fix: Check `context.Response.HasStarted` before writing; wrap WriteAsync in try/catch for `OperationCanceledException`.
  - Files: both `ExceptionHandlerMiddleware.cs`

### SHOULD_FIX

- [ ] **C: Fix walkthrough inaccuracy re: ForbiddenException breaking change** (copilot #2775990365)
  - Text says "breaking for code catching Exception specifically" — incorrect. `catch(Exception)` still catches it.
  - The actual breaking change: `catch(DomainException)` and `is DomainException` now also match `ForbiddenException`.
  - Fix: Update walkthrough text.
  - Files: `docs/walkthroughs/0018-global-exception-handler.md`

- [ ] **E: Correlation ID lookup from accessor instead of headers** (copilot #2775990414, copilot #2775990442)
  - Response headers won't have the ID at log time (OnStarting hasn't fired).
  - Request header fallback only works for client-provided IDs.
  - CorrelationIdMiddleware's logging scope already includes the ID in structured logs, but the explicit log parameter will be null for server-generated IDs.
  - Fix: Resolve `ICorrelationIdAccessor` from `HttpContext.RequestServices` to get the ID reliably.
  - Files: both `ExceptionHandlerMiddleware.cs`

### OUT_OF_SCOPE

- [ ] **A: Shared library for common code** (gemini #2775968783)
  - Services are intentionally independent per Clean Architecture. Each service owns its domain layer. A shared library crosses service boundaries and is outside this task's scope.

- [ ] **F: Ad-hoc `{ error = ... }` format in controllers** (copilot #2775990449, copilot #2775990473)
  - Remaining `{ error = ... }` responses are pre-MediatR validation/CSRF guards. Migrating to ValidationProblemDetails is tracked as a follow-up in the walkthrough (FluentValidation pipeline behavior via MediatR).

### SUGGESTION

- [ ] **D: EMAIL_TAKEN/INVALID_CREDENTIALS as ProblemDetails.detail** (copilot #2775990386)
  - These are intentional domain error codes. The frontend parses them for localized messages. The walkthrough documents this as designed behavior. No change needed.
