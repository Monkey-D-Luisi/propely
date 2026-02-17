# Task: cr-0021 - CI Improvements Review

## Metadata
- ID: cr-0021
- Type: CodeReview
- Status: DOING
- Owner: Agent
- Created: 2026-02-07
- PR: #161 (`feat/0021-ci-improvements`)
- CI Status: Web in progress, CodeQL in progress, .NET jobs skipped (no service changes)

## Changed Files
- `.agent/rules/autonomous-workflow.md`
- `.github/dependabot.yml`
- `.github/workflows/ci.yml`
- `.github/workflows/codeql.yml`
- `README.md`
- `apps/web/package-lock.json`
- `apps/web/package.json`
- `apps/web/vitest.config.ts`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0020-dockerfiles.md`
- `docs/tasks/0021-ci-improvements.md`
- `docs/walkthroughs/0021-ci-improvements.md`

## Review Threads

### Unresolved

1. **Gemini** (inline, `dependabot.yml:23`) — Group NuGet updates to reduce PR count
2. **Copilot** (inline, `ci.yml:118`) — Claims `actions: write` needed for artifact uploads
3. **Copilot** (inline, `ci.yml:103`) — `|| true` masks genuine install failures (ai-api)
4. **Copilot** (inline, `ci.yml:180`) — `|| true` masks genuine install failures (orgs-api)

### General Reviews (no additional items)

5. **Gemini** (review body) — Summary, references thread #1
6. **Copilot** (review body) — Overview, references threads #2-#4

### Issue Comments (no actionable items)

7. **Gemini** (issue comment) — PR summary, no action needed

## Comment Resolution Plan

### SHOULD_FIX
- [x] #1: Add Dependabot `groups` to batch NuGet updates per service
- [x] #3: Replace `dotnet tool install ... || true` with `dotnet tool update` in ai-api coverage step
- [x] #4: Same fix as #3 for orgs-api coverage step

### NO_ACTION
- [x] #2: `upload-artifact` uses internal GitHub APIs, not governed by the `actions` permission scope. Claim is incorrect.
- [x] #5: Review summary, no action needed
- [x] #6: Review summary, no action needed
- [x] #7: Issue comment summary, no action needed
