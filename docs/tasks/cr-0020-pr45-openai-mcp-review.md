# CR-0020: PR #45 — OpenAI MCP Server Review

## PR Metadata
- **PR**: #45 (`feat/openai-mcp-server` → `main`)
- **Title**: feat(tools): add OpenAI MCP server for GPT consultation
- **CI Status**: All checks green (E2E cancelled — unrelated)
- **Scope**: `tools/mcp-openai-oauth/` (auth.ts, index.ts, package.json, tsconfig.json), `CLAUDE.md`

## Changed Files (MCP scope only)
- `tools/mcp-openai-oauth/src/auth.ts` — OAuth token management
- `tools/mcp-openai-oauth/src/index.ts` — MCP server, SSE parsing, `ask_gpt` tool
- `tools/mcp-openai-oauth/package.json` — Dependencies
- `tools/mcp-openai-oauth/tsconfig.json` — TypeScript config
- `tools/mcp-openai-oauth/.gitignore` — Build output exclusion
- `CLAUDE.md` — GPT Consultation section

## Section 1: Agent Review Findings

| # | Severity | Category | File | Description |
|---|----------|----------|------|-------------|
| A1 | NIT | Code Quality | index.ts:111-113 | Catch block logs then re-throws — useful for debugging, keep as-is |
| A2 | NIT | Testing | tools/ | No unit tests for MCP server — acceptable for dev tool with manual testing |

## Section 2: Review Comment Threads

| # | Source | Severity | File:Line | Description | Classification |
|---|--------|----------|-----------|-------------|----------------|
| R1 | Copilot | NIT | index.ts:23 | `parseSSEResponse` buffers full SSE body in memory | NIT — responses are small consultation texts |
| R2 | Copilot | SHOULD_FIX | auth.ts:117 | `writeFileSync` non-atomic — risk of corruption on interruption | SHOULD_FIX |
| R3 | Copilot+Codex | MUST_FIX | index.ts:80 | 401 retry doesn't force token refresh when `expiresAt==0` | MUST_FIX |
| R4 | Copilot | NIT | package.json:6 | PR scope broader than title suggests | OUT_OF_SCOPE — user aware |
| R5 | Copilot | OUT_OF_SCOPE | OutboxDispatcher* | Timing-based tests (4 comments, 4 services) | OUT_OF_SCOPE — being fixed separately |
| R6 | Copilot+Codex | OUT_OF_SCOPE | BookViewingActionHandler:67 | Missing end_time > start_time validation | OUT_OF_SCOPE — separate commit |
| R7 | Copilot | OUT_OF_SCOPE | AppSidebar:63 | Missing aria-hidden on icon spans | OUT_OF_SCOPE — separate commit |

## Resolution Plan

### MUST_FIX
- [x] R3: Force token refresh on 401 (treat `expiresAt <= 0` as expired, or add dedicated `forceRefresh` path)

### SHOULD_FIX
- [x] R2: Atomic write for auth.json (write to temp file, rename)

### NIT (no action)
- R1: SSE buffering — data is small, no change needed
- A1: Catch/log/re-throw pattern — useful for debugging
- R4: PR scope — user's decision

### OUT_OF_SCOPE (documented, no action)
- R5: OutboxDispatcher timing — being fixed separately
- R6: BookViewing validation — belongs to ai-api commit
- R7: AppSidebar a11y — belongs to web commit
