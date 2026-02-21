# Code Review Workflow

## Overview

This document defines the process for performing code reviews on pull requests. The workflow has two complementary modes:

1. **Independent Agent Review** -- The agent performs its own thorough review of the PR diff, checking against project standards, architecture rules, security baseline, and code quality.
2. **GitHub Comment Resolution** -- The agent processes review comments left by human reviewers or automated tools.

Both modes run in every code review pass. The agent's own review catches issues that reviewers may miss, while comment resolution ensures all feedback is addressed.

## Trigger

The workflow is invoked when:
- User asks to review a PR
- User asks to address PR comments/feedback
- User shares a PR link for review

## Workflow Steps

### Operating Mode (Mandatory Autonomy)

During `code review`, the agent must execute this workflow **end-to-end without waiting for user confirmation between steps**.

- The agent must perform its own independent review AND process all reviewer comments.
- The agent must decide what applies vs. what does not apply.
- The agent must implement all applicable fixes immediately.
- The agent must document rejected/non-applicable items with rationale in the `cr-*` task file.
- The agent must run required validations and commit the fixes directly when checks pass.

Only stop and ask the user if there is a hard blocker that cannot be resolved locally (for example: missing credentials, external system outage, or contradictory repo rules).

---

## Phase A: Independent Agent Review

### Step A.1: Fetch PR Diff and Context

```bash
gh pr view <PR_NUMBER> --json title,body,files,state,statusCheckRollup,baseRefName,headRefName
gh pr diff <PR_NUMBER>
```

Read the full diff and understand:
- What services/apps are affected
- The scope and intent of the changes (read the PR description and linked task files)

### Step A.2: Review Against Project Standards

Review the full diff systematically against these checklists. For each category, note any findings as issues with severity (`MUST_FIX`, `SHOULD_FIX`, `NIT`).

#### A.2.1: Architecture & Clean Architecture

- [ ] Domain layer has zero framework dependencies (no EF Core, no ASP.NET, no external packages)
- [ ] Dependencies flow inward only: Presentation -> Application -> Domain; Infrastructure -> Application
- [ ] No business logic in controllers (thin controllers delegating to MediatR handlers)
- [ ] CQRS separation: commands mutate state, queries are read-only with no side effects
- [ ] One handler per command/query (no mega-handlers)
- [ ] Repository interfaces defined in Domain/Application, implementations in Infrastructure
- [ ] No circular project references
- [ ] Correct namespace convention: `Propely.<Service>.<Layer>`

#### A.2.2: Domain Design (DDD)

- [ ] Aggregates enforce their own invariants (no external code setting invalid state)
- [ ] Value objects are immutable and self-validating
- [ ] Domain events named in past tense with version suffix (`SomethingHappenedV1`)
- [ ] Domain events include: `EventId`, `OccurredAtUtc`, `CorrelationId`, `CausationId`, `Producer`, `SchemaVersion`
- [ ] Rich domain model where business rules exist (not anemic)
- [ ] Entity identity handled correctly (GUID-based IDs)

#### A.2.3: API & Security

- [ ] Input validation at the edge (FluentValidation on request DTOs)
- [ ] Authorization via permission-based policies, not hardcoded role checks
- [ ] No secrets or credentials in code, config files, or comments
- [ ] Parameterized queries / ORM patterns (no raw SQL concatenation)
- [ ] Security headers configured (X-Content-Type-Options, HSTS, etc.)
- [ ] Rate limiting applied where appropriate
- [ ] Multi-tenancy: `X-Tenant-Id` header propagated correctly via `TenantDelegatingHandler`
- [ ] No PII in logs

#### A.2.4: Data & Persistence

- [ ] Database constraints enforced at DB level (FKs, unique indexes, not-null)
- [ ] Migrations used for schema changes (no manual DB drift)
- [ ] Redis cache entries have TTL
- [ ] Cache used as optimization, not source of truth
- [ ] Outbox pattern used when publishing events after DB writes

#### A.2.5: Testing

