# Task: 0072 - One-Command Developer Onboarding

## Metadata
- ID: 0072
- Type: Enhancement
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #277
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Enable a buyer to go from `git clone` to a fully running application with a single command, reducing onboarding friction to near-zero.

## Context
The pre-sale audit identified DX onboarding as critical: "if someone takes more than 15 minutes to set up the project from scratch, you lose the sale." Currently the setup requires running `dev-up.ps1`/`dev-up.sh` scripts which assume the buyer already has `.env` configured. A single entry point that handles everything is needed.

### Current Setup Flow
1. Clone repo
2. Copy `.env.example` to `.env` (manual)
3. Edit `.env` with custom values (manual)
4. Run `.\scripts\dev-up.ps1` or `./scripts/dev-up.sh`
5. Wait for services to be healthy
6. Open browser manually

### Target Setup Flow
1. Clone repo
2. Run `make dev` (or platform-specific equivalent)
3. Everything works, browser opens

## Scope
### In scope
- Create cross-platform entry point (`Makefile` with `make dev` target)
- PowerShell bootstrap script (`scripts/bootstrap.ps1`) for Windows users without Make
- Prerequisite checks: Docker, Node.js, .NET SDK (with version validation)
- Automatic `.env` creation from `.env.example` if missing
- Delegate to existing `dev-up` scripts for actual orchestration
- Wait for health checks (HTTP probes on service ports)
- Auto-open browser on success
- Clear, colored terminal output showing progress

### Out of scope
- Installing prerequisites (just check and advise)
- Replacing existing `dev-up` scripts (wrap, don't replace)
- IDE setup (VS Code extensions, etc.)

## Requirements
- R1: `make dev` works on macOS and Linux with Make installed
- R2: `.\scripts\bootstrap.ps1` works on Windows without Make
- R3: Prerequisite checks emit clear, actionable error messages
- R4: `.env` auto-created from `.env.example` with warning if values need editing
- R5: Health checks wait for all services before declaring success
- R6: Total time from clone to running app < 15 minutes (first run, excluding Docker pulls)

## Acceptance Criteria
- AC1: `make dev` boots full stack from fresh clone on macOS/Linux
- AC2: `.\scripts\bootstrap.ps1` boots full stack from fresh clone on Windows
- AC3: Missing prerequisites show clear error with install instructions
- AC4: `.env` auto-created if missing
- AC5: Health check waits for all 3 services (web:3000, ai-api:5010, orgs-api:5020)
- AC6: Browser opens automatically on success
- AC7: Progress output is clear and formatted

## Implementation Steps
1. Create `Makefile` with `dev` target
2. Create `scripts/bootstrap.ps1` for Windows
3. Implement prerequisite checks (Docker version, Node version, .NET version)
4. Implement `.env` auto-creation logic
5. Call existing `dev-up` scripts from Makefile/bootstrap
6. Implement HTTP health check polling loop
7. Implement browser auto-open (cross-platform)
8. Add colored output/progress indicators
9. Test on Windows, verify Makefile approach for macOS/Linux
10. Update README with new quickstart instructions

## Files to Create
- `Makefile`
- `scripts/bootstrap.ps1`
- `docs/walkthroughs/0072-one-command-onboarding.md`

## Files to Modify
- `README.md` (quickstart section)

## Definition of Done Checklist
- [x] `make dev` works on macOS/Linux
- [x] `scripts/bootstrap.ps1` works on Windows
- [x] Prerequisite checks with clear error messages
- [x] `.env` auto-creation works
- [x] Health checks pass before declaring success
- [x] Browser auto-opens
- [x] Walkthrough updated
