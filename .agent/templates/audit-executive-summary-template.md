# Audit Executive Summary: Epic <NNN> — <Title>

## Audit Metadata
- **Epic:** `docs/backlog/epic-<NNN>-<slug>.md`
- **Date:** YYYY-MM-DD
- **Auditor:** Agent
- **Status:** In Progress | Complete
- **Tasks audited:** <count> (<list task IDs>)
- **Services affected:** <list services>
- **Commits analyzed:** <count>

> **Status values:**
> - `In Progress` — There are still `Not started` items in the Prioritized Action Plan.
> - `Complete` — All action plan items are `Done`. The agent will skip this audit when running `next audit action`.

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | /100 | |
| Security (Backend) | /100 | |
| Security (Frontend) | /100 | |
| Code Quality | /100 | |
| Test Coverage | /100 | |
| Documentation | /100 | |
| **Overall** | **/100** | |

---

## Security Findings

### CRITICAL

#### <F#>. <Title>
- **Severity:** CRITICAL
- **Files:** `<path>:<line>`
- **Problem:** <Clear description of the vulnerability.>
- **Impact:** <What an attacker could achieve.>
- **Recommendation:** <Specific fix with code example if helpful.>

### HIGH

#### <F#>. <Title>
- **Severity:** HIGH
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Impact:** <Description.>
- **Recommendation:** <Specific fix.>

### MEDIUM

#### <F#>. <Title>
- **Severity:** MEDIUM
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Recommendation:** <Specific fix.>

### LOW

#### <F#>. <Title>
- **Severity:** LOW
- **Files:** `<path>:<line>`
- **Problem:** <Description.>
- **Recommendation:** <Specific fix.>

---

## Architecture Compliance

### Adherence Score: <X>/100

<Summary of architecture compliance observations.>

### Positive Observations
- <What's done well.>

### Violations / Concerns
- <Any issues found.>

---

## Code Quality

### Backend
**Strengths:**
- <Bullet points.>

**Issues:**
- <Bullet points with file references.>

### Frontend
**Strengths:**
- <Bullet points.>

**Issues:**
- <Bullet points with file references.>

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (handlers) | <count> | <brief gap description or "None"> |
| Integration (endpoints) | <count> | <brief gap description or "None"> |
| Architecture | <count> | <brief gap description or "None"> |
| Frontend | <count> | <brief gap description or "None"> |

### Missing Tests
1. <Specific test description with handler/endpoint and scenario.>

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DOD | Walkthrough | Issue |
|------|----------|-------------|-------|
| <ID> | OK / Incomplete | OK / Incomplete | <Description or "Aligned"> |

### Other Documentation Issues
- <Bullet points.>

---

## Commit History

### Pattern Compliance
- Conventional commits: <Yes/No/Partial>
- Branch naming: <Yes/No/Partial>
- Code review cycles: <Observed/Not observed>

### Observations
- <Bullet points.>

---

## What's Done Well

<Explicit recognition of positive implementation choices, patterns, and practices. This section is mandatory — every audit must acknowledge good work.>

1. <Positive finding with file reference.>

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | CRITICAL | | | | None | Not started |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/<service>/SaasTemplate.<Service>.sln
dotnet test services/<service>/SaasTemplate.<Service>.sln
cd apps/web && npm run build && npm test
```
