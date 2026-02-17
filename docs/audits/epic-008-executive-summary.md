# Audit Executive Summary: Epic 008 — CI/CD & Release Automation

## Audit Metadata
- **Epic:** `docs/backlog/epic-008-cicd-release.md`
- **Date:** 2026-02-12
- **Auditor:** Agent
- **Status:** Complete
- **Tasks audited:** 3 (0050, 0051, 0052)
- **Services affected:** web, CI/CD workflows (`.github/workflows/`)
- **Commits analyzed:** 10 (on main)

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | 90/100 | Clean workflow separation, correct dependency chains, efficient path filtering |
| Security (Backend) | 85/100 | Minimal permissions, built-in GITHUB_TOKEN, Trivy scanning — but report-only mode |
| Security (Frontend) | 85/100 | Safe test data, no real credentials — but incomplete session clearing in login test |
| Code Quality | 82/100 | Good patterns overall — fragile bash parsing and fixture duplication lower the score |
| Test Coverage | 85/100 | 8 E2E smoke tests + 305 unit tests — gaps in negative E2E scenarios |
| Documentation | 92/100 | All walkthroughs aligned — missing CR-0054 docs for GHCR review |
| **Overall** | **86/100** | **Solid CI/CD foundation with actionable improvements** |

---

## Security Findings

### CRITICAL

_No critical findings._

### HIGH

_No high findings._

### MEDIUM

#### F1. Trivy vulnerability scanning is report-only
- **Severity:** MEDIUM
- **Files:** `.github/workflows/publish.yml:75`
- **Problem:** Trivy runs with `exit-code: 0`, meaning it reports vulnerabilities but never blocks the pipeline. Container images with CRITICAL CVEs can be published to GHCR.
- **Recommendation:** Change to `exit-code: 1` once base image vulnerabilities are triaged. Add an allowlist file for accepted CVEs using `.trivyignore`.

#### F2. Login E2E test does not clear localStorage/sessionStorage
- **Severity:** MEDIUM
- **Files:** `apps/web/e2e/login.spec.ts:8`
- **Problem:** The login test clears cookies but not `localStorage` or `sessionStorage`. If authentication tokens are stored in web storage, the test may pass without actually testing the login flow from a clean state.
- **Recommendation:** Add `await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });` after `clearCookies()` and before navigating to `about:blank`.

### LOW

#### F3. Release workflow has no failure notification
- **Severity:** LOW
- **Files:** `.github/workflows/release.yml:14-42`
- **Problem:** If `semantic-release` fails (e.g., due to token issues, network errors, or plugin misconfiguration), no notification is sent to developers. Release failures can go unnoticed.
- **Recommendation:** Add a failure notification step (GitHub issue comment, Slack webhook, or `actions/github-script` to post a comment on the triggering commit).

#### F4. Release commit message includes full release notes
- **Severity:** LOW
- **Files:** `.releaserc.json:40`
- **Problem:** The git commit message format `chore(release): v${nextRelease.version}\n\n${nextRelease.notes}` embeds full release notes in the commit body. For releases with many changes, this creates very long commit messages that clutter `git log`.
- **Recommendation:** Shorten to `chore(release): v${nextRelease.version} [skip ci]` and rely on the GitHub Release page for detailed notes.

---

## Architecture Compliance

### Adherence Score: 90/100

This epic introduces CI/CD infrastructure (GitHub Actions workflows), not application code. Architecture assessment focuses on workflow design and separation of concerns.

### Positive Observations
- **Clean workflow separation:** Three distinct workflows (`ci.yml`, `publish.yml`, `release.yml`) each with a single responsibility.
- **Correct dependency chaining:** Publish and Release both use `workflow_run` after CI, with `conclusion == 'success'` gate.
- **Efficient path filtering:** `dorny/paths-filter` avoids running unnecessary jobs when only docs/agent files change.
- **Minimal permissions:** Each workflow declares only the permissions it needs (principle of least privilege).
- **Matrix strategy in publish:** DRY approach — one job definition builds all three service images.

### Violations / Concerns
- **Duplicated coverage check logic:** The coverage threshold check (lines 97-110 in `ci.yml`) is copied verbatim between `ai-api` and `orgs-api` jobs. This is a DRY violation that makes maintenance harder — a change to the threshold or parsing logic must be applied twice.
- **Always-run condition on push:** The `if` condition on `ai-api` and `orgs-api` jobs includes `github.event_name == 'push'`, which forces both jobs to run on every push to main regardless of path filters. This may be intentional (for safety before release) but contradicts the purpose of the `changes` job.

