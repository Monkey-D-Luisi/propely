# Walkthrough: ft-0002-simplify-quickstart-documentation

## Summary
Simplified the quickstart documentation to reduce developer friction from 8 steps to 4, and created helper scripts to automate environment loading.

## Problem Analysis

### Issues Identified

| Problem | Impact | Root Cause |
|---------|--------|------------|
| `.env.example` with CHANGEME | High - Blocks new developers | Overly cautious approach despite docker-compose having defaults |
| Manual export of DATABASE_CONNECTION_STRING | High - Tedious every session | No script to load .env |
| 8 steps to run API | Medium - Overwhelming for newcomers | Over-documented individual steps |
| `cd ../` navigation | Medium - Confusing if wrong directory | Not using --project flag |

### Before vs After

**Before (9 steps):**
1. Clone repo
2. Copy .env.example to .env
3. Edit .env (replace CHANGEME)
4. Run dev-up.sh
5. cd to Infrastructure directory
6. Run migrations
7. cd to Api directory
8. Export DATABASE_CONNECTION_STRING
9. Run dotnet run

**After (4 steps):**
1. Clone repo, copy .env.example to .env
2. Run dev-up.sh
3. Run migrations with --project flag
4. Run run-api.sh

## Implementation

### 1. Simplified .env.example

Removed CHANGEME placeholders and provided working defaults:

```bash
# Before
POSTGRES_PASSWORD=CHANGEME
DATABASE_CONNECTION_STRING=Host=localhost;...Password=CHANGEME

# After
POSTGRES_PASSWORD=aiapi_dev_password
DATABASE_CONNECTION_STRING=Host=localhost;...Password=aiapi_dev_password
```

**Rationale:** docker-compose.yml already had these defaults. The .env.example should match for zero-config setup.

### 2. Created run-api.sh

New script that:
- Loads .env automatically using `source`
- Validates DATABASE_CONNECTION_STRING is set
- Changes to correct directory
- Runs the API

```bash
./scripts/run-api.sh  # Just works
```

### 3. Created run-api.ps1

Windows PowerShell equivalent:
- Parses .env file
- Sets environment variables
- Runs the API

```powershell
.\scripts\run-api.ps1  # Just works on Windows
```

### 4. Updated Documentation

**QUICKSTART.md:**
- Reduced from 266 to 109 lines
- Clear 4-step process
- Added daily workflow section
- Added troubleshooting table

**README.md:**
- Concise quick start section
- Removed verbose explanations
- Uses new scripts

**local-development.md:**
- Added scripts table
- Added infrastructure services table
- Consistent with other docs

### 5. Used --project Flag

Instead of:
```bash
cd src/SaasTemplate.AiApi.Infrastructure
dotnet ef database update
```

Now:
```bash
dotnet ef database update --project src/SaasTemplate.AiApi.Infrastructure
```

**Rationale:** Works from any directory, clearer for copy-paste.

### 6. Created Windows PowerShell Infrastructure Scripts

To ensure Windows users have the same experience as Linux/macOS users, created three additional PowerShell scripts:

**dev-up.ps1:**
- Checks Docker prerequisites and running status
- Starts containers with `docker-compose up -d`
- Shows success message with service URLs
- Matches `dev-up.sh` functionality

**dev-down.ps1:**
- Stops containers with `docker-compose down`
- Preserves data volumes
- Matches `dev-down.sh` functionality

**dev-reset.ps1:**
- Interactive confirmation (type 'yes')
- Removes all volumes with `docker-compose down -v --remove-orphans`
- Clear warnings about data loss
- Matches `dev-reset.sh` functionality

### 7. Updated QUICKSTART.md for Cross-Platform Support

Added OS-specific sections throughout:
- Step 1: Both `cp` (Linux/macOS) and `Copy-Item` (Windows PowerShell)
- Step 2: Both `./scripts/dev-up.sh` and `.\scripts\dev-up.ps1`
- Step 4: Both `./scripts/run-api.sh` and `.\scripts\run-api.ps1`
- Test API: Added PowerShell `Invoke-RestMethod` examples with `ConvertTo-Json`
- Daily Workflow: Separate sections for each OS

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| Default passwords in .env.example | docker-compose already has them; dev-only; .env is gitignored |
| Separate scripts for bash/PowerShell | Cross-platform without complex detection |
| Keep verbose docs in local-development.md | Advanced users still need reference |
| Tables for troubleshooting | Scannable, quick to find solution |
| ConvertTo-Json for PowerShell | More reliable than escaped JSON strings |
| OS-specific sections in QUICKSTART | Clear what to run on each platform, reduces confusion |

## Files Changed

| File | Change |
|------|--------|
| `.env.example` | Working defaults, removed CHANGEME |
| `scripts/run-api.sh` | New - loads .env, runs API |
| `scripts/run-api.ps1` | New - Windows equivalent |
| `scripts/dev-up.ps1` | New - Windows infrastructure start |
| `scripts/dev-down.ps1` | New - Windows infrastructure stop |
| `scripts/dev-reset.ps1` | New - Windows infrastructure reset |
| `QUICKSTART.md` | Simplified to 4 steps, OS-specific commands |
| `README.md` | Concise quick start |
| `docs/runbooks/local-development.md` | Tables, consistency |

## Metrics

| Metric | Before | After |
|--------|--------|-------|
| Steps to run API | 8 | 4 |
| QUICKSTART.md lines | 266 | ~130 |
| Manual env exports needed | Every session | Never |
| Files edited by new dev | 1 (.env) | 0 |
| PowerShell scripts | 1 | 4 |
| Windows compatibility | Partial | Full |

## Verification

### Tested on Windows (PowerShell)
- ✅ `.\scripts\dev-up.ps1` - Started PostgreSQL, RabbitMQ, Redis
- ✅ Database migrations applied successfully
- ✅ `.\scripts\run-api.ps1` - API running on http://localhost:5008
- ✅ POST endpoint - Created work item successfully
- ✅ GET endpoint - Retrieved work item successfully

All steps from QUICKSTART completed successfully on Windows without workarounds.

## Follow-ups

None required. Documentation is now self-consistent, cross-platform, and follows best practices.

## Commit

```
docs: simplify quickstart and add Windows support (#ft-0002)
```
