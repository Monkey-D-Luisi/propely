# CR-0020: PR #45 — OpenAI MCP Server Review Walkthrough

## Task Reference
- Task: `docs/tasks/cr-0020-pr45-openai-mcp-review.md`
- PR: #45 (`feat/openai-mcp-server` → `main`)

## Changes Made

### R3 (MUST_FIX): Force token refresh on 401
- **Problem**: When `expiresAt` is 0 (missing from auth file), `clearTokenCache()` + `getAuth()` just reloads the same expired token
- **Fix**: Added `forceRefresh()` export to `auth.ts` that unconditionally calls `refreshTokens()` regardless of `expiresAt`. Updated `index.ts` to call `forceRefresh()` on 401 instead of `clearTokenCache()` + implicit `getAuth()`.

### R2 (SHOULD_FIX): Atomic write for auth.json
- **Problem**: `writeFileSync` directly to auth.json can corrupt the file if process is interrupted mid-write
- **Fix**: Changed to write to a temp file (`auth.json.tmp`) in the same directory, then rename atomically via `renameSync`.

## Commands Run
- `npm run build` — TypeScript compiles cleanly
- Manual `ask_gpt` tool test — verified tool still works after changes

## Validation Results
- Build: PASS
- Manual test (basic call): PASS
- Manual test (model override): PASS