---

## Code Quality

### Backend
**Strengths:**
- NuGet vulnerability scanning in CI catches known CVEs before merge.
- Coverage threshold enforcement (60% minimum) prevents regression.
- `.NET` build and test run in Release configuration (matching production).

**Issues:**
- `.github/workflows/ci.yml:105-110` — Fragile coverage parsing using `grep -oP` + `bc -l`. If ReportGenerator's output format changes or the line doesn't exist, `$line_coverage` will be empty and `bc` will silently fail. No error handling around the extraction.
- `.github/workflows/ci.yml:97-110` vs `174-187` — Identical coverage check blocks duplicated between ai-api and orgs-api. Should be extracted to a composite action or reusable workflow.

### Frontend
**Strengths:**
- Label-based selectors (`getByLabel`, `getByRole`) follow Playwright best practices — resilient to styling changes.
- Unique test data per run (`Date.now()` + random suffix) avoids test data collisions.
- `registerUser()` helper extracted after code review — good DRY practice.
- `createOrg()` helper shared across specs.
- `eslint.config.mjs` correctly excludes `e2e/` from React hook rules.

**Issues:**
- `apps/web/e2e/fixtures/auth.fixture.ts:37-44` — `registeredPage` and `authenticatedPage` fixtures are functionally identical. Both call `registerUser()` and return the same structure. The distinction adds cognitive overhead without behavioral difference.
- `apps/web/e2e/invite-member.spec.ts:21-23` — Scoping to `<section>` element via `.filter({ has: ... })` is fragile if the HTML structure changes (e.g., if `<section>` becomes `<div>`). Consider `data-testid` for better isolation.

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (frontend - Vitest) | 305 | None identified in this epic |
| Unit (backend - xUnit) | Existing | None identified in this epic |
| E2E (Playwright) | 8 | No duplicate-registration test, no notification bell test, incomplete session clearing |
| Architecture | N/A | N/A — no application code changed |

### Missing Tests
1. **Duplicate registration E2E test** — No test verifies what happens when a user tries to register with an already-existing email. This is a common error path that should be covered.
2. **Notification bell E2E test** — Task 0052 scope mentions "notification bell" but it was not implemented as a smoke test. Documented as a potential follow-up.

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DOD | Walkthrough | Issue |
|------|----------|-------------|-------|
| 0050 | 3/5 checked (2 pending CI) | Complete | Missing CR-0054 task/walkthrough |
| 0051 | 2/5 checked (3 pending CI) | Complete | Aligned |
| 0052 | 3/6 checked (3 pending CI) | Complete | Aligned |

### Other Documentation Issues
- **CR-0054 does not exist:** Task 0050 (GHCR publishing) had PR #253 reviewed, and commit `f4adeea` references `#cr-0054`, but no `docs/tasks/cr-0054-*.md` or `docs/walkthroughs/cr-0054-*.md` files exist. This breaks the project's documentation convention.
- **DoD items "pending first merge to main":** All three tasks have unchecked DoD items that depend on CI runs on the `main` branch. These were merged to main; the items should be re-verified and checked off.

---

## Commit History

### Pattern Compliance
- Conventional commits: **Yes** — All commits follow `type(scope): message` format.
- Branch naming: **Yes** — `feat/0050-ghcr-publishing`, `feat/0051-semantic-release`, `feat/0052-e2e-smoke-tests`.
- Code review cycles: **Observed** — Clear pattern of `feat/test(scope): ...` followed by `fix(scope): address PR #NNN review feedback (#cr-NNNN)`.

