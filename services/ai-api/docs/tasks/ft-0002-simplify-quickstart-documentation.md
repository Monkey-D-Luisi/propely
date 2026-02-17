# Task: ft-0002-simplify-quickstart-documentation

## Metadata
- ID: ft-0002
- Type: FastTrack
- Status: DONE
- Owner: Agent
- Created: 2026-01-29
- Related docs:
  - Walkthrough: `docs/walkthroughs/ft-0002-simplify-quickstart-documentation.md`

## Goal
Simplify the quickstart documentation to reduce developer friction and follow best practices.

## Context
The existing quickstart documentation had several friction points:
1. `.env.example` required manual editing (CHANGEME placeholders)
2. Starting the API required 8 manual steps
3. Developers had to manually export `DATABASE_CONNECTION_STRING` every terminal session
4. Navigation used confusing relative paths (`cd ../`)
5. Documentation was duplicated and verbose across multiple files

## Scope

### In scope
- Simplify `.env.example` with working defaults
- Create `run-api.sh` and `run-api.ps1` scripts to load `.env` automatically
- Create `dev-up.ps1`, `dev-down.ps1`, `dev-reset.ps1` for Windows compatibility
- Update QUICKSTART.md with OS-specific instructions (Linux/macOS and Windows PowerShell)
- Reduce quickstart steps from 8 to 4
- Use `--project` flag instead of `cd` navigation
- Consolidate documentation across README.md, QUICKSTART.md, and local-development.md

### Out of scope
- Changes to application code
- New features or endpoints
- CI/CD pipeline changes

## Acceptance Criteria
- [x] `.env.example` has working defaults (no CHANGEME)
- [x] `run-api.sh` loads `.env` and starts API
- [x] `run-api.ps1` provides Windows equivalent
- [x] `dev-up.ps1`, `dev-down.ps1`, `dev-reset.ps1` provide Windows infrastructure scripts
- [x] QUICKSTART.md has 4 clear steps with OS-specific commands
- [x] QUICKSTART.md shows both Linux/macOS and Windows PowerShell examples
- [x] README.md quick start is concise
- [x] local-development.md is consistent with other docs
- [x] Scripts have valid syntax
- [x] Full flow tested on Windows from clean state

## Files to Create / Modify
- `.env.example` - Remove CHANGEME, add working defaults
- `scripts/run-api.sh` - New script for Linux/macOS
- `scripts/run-api.ps1` - New script for Windows
- `scripts/dev-up.ps1` - New script for Windows
- `scripts/dev-down.ps1` - New script for Windows
- `scripts/dev-reset.ps1` - New script for Windows
- `QUICKSTART.md` - Simplified flow with OS-specific instructions
- `README.md` - Concise quick start section
- `docs/runbooks/local-development.md` - Updated for consistency

## Definition of Done
- [x] Goal achieved
- [x] No secrets committed
- [x] Walkthrough updated
- [x] Scripts executable and syntax valid