- [ ] TDD followed: tests exist for new behavior (not just after-the-fact)
- [ ] Test naming follows `MethodName_Scenario_ExpectedResult` convention
- [ ] AAA pattern (Arrange-Act-Assert) used consistently
- [ ] Happy path AND error paths covered
- [ ] No logic in tests (no if/loops)
- [ ] Domain coverage target >90%, Application >80%, Infrastructure >60%, API >50%
- [ ] Integration tests use `WebApplicationFactory` and Testcontainers
- [ ] Frontend tests use React Testing Library (not implementation details)

#### A.2.6: Frontend (if applicable)

- [ ] Semantic `primary-*` classes used (no hardcoded color values)
- [ ] Page background uses `bg-surface` (not `bg-slate-50`)
- [ ] Correct border radius: cards `rounded-xl`, inputs/buttons `rounded-lg`, badges `rounded-full`
- [ ] Correct layout widths: auth `max-w-md`, dashboard `max-w-4xl`, detail `max-w-5xl`
- [ ] Focus rings applied correctly on interactive elements
- [ ] Stitch design exists for new screens and implementation matches pixel-perfect

#### A.2.7: Inter-Service Communication

- [ ] NuGet SDK clients used (Refit interface + DTOs + DI extension)
- [ ] Polly resilience policies configured (retry + circuit breaker)
- [ ] Tenant context propagated via `TenantDelegatingHandler`
- [ ] SDK contracts match the actual API endpoints

#### A.2.8: Code Quality

- [ ] No dead code, unused imports, or commented-out code
- [ ] No TODO/HACK/FIXME left without a linked task
- [ ] PascalCase for public members, camelCase for locals (.NET)
- [ ] camelCase for variables/functions, PascalCase for components/types (TypeScript)
- [ ] No over-engineering (YAGNI)
- [ ] No drive-by refactors unrelated to the PR scope
- [ ] Error handling is appropriate (not swallowing exceptions silently)

### Step A.3: Behavioral Parity Checks (Automatic)

Run these checks against the full PR diff even if no reviewer mentioned them. If any check fails, classify it as **MUST_FIX**.

1. **Redirect parity (`next` propagation)**
   - For auth entry points, verify `next` is parsed, sanitized, and applied consistently.

2. **Locale source correctness**
   - Verify locale is derived from explicit user/context input where available, not exclusively `Accept-Language`.

3. **API-to-UI contract parity**
   - For each new/changed request DTO field in API, verify frontend payloads are updated.
   - For each new frontend form field, verify backend contract/validation supports it.

4. **Test parity for behavior changes**
   - New/changed API behavior must have integration coverage (happy + error paths).
   - New/changed form behavior must have frontend tests (success + validation paths).

### Step A.4: Compile Agent Findings

Produce a findings list, each with:
- **File and line** (or general area)
- **Severity**: `MUST_FIX` | `SHOULD_FIX` | `NIT`
- **Category**: Architecture, Security, Testing, DDD, Code Quality, etc.
- **Description**: What the issue is and why it matters
- **Suggested fix**: How to resolve it

---

## Phase B: GitHub Comment Resolution

### Step B.1: Fetch ALL Review Comments (CRITICAL)

**You MUST fetch from all three sources.** Different reviewers post comments in different locations.

```bash
# Source 1: Review Comments (inline comments on specific code lines)
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments

# Source 2: Reviews (general review bodies and approval/rejection)
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/reviews

# Source 3: Issue Comments (general PR discussion, not tied to code)
gh pr view <PR_NUMBER> --json comments
```

#### Step B.1.1: Verify Comment Counts (MANDATORY)

After fetching, you MUST verify the total count from each source:

```bash
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments --jq "length"
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments --jq ".[] | {id: .id, user: .user.login, path: .path, line: .line, body: .body[0:100]}"
```

**Before proceeding, confirm:**
- [ ] Total count of inline review comments: ___
- [ ] Total count of general reviews: ___
- [ ] Total count of issue comments: ___

#### Step B.1.2: Handle Truncated Output (CRITICAL)

**If the Bash tool output shows `<persisted-output>` with a truncated message:**

1. The tool will provide a file path where the full output was saved
2. **You MUST use the Read tool** to read the COMPLETE output from that file
3. **DO NOT proceed** until you have verified you can see ALL comments

### Step B.2: Analyze ALL Comments Critically

