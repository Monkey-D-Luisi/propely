# Task: cr-0016 — Docker Dev Services Review

## Metadata
- ID: cr-0016
- Type: Code Review
- Status: IN_PROGRESS
- PR: #156 (`ft/ft-0004-docker-dev-services`)
- Target: `main`
- CI: Detect Changes (SUCCESS), Web - Build (SUCCESS), AI API / Orgs API (IN_PROGRESS)

## Changed Files
- `.env.docker`, `.github/workflows/ci.yml`
- `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`
- `apps/web/.dockerignore`, `apps/web/Dockerfile.dev`
- `docker-compose.yml`
- `docs/tasks/ft-0004-docker-dev-services.md`, `docs/walkthroughs/ft-0004-docker-dev-services.md`
- `scripts/dev-down.ps1`, `scripts/dev-down.sh`
- `scripts/dev-reset.ps1`, `scripts/dev-reset.sh`
- `scripts/dev-up.ps1`, `scripts/dev-up.sh`
- `services/ai-api/.dockerignore`, `services/ai-api/Dockerfile.dev`
- `services/orgs-api/.dockerignore`, `services/orgs-api/Dockerfile.dev`

## Review Threads (10 inline, 2 reviews, 1 issue comment)

### Comment Resolution Plan

#### MUST_FIX
- [x] #7 (Copilot) `ft-0004-docker-dev-services.md:6` — Task status IN_PROGRESS → DONE

#### SHOULD_FIX
- [x] #3 (Copilot) `dev-reset.sh:26` — "Infrastructure reset complete" → accurate message
- [x] #4 (Copilot) `dev-reset.ps1:26` — Same for PowerShell
- [x] #8 (Copilot) `Dockerfile.dev:6` — Remove `*` wildcard from `package-lock.json*`
- [x] #9 (Copilot) `dev-down.sh:7` — "Stopping infrastructure..." → "Stopping all services..."
- [x] #10 (Copilot) `dev-down.ps1:14` — "Failed to stop infrastructure" → "Failed to stop services"

#### ALREADY_ADDRESSED
- [x] #1 (Gemini) `Dockerfile.dev:8` — Claims curl needed; `wget` is available in Alpine via BusyBox, container is healthy
- [x] #2 (Gemini) `docker-compose.yml:172` — Claims wget not available; verified `wget` exists at `/usr/bin/wget`

#### OUT_OF_SCOPE
- [x] #5 (Copilot) `docker-compose.yml:170` — Suggests `service_started` instead of `service_healthy`; `service_healthy` is intentional so web starts only when APIs are ready

#### SUGGESTION
- [x] #6 (Copilot) `docker-compose.yml:99` — Document named volume tradeoff; already covered in walkthrough
