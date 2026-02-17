# Audit Services Workflow

## Overview

This workflow performs a comprehensive audit of clean code, security, and performance for each of the three services in the monorepo. It generates one report per service in `docs/audits/` with scores, findings, and a prioritized action plan. The output feeds into the `fix service audits` workflow.

## Trigger Command

**Command:** `audit services`

When the user issues this command, the agent:
1. Audits each service independently: ai-api, orgs-api, web.
2. Generates a detailed audit report per service.
3. Each report contains a Prioritized Action Plan table consumed by `fix service audits`.

## Operating Mode (Mandatory Autonomy)

The agent must execute this workflow **end-to-end without waiting for user confirmation between steps**. Only stop and ask the user if there is a hard blocker that cannot be resolved locally.

## Workflow Steps

### Step 0: Verify Clean Working Tree

1. Run `git status` to ensure no uncommitted changes.
2. If there are uncommitted changes, warn the user and ask whether to proceed.

### Step 1: Audit Service — ai-api

Service path: `services/ai-api/`

Launch parallel explorations for the following sub-analyses:

#### 1a. Clean Code Analysis
- Read ALL source files in `services/ai-api/src/`.
- Verify **Clean Architecture compliance**:
  - Domain layer has zero framework dependencies.
  - Application layer does not import Infrastructure or Api.
  - Infrastructure does not import Api.
  - Dependencies flow inward only.
- Check **CQRS pattern adherence**: command/query handler naming, separation of read/write models.
- Evaluate **naming conventions**: PascalCase for public members, camelCase for locals.
- Check for **dead code**: unused classes, unreferenced methods, commented-out blocks.
- Evaluate **SOLID compliance**: single responsibility of handlers, interface segregation, dependency inversion.
- Check **DRY violations**: duplicated logic across handlers, duplicated DTOs.
- Evaluate **error handling**: typed exceptions, no bare `catch` blocks, proper `CancellationToken` propagation.
- Check **input validation**: FluentValidation presence and completeness for all request DTOs.
- Evaluate **controller size**: controllers should be thin (delegation to MediatR only).

#### 1b. Security Audit (OWASP Top 10)
Analyze all security-sensitive code:

| OWASP | Check |
|-------|-------|
| **A01 Broken Access Control** | Missing `[Authorize]` attributes, missing policy checks, endpoint protection gaps, tenant isolation bypass vectors |
| **A02 Cryptographic Failures** | JWT configuration, key management, API key storage, sensitive data in responses |
| **A03 Injection** | Raw SQL queries, ORM misuse, command injection vectors, `EF.Functions.ILike` wildcard escaping |
| **A04 Insecure Design** | Missing rate limiting, resource exhaustion vectors (unbounded pagination, large payloads) |
| **A05 Security Misconfiguration** | CORS settings, debug endpoints in production, verbose error responses, missing security headers |
| **A06 Vulnerable Components** | Run `dotnet list package --vulnerable` on the solution, check transitive dependencies |
| **A07 Auth Failures** | Token expiry configuration, refresh patterns, session management |
| **A08 Data Integrity Failures** | Deserialization safety (`JsonStringEnumConverter` usage), unsigned/unvalidated data |
| **A09 Logging Failures** | Sensitive data in logs (passwords, tokens, PII), insufficient audit trails |
| **A10 SSRF** | Outbound HTTP calls (OpenAI), URL validation, request forgery vectors |

Classify each finding:
| Severity | Criteria |
|----------|----------|
| CRITICAL | Exploitable in production, data breach risk, session hijacking |
| HIGH | Security gap requiring attacker sophistication, brute force vectors |
| MEDIUM | Defense-in-depth improvements, missing validation bounds |
| LOW | Hardening measures, logging improvements, minor enhancements |

