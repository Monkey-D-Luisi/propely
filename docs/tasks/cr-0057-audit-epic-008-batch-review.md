# Code Review: cr-0057-audit-epic-008-batch-review

## PR Metadata
- PR: #257
- Title: fix(audit): address all audit findings for epic 008
- Target branch: main
- CI Status: Web Build SUCCESS, E2E SUCCESS, AI API SKIPPED, Orgs API SKIPPED

## Changed Files
- `.github/actions/check-coverage/action.yml` (new)
- `.github/workflows/ci.yml`
- `.github/workflows/publish.yml`
- `.github/workflows/release.yml`
- `.releaserc.json`
- `.trivyignore` (new)
- `apps/web/e2e/fixtures/auth.fixture.ts`
- `apps/web/e2e/login.spec.ts`
- `apps/web/e2e/register.spec.ts`
- `docs/audits/epic-008-executive-summary.md` (new)
- `docs/tasks/0050-ghcr-publishing.md`
- `docs/tasks/0051-semantic-release.md`
- `docs/tasks/0052-e2e-smoke-tests.md`
- 8 audit task + walkthrough files

## Review Threads

### Inline Comments (4)

1. **gemini-code-assist** — `.github/actions/check-coverage/action.yml:22`
   Pin `dotnet-reportgenerator-globaltool` version instead of `update` for CI stability.

2. **Copilot** — `.github/workflows/ci.yml:102`
   `defaults.run.working-directory` does NOT apply to `uses:` steps. Coverage paths are relative and will resolve from repo root, not `services/ai-api/`.

3. **Copilot** — `.github/workflows/ci.yml:171`
   Same working-directory issue for orgs-api coverage paths.

4. **Copilot** — `apps/web/e2e/register.spec.ts:52`
   `registeredPage` fixture leaves browser authenticated; navigating to `/en/register` may redirect. Need to clear auth state first.

### Reviews (2)
- gemini-code-assist: COMMENTED — positive summary, 1 inline suggestion
- Copilot: COMMENTED — 3 inline suggestions

### Issue Comments (2)
- chatgpt-codex-connector: Usage limit message (not actionable)
- gemini-code-assist: PR summary (not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] #2 (Copilot): Fix coverage paths in ci.yml ai-api job — use repo-rooted paths
- [x] #3 (Copilot): Fix coverage paths in ci.yml orgs-api job — use repo-rooted paths
- [x] #4 (Copilot): Clear auth state in duplicate-registration E2E test before navigating to register

### SHOULD_FIX
- [x] #1 (Gemini): Pin `dotnet-reportgenerator-globaltool` version in composite action

## Parity Verification
- [x] Redirect parity: N/A (no auth redirect changes in this PR)
- [x] Locale source correctness: N/A (no locale changes)
- [x] API/UI contract parity: N/A (no API changes)
- [x] Test parity: Duplicate-registration test covers server-side 409 error path