**CRITICAL: Process EVERY single comment. Do not stop after reading the first few.**

For EACH comment from ALL sources:

1. **Understand the claim**: What is the reviewer actually saying?
2. **Verify against project standards**: Does the suggestion align with documented standards?
3. **Assess correctness**: Is the reviewer's claim technically accurate?
4. **Make your own decision**: Reviewer suggestions are input, not instructions.

**Processing Order:**
1. Read ALL inline review comments (Source 1)
2. Read ALL general review bodies (Source 2) -- check EACH review's body for all issues
3. Read ALL issue comments (Source 3)

> Do not blindly follow reviewer suggestions. Automated reviewers may suggest changes that conflict with project-specific decisions. Always cross-reference with `.agent/rules/`, `.agent.md`, and existing architecture.

**Common Mistake:** Reading only the first issue from a review body that contains multiple numbered issues. Always read the ENTIRE body of each review to find ALL issues mentioned.

---

## Phase C: Consolidate & Execute

### Step C.0: Create the `cr-NNNN` Task + Walkthrough Files (Required)

Before making any code changes, create both files:

```
docs/tasks/cr-NNNN-<short-slug>.md
docs/walkthroughs/cr-NNNN-<short-slug>.md
```

The `cr-*` task must include:
- PR metadata (link/ID, target branch, CI status summary)
- Changed files list
- **Section 1: Agent Review Findings** (from Phase A)
- **Section 2: Review Comment Threads** (from Phase B, unresolved first)
- The "Resolution Plan" checklist (grouped by classification, merging both sources)

The `cr-*` walkthrough must include:
- Task reference and PR reference
- What changed to address agent findings and reviewer feedback
- Commands run and validation results
- Any process deviations and corrective actions

#### How to Pick `NNNN` and `<short-slug>`

1. **Determine `NNNN`**: Scan `docs/tasks/` for existing `cr-*-*.md` files
   - Extract the numeric prefix from each
   - Use the next sequential number

2. **`<short-slug>`**: Must be kebab-case and short

#### Hard Gate (Do Not Proceed Without Artifacts)

Before proceeding to Step C.1, verify both files exist. If either file is missing, STOP and create it first. Do not start code edits or fixes before this gate passes.

### Step C.1: Classify All Findings

Merge agent findings (Phase A) and reviewer comments (Phase B) into a single classified list:

| Classification | Criteria | Action Required |
|----------------|----------|-----------------|
| **MUST_FIX** | Blocking merge, correctness issue, security vulnerability | Must address before merge |
| **SHOULD_FIX** | Maintainability, clarity, minor bug risk | Implement if low-risk and within scope |
| **SUGGESTION** | Style, nit, personal preference | Implement quickly or respond with rationale |
| **QUESTION** | Needs clarification | Respond with explanation |
| **OUT_OF_SCOPE** | Conflicts with task scope/roadmap | Explain why and defer to future task |
| **FALSE_POSITIVE** | Incorrect finding (reviewer or agent error) | Document rationale, skip |

### Step C.2: Produce the Resolution Plan

Create a Markdown checklist in the task file grouped by classification and source (Agent / Reviewer).

### Step C.3: Execute

1. **MUST_FIX**: Implement all items. These are blocking.
2. **SHOULD_FIX**: Implement when low-risk and within scope.
3. **SUGGESTION**: Implement quickly if trivial, or respond with rationale.
4. **QUESTION**: Respond with clear explanation.
5. **OUT_OF_SCOPE**: Respond explaining why and reference future task if applicable.
6. **FALSE_POSITIVE**: Document rationale in the task file.

**Autonomous application policy (mandatory):**
- Do not ask the user for permission to apply an item that is technically valid and within scope.
- Decide and act in the same pass: classify -> implement -> validate -> document.
- For non-applicable comments, add explicit rationale in the task file and continue.

#### Rules

- **Never mark a thread as resolved without addressing it**
- **Keep changes minimal** and aligned with current task/roadmap
- **Avoid unrelated refactors**
- **Update tests** if code changes affect behavior
- **Update walkthrough** with changes made during review

### Step C.4: Commit and Update

```bash
git add <specific-files>
git commit -m "fix(scope): address PR review feedback (#cr-NNNN)"
git push
```