#### 1c. Performance Analysis
- **N+1 queries:** Check EF Core queries for `Include`/`ThenInclude` patterns; look for DB calls inside loops.
- **Missing indexes:** Cross-reference query `Where` clauses against entity configurations and migrations.
- **Connection pooling:** Verify `DbContext` lifetime (scoped, not singleton), `IHttpClientFactory` usage.
- **Caching:** Evaluate Redis usage patterns, cache key design, cache invalidation correctness.
- **Async patterns:** Check for blocking calls (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`), proper `CancellationToken` propagation through the entire call chain.
- **Serialization:** JSON serializer configuration, use of source generators, `JsonStringEnumConverter` placement.
- **Message processing:** Outbox dispatcher polling interval, projection throughput, dead-letter handling.

#### 1d. Test Coverage Analysis
- Read ALL test files in `services/ai-api/tests/`.
- Identify gaps:
  - Handlers without tests (commands, queries).
  - Missing negative/error path tests.
  - Missing security scenario tests (auth, tenant isolation).
  - Missing edge case coverage.
- Evaluate test quality:
  - Naming conventions (`Method_Scenario_ExpectedResult`).
  - Arrange-Act-Assert pattern.
  - Proper mocking (NSubstitute) vs real dependencies (Testcontainers).
  - Test isolation.
- Reference `.agent/rules/testing-standards.md` for coverage targets:
  - Domain >90%, Application >80%, Infrastructure >60%, Presentation >50%.

#### 1e. Generate Report
Create `docs/audits/service-ai-api-audit.md` using the template at `.agent/templates/service-audit-template.md`.

---

### Step 2: Audit Service — orgs-api

Service path: `services/orgs-api/`

Execute the same sub-steps (2a–2e) as Step 1, targeting `services/orgs-api/`.

Additional orgs-api-specific checks:
- **Authentication flows:** Registration, login, password reset, email verification, OAuth (Google, GitHub).
- **Authorization middleware:** Policy-based auth, role checks, org membership enforcement.
- **Billing integration:** Stripe webhook signature verification, idempotency, webhook retry handling.
- **Email sending:** SMTP configuration, template injection vectors, sender spoofing prevention.
- **Rate limiting:** AspNetCoreRateLimit configuration, per-endpoint limits, bypass prevention.

Create `docs/audits/service-orgs-api-audit.md`.

---

### Step 3: Audit Service — web

Service path: `apps/web/`

#### 3a. Clean Code Analysis
- Read ALL source files in `apps/web/src/`.
- Evaluate **component structure**: co-location, separation of concerns, prop drilling depth.
- Check **hook patterns**: custom hook composition, effect dependencies, cleanup.
- Verify **design system compliance** (per `CLAUDE.md` rules):
  - Semantic `primary-*` classes used (never hardcoded `indigo-*`).
  - Correct border radius tokens (`rounded-xl` cards, `rounded-lg` inputs, `rounded-full` badges).
  - Correct layout widths per page type.
  - `bg-surface` for page backgrounds (not `bg-slate-50`).
- Check **TypeScript strictness**: `any` casts, missing type annotations at boundaries, `as` assertions.
- Check for **dead exports/imports**: unused components, unreferenced hooks.
- Evaluate **i18n completeness**: missing translation keys, hardcoded strings.

#### 3b. Security Audit (Frontend-specific)
| Check | Details |
|-------|---------|
| **XSS** | `dangerouslySetInnerHTML` usage, unsanitized user input rendered in JSX |
| **CSRF** | Cookie-based auth token handling, `SameSite` attributes, CSRF token flow |
| **Open Redirects** | URL validation in redirects, `window.location` assignments, `router.push` with user input |
| **Secrets Exposure** | API keys in client bundle, `NEXT_PUBLIC_*` var leakage, source maps in production |
| **Auth Token Storage** | No `localStorage` for tokens, `HttpOnly` cookie verification, credential handling |
| **Dependency Vulnerabilities** | Run `npm audit`, check transitive dependency risks |
| **CSP Readiness** | Inline scripts, `eval()` usage, `unsafe-inline` requirements |

#### 3c. Performance Analysis
- **Unnecessary re-renders:** Missing `memo`/`useMemo`/`useCallback` on expensive computations, prop drilling causing subtree re-renders.
- **Bundle size:** Dynamic imports for large components, tree shaking effectiveness, large dependency analysis.
- **Image optimization:** Next.js `Image` component usage, unoptimized `<img>` tags.
- **Data fetching patterns:** Waterfall requests, missing parallel fetching, stale-while-revalidate patterns.
- **Layout shifts (CLS):** Components that change size after load, skeleton/placeholder patterns.
- **Route-level code splitting:** Page-level dynamic imports, shared chunk sizes.

#### 3d. Test Coverage Analysis
- Read ALL test files in `apps/web/src/`.
- Identify untested components, hooks, utilities.
- Check coverage of:
  - Form validation paths.
  - Error state rendering.
  - Auth-protected route behavior.
  - i18n string rendering.
- Reference testing-standards.md targets: Presentation >50%.

#### 3e. Generate Report
Create `docs/audits/service-web-audit.md`.

---

### Step 4: Report to User

After generating all three reports:

1. **Print a summary table:**

```
| Service  | Clean Code | Security | Performance | Tests | Overall |
|----------|-----------|----------|-------------|-------|---------|
| ai-api   |       /100 |     /100 |        /100 |  /100 |    /100 |
| orgs-api |       /100 |     /100 |        /100 |  /100 |    /100 |
| web      |       /100 |     /100 |        /100 |  /100 |    /100 |
```

2. **Print action item totals per priority:**

```
| Priority | ai-api | orgs-api | web | Total |
|----------|--------|----------|-----|-------|
| P0       |        |          |     |       |
| P1       |        |          |     |       |
| P2       |        |          |     |       |
| P3       |        |          |     |       |
```

3. Remind the user to use `fix service audits` to process all items.

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Service audit | `service-<name>-audit.md` | `service-ai-api-audit.md` |

## Quality Gates

Each service audit report must meet these standards:
- Every security finding has a file path and line reference.
- Every action plan item has a clear, implementable description.
- Scores are justified by the findings (not arbitrary).
- Positive findings are documented alongside issues.
- No vague recommendations ("improve security" is not actionable).
- Performance findings include specific file/query references.

## Related Documents

- [Fix Service Audits Workflow](fix-service-audits-workflow.md) — Consumes the action plans.
- [Service Audit Template](../templates/service-audit-template.md) — Template for each report.
- [Architecture Standards](architecture-standards.md)
- [Coding Standards](coding-standards.md)
- [Testing Standards](testing-standards.md)
