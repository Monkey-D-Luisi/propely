# Walkthrough: CR-0070 — One-Command Onboarding PR Review

## Task Reference
- Task: `docs/tasks/cr-0070-onboarding-scripts-review.md`
- PR: #285 — feat(dx): one-command developer onboarding (#0072)

## Changes Made

### 1. Fixed non-portable `grep -oP` on macOS
- Replaced `grep -oP '\d+\.\d+\.\d+' | head -1` with portable `sed` for Docker version extraction
- macOS BSD grep does not support `-P` (Perl regex), making the original code fail on macOS

### 2. Fixed health check endpoints
- Changed `/health` to `/health/live` in both `bootstrap.sh` and `bootstrap.ps1`
- The .NET services expose `/health/live` (liveness) and `/health/ready` (readiness) — no `/health` endpoint exists
- Updated walkthrough documentation to reflect the correct endpoints

### 3. Added `help` to Makefile `.PHONY`
- Prevents conflicts with potential file named `help`

### 4. Simplified PowerShell Docker daemon check
- Removed redundant `$LASTEXITCODE` check inside try block (unreachable with `$ErrorActionPreference = "Stop"`)
- Simplified to rely on try/catch pattern cleanly

### 5. Simplified `$LASTEXITCODE` check after dev-up.ps1
- Changed `if ($LASTEXITCODE -and $LASTEXITCODE -ne 0)` to `if ($LASTEXITCODE)`

### 6. Added dotnet version empty check
- Added null/empty guard before parsing `dotnet --version` output to handle corrupted installations

### 7. Added explicit dev-up.sh exit code check
- Replaced implicit `set -e` exit with `if ./scripts/dev-up.sh; then` for better error messaging

## Commands Run
- `cd apps/web && npm run build` — verify build passes
- `cd apps/web && npm test` — verify all 431 tests pass

## Process Deviations
None.