**Commit policy (mandatory):**
- After validations pass, create the commit directly without asking "should I commit?".
- If fixes span independent concerns, create multiple small commits; otherwise use one atomic commit.
- If validation fails, keep iterating until fixed or until a real blocker is identified and documented.

Before committing, verify the artifact pair is included:
- [ ] `docs/tasks/cr-NNNN-<short-slug>.md` exists and is updated
- [ ] `docs/walkthroughs/cr-NNNN-<short-slug>.md` exists and is updated
- [ ] Both files are included in `git status`

### Step C.5: Reply to Reviewer Comments

For each addressed reviewer comment:
- Reply on GitHub indicating the fix
- Reference the commit if helpful
- Mark as resolved only after reply

For agent-originated findings, no GitHub reply is needed (they are documented in the task file).

---

## Phase D: CI Verification, Conflict Resolution, and Merge

**The code review task is NOT complete until ALL CI checks are green and the PR is merged.** This phase is mandatory and cannot be skipped.

### Step D.1: Verify ALL CI Checks Pass (Hard Gate)

After pushing fixes, monitor CI until ALL checks complete:

```bash
gh pr checks <PR_NUMBER> --watch
```

**Rules:**
- **Every CI check must pass.** If any check fails, investigate and fix the root cause.
- **Fix CI proactively, not by lowering standards.** Never reduce coverage thresholds, disable linting rules, skip tests, or remove checks to make CI pass. Apply the boy scout rule: leave the codebase better than you found it.
- If coverage is below the threshold, **add tests** to increase coverage — do not lower the threshold.
- If a linter fails, **fix the code** — do not disable the rule.
- If a test fails, **fix the test or the code** — do not skip the test.
- If a pre-existing CI failure exists on `main` that is unrelated to the PR, **fix it anyway** as part of the boy scout rule.
- Keep iterating (fix → push → watch CI) until all checks are green. There is no limit on the number of iterations.

### Step D.2: Resolve Merge Conflicts with Main

Before merging, ensure the branch is up to date with the target branch:

```bash
git fetch origin main
git rebase origin/main
```

If conflicts arise:
1. Resolve each conflict manually, preserving both the PR changes and the latest main changes.
2. Run the full test suite again to verify nothing is broken.
3. Push the rebased branch: `git push --force-with-lease`.
4. Wait for CI to go green again (repeat Step D.1).

### Step D.3: Merge with Rebase

Once ALL checks are green and there are no conflicts:

```bash
gh pr merge <PR_NUMBER> --rebase --delete-branch
```

Verify the merge succeeded:

```bash
gh pr view <PR_NUMBER> --json state,mergedAt
```

### Completion Criteria

The code review task is **only complete** when:
- [ ] All agent findings and reviewer comments are addressed (Phase C)
- [ ] All CI checks are green (Phase D.1)
- [ ] No merge conflicts with main (Phase D.2)
- [ ] PR is merged with rebase (Phase D.3)

---

## Summary: Review Dimensions

The agent review covers these dimensions in every pass:

| Dimension | Source | Key Checks |
|-----------|--------|------------|
| Architecture | `.agent/rules/architecture-standards.md` | Layer boundaries, dependency direction, CQRS |
| Domain Design | `.agent.md` section 4 | Aggregates, value objects, events, invariants |
| Security | `.agent.md` section 9 | OWASP top 10, auth, input validation, secrets |
| Testing | `.agent/rules/testing-standards.md` | TDD, coverage, naming, AAA pattern |
| Data | `.agent.md` section 7-8 | Migrations, constraints, cache TTL |
| Frontend | `.agent.md` section 14, `CLAUDE.md` | Design system, tokens, Stitch compliance |
| Inter-Service | `.agent.md` section 5.1 | SDK clients, Polly, tenant propagation |
| Code Quality | `.agent.md` section 12 | Naming, dead code, over-engineering |
| Behavioral Parity | Step A.3 | Redirects, locale, API/UI contract, test parity |

## Related Documents

- [Autonomous Workflow](autonomous-workflow.md)
- [Architecture Standards](architecture-standards.md)
- [Testing Standards](testing-standards.md)
- [Task Template](../templates/task-template.md)