### Observations
- 10 commits on main for this epic: 3 feature/test commits, 1 auto-release (`v1.0.0`), 6 fix commits from code reviews and CI debugging.
- All 3 PRs merged (#253, #255, #256) with proper review cycles.
- CR-0056 required 4 follow-up fix commits due to CI failures (ESLint false positives, Docker permissions, test flakiness). This is a higher-than-usual fix count but reflects the complexity of E2E testing in CI.
- Auto-release `v1.0.0` was generated by semantic-release after the first feature merge — confirming the release pipeline works.

---

## What's Done Well

1. **Workflow dependency chaining via `workflow_run`** — Native GitHub Actions feature used instead of third-party wait-on-check actions. Clean and reliable. (`.github/workflows/publish.yml:4-7`, `.github/workflows/release.yml:4-7`)

2. **Matrix strategy for Docker publishing** — Single job definition builds all three service images with consistent tagging and scanning. DRY and maintainable. (`.github/workflows/publish.yml:21-30`)

3. **Label-based Playwright selectors** — Tests use `getByLabel()` and `getByRole()` instead of brittle CSS selectors or `data-testid`. Aligned with Playwright best practices and the application's `<label htmlFor>` patterns. (`apps/web/e2e/fixtures/auth.fixture.ts:17-21`)

4. **Unique test data generation** — Every test run creates unique emails and org names with `Date.now()` + random suffix. No cleanup needed; CI destroys Docker volumes. (`apps/web/e2e/fixtures/auth.fixture.ts:28-29`)

5. **Iterative CI debugging** — E2E tests required 4 fix iterations to resolve ESLint false positives, Docker permission conflicts, strict mode violations, and navigation timing issues. Each was diagnosed and fixed systematically.

6. **Comprehensive code reviews** — CR-0055 and CR-0056 produced meaningful improvements: breaking change rule added, `persist-credentials` fixed, `workflow_run` gating, helper extraction, health check URL fix, ESLint exclusion.

7. **Semantic-release configuration** — Correct plugin ordering, `conventionalcommits` preset, hidden non-user-facing commit types in changelog, `npmPublish: false` for monorepo. (`.releaserc.json`)

8. **CI artifact uploads** — All jobs upload test results, coverage reports, and Playwright reports with 7-day retention and `if: always()` to capture even on failure.

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P1 | MEDIUM | Tighten Trivy to block on vulnerabilities | Change `exit-code` from `0` to `1` and add `.trivyignore` for accepted CVEs so CRITICAL/HIGH vulnerabilities block image publishing. | `.github/workflows/publish.yml:75` | None | Done |
| 2 | P1 | MEDIUM | Clear all storage in login E2E test | Add `localStorage.clear()` and `sessionStorage.clear()` after `clearCookies()` in the login test to ensure complete session isolation. | `apps/web/e2e/login.spec.ts:8-9` | None | Done |
| 3 | P2 | LOW | Add release failure notification | Add a step to `release.yml` that creates a GitHub issue or posts a commit comment when `semantic-release` fails, so failures don't go unnoticed. | `.github/workflows/release.yml:39-42` | None | Done |
| 4 | P2 | LOW | Shorten release commit message | Remove `${nextRelease.notes}` from the git commit message in `.releaserc.json` to avoid cluttering `git log` with full changelogs. | `.releaserc.json:40` | None | Done |
| 5 | P2 | LOW | Harden coverage threshold parsing | Replace fragile `grep -oP` + `bc` coverage parsing with a more robust approach (e.g., `awk` with error handling or a dedicated coverage gate action). Extract to reusable workflow to eliminate duplication. | `.github/workflows/ci.yml:105-110`, `.github/workflows/ci.yml:182-187` | None | Done |
| 6 | P2 | LOW | Create missing CR-0054 documentation | ~~Create docs~~ Files already exist as `cr-0054-ghcr-publish-review.md` (audit search used wrong filename). No action needed. | `docs/tasks/cr-0054-*.md`, `docs/walkthroughs/cr-0054-*.md` | None | Done |
| 7 | P2 | LOW | Verify and check off pending DoD items | All three tasks have unchecked DoD items "pending first merge to main". Now that PRs are merged and CI has run, verify each item and check them off. | `docs/tasks/0050-*.md`, `docs/tasks/0051-*.md`, `docs/tasks/0052-*.md` | None | Done |
| 8 | P3 | LOW | Consolidate duplicate fixtures | Merge `registeredPage` and `authenticatedPage` into a single fixture (or make `authenticatedPage` navigate to a known authenticated page like dashboard) to reduce cognitive overhead. | `apps/web/e2e/fixtures/auth.fixture.ts:37-45` | None | Done |
| 9 | P3 | LOW | Add duplicate-registration E2E test | Add a test that attempts to register with an already-used email and verifies the appropriate error message appears. | `apps/web/e2e/register.spec.ts` | None | Done |

---

## Verification Commands

```bash
# Run after all audit actions are implemented

# Verify CI workflows (syntax check)
cd .github/workflows && yamllint ci.yml publish.yml release.yml

# Verify web build and unit tests
cd apps/web && npm run build && npm test

# Verify E2E tests (requires full stack running)
cd apps/web && npx playwright test

# Verify semantic-release config
npx semantic-release --dry-run
```
