# Walkthrough: ft-0004 — Docker Dev Services

## Task Reference
- Task: `docs/tasks/ft-0004-docker-dev-services.md`
- Walkthrough: `docs/walkthroughs/ft-0004-docker-dev-services.md`
- Branch/PR: `ft/ft-0004-docker-dev-services`
- Date: `2026-02-06`

## Summary
Added all three application services (ai-api, orgs-api, web) to Docker Compose with hot reload for development. Running `docker compose --profile apps up` now starts the entire stack. The existing `docker compose up` behavior is unchanged (infrastructure only).

## Architecture

### Two modes of development
| Mode | Command | What runs in Docker |
|------|---------|-------------------|
| Host-based (existing) | `.\scripts\dev-up.ps1` + `.\scripts\run-*.ps1` | Infrastructure only |
| Full Docker (new) | `.\scripts\dev-up.ps1 -Apps` | Infrastructure + all apps |

### Profile-based activation
- Infrastructure services have no profile (always start)
- App services use `profiles: ["apps"]` (start only with `--profile apps`)

### Environment variable strategy
- `.env` — localhost hostnames (for host-based dev)
- `.env.docker` — container-name hostnames (overrides for Docker)
- Compose uses `env_file: [.env, .env.docker]` — second file wins
- Secrets flow from `.env` automatically

### Hot reload
- .NET: `dotnet watch run` with source volume mount, `obj/` in named volumes
- Next.js: `npm run dev` with source volume mount, `node_modules/` and `.next/` in named volumes
- `WATCHPACK_POLLING=true` for file watching on Windows Docker

## Issues Encountered

### 1. SLN restore fails with tests excluded
`.dockerignore` excludes `tests/` but the SLN references test projects. Fixed by restoring the API `.csproj` directly instead of the SLN (transitively restores all src dependencies).

### 2. Next.js Turbopack lockfile permission error
The `.next/` directory from the host volume mount conflicted with the container's lockfile. Fixed by adding a named volume for `.next/` (`web-next:/app/.next`).

### 3. Port conflict from host processes
Port 5020 was still in use from a previous host-based orgs-api run. Resolved by killing the process before Docker start.

### 4. localhost not accessible (IPv6 resolution)
Docker port bindings using `127.0.0.1:PORT:PORT` only bind IPv4. On Windows, `localhost` can resolve to `::1` (IPv6), making services unreachable via `localhost`. This also broke login because the browser at `127.0.0.1:3000` tried to call `localhost:5020` for CSRF/auth — a cross-origin mismatch. Fixed by changing app service port bindings from `127.0.0.1:PORT:PORT` to `PORT:PORT` (binds both IPv4 and IPv6).

## Files Created
- `.env.docker` — Container-name hostname overrides
- `services/ai-api/Dockerfile.dev` — Dev Dockerfile (SDK image + dotnet watch)
- `services/ai-api/.dockerignore` — Docker build exclusions
- `services/orgs-api/Dockerfile.dev` — Dev Dockerfile
- `services/orgs-api/.dockerignore` — Docker build exclusions
- `apps/web/Dockerfile.dev` — Dev Dockerfile (Node Alpine + npm run dev)
- `apps/web/.dockerignore` — Docker build exclusions
- `docs/tasks/ft-0004-docker-dev-services.md` — Task file
- `docs/walkthroughs/ft-0004-docker-dev-services.md` — This walkthrough

## Files Modified
- `docker-compose.yml` — Added 3 app services with profiles, health checks, volumes
- `scripts/dev-up.ps1` — Added `-Apps` parameter
- `scripts/dev-up.sh` — Added `--apps` argument
- `scripts/dev-down.ps1` — Added `--profile apps` flag
- `scripts/dev-down.sh` — Added `--profile apps` flag
- `scripts/dev-reset.ps1` — Added `--profile apps` flag
- `scripts/dev-reset.sh` — Added `--profile apps` flag
- `CLAUDE.md` — Updated Key Commands section
- `AGENTS.md` — Updated Key Commands section
- `GEMINI.md` — Updated Key Commands section

## Verification
- `docker compose --profile apps up --build -d` — All 8 containers started
- `docker compose --profile apps ps` — All healthy
- `curl http://localhost:5010/health/ready` — Healthy (PostgreSQL, Redis, RabbitMQ)
- `curl http://localhost:5020/health/ready` — Healthy (PostgreSQL, Redis, RabbitMQ)
- `curl http://localhost:3000` — HTTP 307 (redirect to locale)
- `curl http://127.0.0.1:3000` — HTTP 307 (both hostnames work)

## Checklist
- [x] Task scope matches `docs/tasks/ft-0004-docker-dev-services.md`
- [x] All services running and healthy in Docker
- [x] Backward-compatible (infra-only mode unchanged)
- [x] Docs updated
- [x] No secrets committed
