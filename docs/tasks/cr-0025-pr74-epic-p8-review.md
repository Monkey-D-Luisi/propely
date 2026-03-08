# Code Review: cr-0025-pr74-epic-p8-review

## PR Metadata
- PR: #74 (`epic/P8-ai-provider-abstraction` → `main`)
- CI Status: All green except **Third-Party Notices - Staleness Check** (FAILURE — dependency hash changed)
- Changed files: 107

## Section 1: Agent Review Findings

### A.2.3 API & Security
1. **MUST_FIX** — `app.MapMcp("/mcp")` does not enforce authorization. The comment claims "Sits behind the same auth middleware as REST endpoints" but `MapMcp` uses endpoint routing which requires explicit `.RequireAuthorization()`. Without it, the MCP endpoint is accessible anonymously.
2. **MUST_FIX** — `ExtractUserContext` in `PropelyMcpTools.cs` silently falls back to `Guid.Empty` when `org_id` or `NameIdentifier` claims are missing. This could allow unauthenticated MCP calls to execute tools under an empty tenant context, violating tenant isolation.

### A.2.5 Testing
3. **NIT** — `McpToolHandlerTests.HandleToolCallAsync_WithJsonElementArguments_ShouldNormalizeToDict` test name suggests normalization happens in McpToolHandler, but normalization actually happens in DefaultParameterBinder. The test name is misleading.

### A.2.8 Code Quality
4. **SHOULD_FIX** — Task 0053 status is `DOING` but should be `DONE` (epic marks 8.2 as DONE).
5. **SHOULD_FIX** — Task 0054 status is `DOING` but should be `DONE` (epic marks 8.4 as DONE).

### A.3 Behavioral Parity
6. **FALSE_POSITIVE** — UpdatePropertyHandler lost multi-field update fallback. However, the tool schema (ToolSchemaRegistry from Task 8.1) only defines `field` and `value` parameters. The multi-field fallback was dead code — the LLM/MCP client would never send arbitrary extra keys outside the schema-defined parameters. The single field+value approach matches the documented API contract. No action needed.

### CI
7. **MUST_FIX** — Third-Party Notices staleness check failed because `Propely.AiApi.Api.csproj` added `ModelContextProtocol.AspNetCore`. Need to regenerate via `./scripts/generate-third-party-notices.ps1`.

## Section 2: Review Comment Threads

### Gemini Code Assist (1 inline comment)
- G1: UpdatePropertyHandler multi-field regression → **FALSE_POSITIVE** (see finding #6)

### Copilot (8 inline comments)
- C1: Walkthrough 0054 test count "25" → **FALSE_POSITIVE** (5 Facts + 1 Theory×20 InlineData = 25 test cases, count is accurate)
- C2: Task 0054 status DOING→DONE → **SHOULD_FIX** (same as finding #5)
- C3: Task 0053 status DOING→DONE → **SHOULD_FIX** (same as finding #4)
- C4: UpdatePropertyHandler multi-field → **FALSE_POSITIVE** (same as finding #6)
- C5: MapMcp requires `.RequireAuthorization()` → **MUST_FIX** (same as finding #1)
- C6: ExtractUserContext fail-fast → **MUST_FIX** (same as finding #2)
- C7: DateTime UTC handling in DefaultParameterBinder → **OUT_OF_SCOPE** — valid concern but DateTime normalization is a separate concern. Handlers already deal with UTC conversion. Adding a custom JsonConverter is a new feature, not a review fix.
- C8: McpToolHandlerTests misleading test name → **NIT** (same as finding #3)

## Resolution Plan

### MUST_FIX
- [x] #1/#C5: Add `.RequireAuthorization()` to `app.MapMcp("/mcp")`
- [x] #2/#C6: Make `ExtractUserContext` throw on missing/invalid claims
- [x] #7: Regenerate third-party notices

### SHOULD_FIX
- [x] #4/#C3: Update task 0053 status to DONE
- [x] #5/#C2: Update task 0054 status to DONE

### NIT
- [x] #3/#C8: Rename misleading test name

### FALSE_POSITIVE
- #6/#G1/#C4: UpdatePropertyHandler multi-field — tool schema only defines `field`+`value`, multi-field fallback was dead code
- #C1: Walkthrough test count — 5 Facts + 20 InlineData = 25 test cases, accurate

### OUT_OF_SCOPE
- #C7: DateTime UTC handling — separate concern, handlers manage UTC conversion already
