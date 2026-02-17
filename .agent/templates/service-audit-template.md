# Service Audit: <service-name>

## Audit Metadata
- **Service:** `<path>` (e.g., `services/ai-api/`, `services/orgs-api/`, `apps/web/`)
- **Date:** YYYY-MM-DD
- **Auditor:** Agent
- **Status:** In Progress | Complete
- **Technology:** <e.g., .NET 10 Clean Architecture + CQRS | Next.js 16 + Tailwind CSS>
- **Source files analyzed:** <count>
- **Test files analyzed:** <count>

> **Status values:**
> - `In Progress` — There are still `Not started` items in the Prioritized Action Plan.
> - `Complete` — All action plan items are `Done`.

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Clean Code | /100 | |
| Architecture | /100 | |
| Security | /100 | |
| Performance | /100 | |
| Test Coverage | /100 | |
| **Overall** | **/100** | |

---

## Security Findings

### CRITICAL

#### F#. <Title>
- **Severity:** CRITICAL
- **OWASP:** <A01–A10 category>
- **Files:** `<path>:<line>`
- **Problem:** <Clear description of the vulnerability.>
- **Impact:** <What an attacker could achieve.>
- **Recommendation:** <Specific fix with code example if helpful.>

### HIGH

#### F#. <Title>
- **Severity:** HIGH
- **OWASP:** <A01–A10 category>
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Impact:** <Description.>
- **Recommendation:** <Specific fix.>

### MEDIUM

#### F#. <Title>
- **Severity:** MEDIUM
- **OWASP:** <A01–A10 category>
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Recommendation:** <Specific fix.>

### LOW

#### F#. <Title>
- **Severity:** LOW
- **OWASP:** <A01–A10 category>
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Recommendation:** <Specific fix.>

---

## Clean Code

### Architecture Compliance

**Score:** <X>/100

<For .NET services: Clean Architecture layer checks, CQRS pattern adherence, dependency direction verification.>
<For web service: Component structure, separation of concerns, design system compliance.>

### Strengths
- <Bullet points with file references.>

### Issues
- <Bullet points with file references.>

### Dead Code / Duplication
- <Specific findings with file references.>

---

## Performance

### Database / Data Access (.NET services only)
- **N+1 queries:** <findings or "None detected">
- **Missing indexes:** <findings or "None detected">
- **Connection pooling:** <findings or "Properly configured">
- **Query optimization:** <findings or "No issues">

### Frontend Performance (web service only)
- **Re-renders:** <findings or "None detected">
- **Bundle size:** <findings or "Acceptable">
- **Image optimization:** <findings or "Properly using Next.js Image">
- **Data fetching patterns:** <findings or "No waterfalls detected">

### Caching
- <Redis/in-memory caching findings.>

### Async Patterns
- <Blocking calls, CancellationToken propagation findings.>

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit | <count> | <brief gap description or "None"> |
| Integration | <count> | <brief gap description or "None"> |
| Architecture | <count> | <brief gap description or "None"> |
| Frontend (components) | <count> | <brief gap description or "None"> |
| Frontend (hooks) | <count> | <brief gap description or "None"> |

### Missing Tests
1. <Specific test description with handler/endpoint/component and scenario.>

---

## What's Done Well

<Explicit recognition of positive implementation choices, patterns, and practices. This section is mandatory — every audit must acknowledge good work.>

1. <Positive finding with file reference.>

---

## Prioritized Action Plan

> This table is consumed by the `fix service audits` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Category | Title | Description | Files | Dependencies | Status |
|---|----------|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | CRITICAL | Security | | | | None | Not started |

**Priority rules:**
- **P0 (Before production):** CRITICAL and HIGH severity security issues.
- **P1 (Next sprint):** MEDIUM security issues, missing critical tests, significant code quality issues.
- **P2 (Short term):** LOW security issues, test coverage gaps, documentation fixes.
- **P3 (Backlog):** Enhancements, refactoring suggestions, nice-to-have improvements.

**Category values:** Security, Clean Code, Performance, Test Coverage

**Status values:**
- `Not started` — Not yet addressed.
- `In progress` — Being worked on.
- `Done` — Completed and verified.

---

## Verification Commands

```bash
# Run after all audit actions are implemented

# .NET service
dotnet build services/<service>/SaasTemplate.<Service>.sln
dotnet test services/<service>/SaasTemplate.<Service>.sln

# Web service
cd apps/web && npm run build && npm test
```
