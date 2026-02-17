# Audit Epic Workflow

## Overview

This workflow performs a comprehensive audit of a completed epic. It analyzes documentation, code, commits, tests, architecture compliance, and security posture. The output is a detailed executive summary with a prioritized action plan that feeds into the `next audit action` workflow.

## Trigger Command

**Command:** `audit epic <number>`

Example: `audit epic 002`

When the user issues this command, the agent:
1. Locates the epic definition and all related tasks.
2. Analyzes code, commits, tests, and documentation.
3. Generates a detailed executive summary in `docs/audits/`.
4. The summary contains a Prioritized Action Plan table consumed by `next audit action`.

## Workflow Steps

### Step 0: Locate the Epic

1. Find the epic definition file: `docs/backlog/epic-<NNN>-*.md`.
2. Parse the **Task List** section to identify all tasks (IDs, status, file paths).
3. If any task is not `DONE`, warn the user and ask whether to proceed with a partial audit.

### Step 1: Gather Context (Parallel Exploration)

Launch parallel explorations to collect data efficiently. Aim for breadth and depth.

#### 1a. Documentation Analysis
- Read the epic definition (`docs/backlog/epic-<NNN>-*.md`).
- Read ALL task files referenced in the epic (`docs/tasks/<NNNN>-*.md`).
- Read ALL matching walkthroughs (`docs/walkthroughs/<NNNN>-*.md`).
- Check: Are walkthroughs aligned with task specs? Are DOD checkboxes marked? Are there discrepancies?

#### 1b. Code & Architecture Analysis
For each service modified by the epic:
- Read ALL files created or modified by the epic tasks.
- Verify Clean Architecture compliance:
  - Domain layer has zero framework dependencies.
  - Application layer does not import Infrastructure or Api.
  - Infrastructure does not import Api.
  - Dependencies flow inward only.
- Check CQRS pattern adherence (commands vs queries, handler naming).
- Evaluate code quality:
  - Naming conventions (.NET PascalCase, TS camelCase).
  - Error handling patterns (typed exceptions, no bare catch blocks).
  - Input validation (FluentValidation, Zod schemas).
  - DRY violations, controller size, SOLID compliance.

#### 1c. Security Audit
Analyze security-sensitive code with focus on:
- **Authentication:** Cookie security (HttpOnly, Secure, SameSite), token handling, session management.
- **Authorization:** Access control, endpoint protection, middleware ordering.
- **Cryptography:** Password hashing (algorithm, work factor), JWT (algorithm, key strength, expiry, claims).
- **Input validation:** Injection vectors (SQL, XSS, command injection), max lengths, email format.
- **Information leakage:** Error messages, timing attacks, email enumeration.
- **Rate limiting:** Endpoint-specific limits, bypass prevention.
- **CSRF protection:** Token generation, validation, double-submit pattern.
- **OAuth security:** State parameter, provider binding, email verification.
- **Frontend security:** No dangerouslySetInnerHTML, no localStorage for tokens, proper redirect validation.

Classify each finding:
| Severity | Criteria |
|----------|----------|
| CRITICAL | Exploitable in production, data breach risk, session hijacking |
| HIGH | Security gap requiring attacker sophistication, brute force vectors |
| MEDIUM | Defense-in-depth improvements, missing validation bounds |
| LOW | Hardening measures, logging improvements, minor enhancements |

#### 1d. Test Coverage Analysis
- Read ALL test files related to the epic (unit, integration, architecture, frontend).
- Identify coverage gaps:
  - Which handlers/endpoints lack tests?
  - Are negative/error paths tested?
  - Are security scenarios tested (CSRF rejection, rate limiting, token expiry)?
  - Are edge cases covered?
- Evaluate test quality:
  - Naming conventions (`Method_Scenario_Result`).
  - Arrange-Act-Assert pattern.
  - Proper mocking (NSubstitute) vs real dependencies.
  - Test isolation.

#### 1e. Commit History Analysis
```bash
git log --oneline --all --grep="<task-numbers>"
```
- Analyze commit patterns (conventional commits compliance).
- Check for code review cycles (fix/cr- commits).
- Verify branch naming conventions.
- Look for force pushes or unusual patterns.

### Step 2: Cross-Reference Findings

After gathering all data:
1. Verify each task's acceptance criteria against actual implementation.
2. Cross-check walkthrough claims against code reality.
3. Identify any undocumented changes or deviations from task specs.
4. Note anything done well (positive findings are important context).

### Step 3: Generate the Executive Summary

Create the file: `docs/audits/epic-<NNN>-executive-summary.md`

Use the template at `.agent/templates/audit-executive-summary-template.md`.

The executive summary MUST include:
1. **Audit Metadata** — Epic reference, date, scope, task count.
2. **Scores Table** — Numeric scores (0-100) for each audit dimension.
3. **Security Findings** — Every finding with severity, file path, line number, description, recommendation.
4. **Architecture Compliance** — Layer violations, pattern adherence, positive observations.
5. **Code Quality** — Issues and strengths by service/layer.
6. **Test Coverage** — Gap analysis with specific missing test descriptions.
7. **Documentation** — Consistency issues, DOD mismatches, missing content.
8. **Commit History** — Pattern compliance, review cycles.
9. **What's Done Well** — Explicit recognition of good practices (critical for balanced audit).
10. **Prioritized Action Plan** — The table that `next audit action` consumes.

### Step 4: Generate the Prioritized Action Plan

The action plan table MUST follow this exact format for `next audit action` compatibility:

```markdown
## Prioritized Action Plan

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | CRITICAL | <short-title> | <1-2 sentence description> | `path/to/file.cs:line` | None | Not started |
| 2 | P0 | HIGH | <short-title> | <1-2 sentence description> | `path/to/file.cs:line` | None | Not started |
```

**Priority rules:**
- **P0 (Before production):** CRITICAL and HIGH severity security issues.
- **P1 (Next sprint):** MEDIUM security issues, missing critical tests, significant code quality issues.
- **P2 (Short term):** LOW security issues, test coverage gaps, documentation fixes.
- **P3 (Backlog):** Enhancements, refactoring suggestions, nice-to-have improvements.

**Status values:**
- `Not started` — No audit action created yet.
- `In progress` — Audit action task exists and is being worked on.
- `Done` — Audit action completed and verified.

### Step 5: Report to User

After generating the executive summary:
1. Print a concise summary of findings to the user (scores table + critical items).
2. Tell the user how many action items were generated per priority level.
3. Remind them to use `next audit action` to process items sequentially.

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Executive summary | `<epic-number>-executive-summary.md` | `epic-002-executive-summary.md` |

## Quality Gates

The executive summary itself must meet these standards:
- Every security finding has a file path and line reference.
- Every action plan item has a clear, implementable description.
- Scores are justified by the findings (not arbitrary).
- Positive findings are documented alongside issues.
- No vague recommendations ("improve security" is not actionable).

## Related Documents

- [Audit Action Workflow](audit-action-workflow.md) — Consumes the action plan.
- [Audit Action Template](../templates/audit-action-template.md) — Template for individual actions.
- [Executive Summary Template](../templates/audit-executive-summary-template.md) — Template for the audit report.
- [Architecture Standards](architecture-standards.md) — Reference for architecture compliance checks.
- [Coding Standards](coding-standards.md) — Reference for code quality checks.
- [Testing Standards](testing-standards.md) — Reference for test coverage analysis.
