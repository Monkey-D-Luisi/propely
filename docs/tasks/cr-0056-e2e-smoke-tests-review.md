# Task: CR-0056 — E2E Smoke Tests PR Review

## PR Metadata
- PR: [#256](https://github.com/Monkey-D-Luisi/saas-template/pull/256)
- Target branch: main
- CI: All checks passed

## Changed Files
- `apps/web/playwright.config.ts`
- `apps/web/e2e/fixtures/auth.fixture.ts`
- `apps/web/e2e/register.spec.ts`
- `apps/web/e2e/login.spec.ts`
- `apps/web/e2e/create-org.spec.ts`
- `apps/web/e2e/invite-member.spec.ts`
- `apps/web/package.json`
- `apps/web/package-lock.json`
- `apps/web/tsconfig.json`
- `apps/web/.gitignore`
- `.gitignore`
- `.github/workflows/ci.yml`
- `docs/tasks/0052-e2e-smoke-tests.md`
- `docs/backlog/epic-008-cicd-release.md`
- `docs/walkthroughs/0052-e2e-smoke-tests.md`

## Review Comments (8 inline, 2 reviews, 2 issue comments)

### Comment Resolution Plan

#### SHOULD_FIX
- [x] **Gemini #2797607210 + Copilot #2797628682** — `registeredPage` and `authenticatedPage` fixtures are identical. Extract shared `registerUser` helper to deduplicate.
- [x] **Gemini #2797607193** — Org name in `create-org.spec.ts` uses only `Date.now()` without random suffix. Add random suffix for robustness (same pattern as `testUser` fixture).
- [x] **Copilot #2797628644** — Web health check in CI uses `/` which may 404 since routes live under `/[locale]`. Use `/en` for a direct 200 response instead of relying on redirect behavior.

#### SUGGESTION
- [x] **Gemini #2797607217** — Org creation logic duplicated in `invite-member.spec.ts`. Extract `createOrg` helper for reuse.

#### REJECTED
- **Copilot #2797628584** — DoD checklist inconsistency. Rejected: established project pattern from tasks 0050/0051 where status = DONE with DoD items marked "pending first merge to main".
- **Copilot #2797628620** — Epic status should be PENDING. Rejected: same reasoning as above.
- **Copilot #2797628707** — Task status should be IN_PROGRESS. Rejected: same reasoning as above.

## Parity Verification
- [x] Redirect parity checked — N/A (no auth changes)
- [x] Locale source correctness checked — Tests use explicit `/en/` locale prefix
- [x] API/UI contract parity checked — N/A (no API/UI changes)
- [x] Test parity checked — This task IS the test setup
