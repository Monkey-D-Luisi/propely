# Code Review: cr-0005-quickstart-docs-review

## Metadata
- PR: #9 - docs: simplify quickstart and reduce developer friction
- PR Link: https://github.com/Monkey-D-Luisi/ai-api-template/pull/9
- Target Branch: main
- CI Status: Passing (claude-review: SUCCESS)
- Review Date: 2026-01-29

## Changed Files
- `.env.example`
- `QUICKSTART.md`
- `README.md`
- `docs/runbooks/local-development.md`
- `docs/tasks/ft-0002-simplify-quickstart-documentation.md`
- `docs/walkthroughs/ft-0002-simplify-quickstart-documentation.md`
- `scripts/run-api.ps1`
- `scripts/run-api.sh`

## Review Sources
- Review Comments: 8
- Reviews: 3 (Gemini, Codex, Copilot)
- Issue Comments: 2

## Comment Resolution Plan

### MUST_FIX
- [x] [Codex #2740062031](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740062031): .env sourcing with semicolons breaks DATABASE_CONNECTION_STRING
  - File: `scripts/run-api.sh`
  - Proposed change: Replace `source` with line-by-line parser that preserves semicolons
- [x] [Copilot #2740077566](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740077566): Same issue - source breaks with semicolons
  - File: `scripts/run-api.sh`
  - Proposed change: Implement load_env_file function that reads line-by-line

### SHOULD_FIX
- [x] [Gemini #2740061061](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740061061): Use --project flag instead of Set-Location
  - File: `scripts/run-api.ps1`
  - Proposed change: Replace `Set-Location $ApiDir; dotnet run` with `dotnet run --project $ApiDir`
- [x] [Gemini #2740061063](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740061063): Use --project flag instead of cd
  - File: `scripts/run-api.sh`
  - Proposed change: Replace `cd "$API_DIR"; dotnet run` with `dotnet run --project "$API_DIR"`
- [x] [Gemini #2740061058](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740061058): JSON blocks with // comments are invalid
  - File: `QUICKSTART.md`
  - Proposed change: Separate into labeled sections with valid JSON
- [x] [Copilot #2740077547](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740077547): Same - JSON with comments invalid
  - File: `QUICKSTART.md`
  - Proposed change: Same as above
- [x] [Copilot #2740077576](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740077576): Troubleshooting table misleading
  - File: `QUICKSTART.md`
  - Proposed change: Clarify port conflict solution and dev-reset.sh interactive prompt

### SUGGESTION
- [x] [Copilot #2740077558](https://github.com/Monkey-D-Luisi/ai-api-template/pull/9#discussion_r2740077558): "Before (8 steps)" lists 9 items
  - File: `docs/walkthroughs/ft-0002-simplify-quickstart-documentation.md`
  - Action: Update heading to match list count

## Implementation Notes

### run-api.sh .env parsing fix
The critical issue is that `source` interprets semicolons as command separators. The fix implements a proper parser that:
1. Reads lines one at a time
2. Skips comments and empty lines
3. Splits on first `=` only
4. Strips surrounding quotes from values
5. Exports the raw value without shell interpretation

### Scripts consistency
Both run-api.sh and run-api.ps1 updated to use `--project` flag, maintaining working directory.

## Commits
- `e71af58`: fix(0009): address PR review feedback (#cr-0005)
