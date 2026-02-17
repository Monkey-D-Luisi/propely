# Walkthrough: 0072-one-command-onboarding

## Task Reference
- Task: `docs/tasks/0072-one-command-onboarding.md`
- Walkthrough: `docs/walkthroughs/0072-one-command-onboarding.md`
- Branch/PR: `feat/0072-one-command-onboarding`
- Date: `2026-02-14`

## Summary
Created a one-command developer onboarding experience that takes a buyer from `git clone` to a fully running application. On macOS/Linux, `make dev` runs the full bootstrap. On Windows, `.\scripts\bootstrap.ps1` does the same. Both check prerequisites (Docker, Node.js, .NET SDK with version validation), auto-create `.env` from `.env.example`, start all services via existing `dev-up` scripts, wait for HTTP health checks on all 3 services, and open the browser automatically.

## Context
- Background: The pre-sale audit identified DX onboarding as critical — "if someone takes more than 15 minutes to set up the project from scratch, you lose the sale." The previous setup required manual `.env` creation and no health check feedback.
- Problem statement: Multi-step manual setup process causes buyer friction and lost conversions.
- Constraints: Must wrap existing `dev-up` scripts (not replace), must work cross-platform, must not install prerequisites (just check and advise).

## Decisions & Trade-offs
- **Decision: Makefile + separate bootstrap.sh for macOS/Linux**
  - Options considered: (a) Makefile only with inline shell, (b) Makefile that calls bootstrap.sh, (c) bash-only with no Makefile
  - Why this choice: Option (b) keeps the Makefile clean while the script handles complex logic (health checks, colored output, version parsing). The Makefile provides the ergonomic `make dev` entry point.
  - Consequences / risks: Requires Make on Linux/macOS (pre-installed on macOS, standard on Linux). Windows users use `bootstrap.ps1` directly.

- **Decision: Health check polling with 120s timeout (60 attempts × 2s)**
  - Why: First-run Docker builds can take a while. 120s is generous without being indefinite. Non-blocking: warns on timeout rather than failing, since the service may still be starting.
  - Risk: If all services are genuinely broken, user waits 6 minutes (120s × 3 services). Acceptable trade-off for first-run reliability.

- **Decision: Check `/health/live` for API services, `/` for web**
  - Why: Both .NET services expose `/health/live` (liveness) and `/health/ready` (readiness) endpoints. The Next.js app doesn't have a dedicated health endpoint, so we check the root URL which returns 200 when the server is ready.

## Implementation Notes
- Key changes:
  - `scripts/bootstrap.sh` — 5-step bootstrap for macOS/Linux with colored terminal output
  - `scripts/bootstrap.ps1` — Equivalent for Windows PowerShell with Unicode check marks
  - `Makefile` — `dev`, `down`, `reset`, `help` targets wrapping existing scripts
  - `README.md` — Simplified Quick Start section featuring `make dev` / `bootstrap.ps1` as primary entry point
- Edge cases handled:
  - Docker installed but daemon not running (separate check)
  - Node.js installed but version too old (< 22)
  - .NET SDK installed but version too old (< 10)
  - `.env` already exists (skip creation)
  - No browser opener available on Linux (warns with manual URL)
  - Health check timeout is non-fatal (warns instead of failing)
- Known limitations:
  - `make` is not available on Windows by default — Windows users must use `bootstrap.ps1` directly
  - Health check uses simple HTTP polling, not Docker healthcheck status

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Fully backward compatible — existing `dev-up` scripts unchanged

## Commands Run
```bash
cd apps/web && npm run build   # Verify build passes
cd apps/web && npm test        # Verify all 431 tests pass
docker --version               # Verify Docker 28.4.0
node --version                 # Verify Node.js 22.12.0
dotnet --version               # Verify .NET SDK 10.0.102
```

## Files Changed
- `scripts/bootstrap.sh` — New: bash bootstrap script with prerequisite checks, .env creation, health checks, browser open
- `scripts/bootstrap.ps1` — New: PowerShell bootstrap script (Windows equivalent)
- `Makefile` — New: `dev`, `down`, `reset`, `help` targets
- `README.md` — Updated Quick Start to feature one-command setup, added bootstrap to scripts table
- `docs/backlog/epic-013-presale-hardening.md` — Status: PENDING → IN_PROGRESS → DONE
- `docs/tasks/0072-one-command-onboarding.md` — Status: PENDING → DONE, DoD checked
- `docs/walkthroughs/0072-one-command-onboarding.md` — New: this file

## Tests
### Unit
- No unit tests needed — scripts are not testable in isolation (they check system prerequisites and Docker state)

### Integration
- Build verification: `npm run build` passes with all 35 routes
- Test suite: 431/431 tests pass across 46 test files

### Manual
- Verified prerequisite checks detect Docker 28.4.0, Node.js 22.12.0, .NET SDK 10.0.102 on Windows

## Observability
- Logs added/updated: Bootstrap scripts produce structured, colored step-by-step output
- Traces/metrics: N/A

## Security
- Validation: Scripts do not handle secrets — `.env` is created as a copy of `.env.example` which contains only dev defaults
- AuthN/AuthZ impact: None
- Sensitive data handling: No secrets in scripts. `.env` is already in `.gitignore`

## Follow-ups / Backlog
- [ ] Add `make test` target to run all test suites from a single command
- [ ] Add `seed-dev.sh` equivalent for Windows (PowerShell)
- [ ] Consider adding Docker Compose healthcheck definitions so `docker compose up --wait` can replace custom polling

## Checklist
- [x] Task scope matches `docs/tasks/0072-one-command-onboarding.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
