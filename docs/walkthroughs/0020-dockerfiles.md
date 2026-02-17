# Walkthrough: 0020 - Dockerfiles for All Services

**Date:** 2026-02-07
**Branch:** `feat/0020-dockerfiles`

## Summary

Created optimized multi-stage production Dockerfiles for all three services (ai-api, orgs-api, web). Added a `docker-compose.production.yml` that builds and runs the full stack. Added a root `.dockerignore` to prevent host build artifacts from breaking Docker builds.

## What Changed

### Dockerfiles Created

#### `.NET Services` (ai-api, orgs-api)

Both .NET Dockerfiles follow the same 3-stage pattern:

| Stage | Base Image | Purpose |
|-------|-----------|---------|
| `restore` | `mcr.microsoft.com/dotnet/sdk:10.0-alpine` | Copy .csproj files only, `dotnet restore` (layer-cached) |
| `build` | (continues from restore) | Copy source, `dotnet publish -c Release` |
| `runtime` | `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` | Non-root user, copy published output, expose 8080 |

**Files:**
- `services/ai-api/Dockerfile`
- `services/orgs-api/Dockerfile`

**Key design decisions:**
- Build context is repo root (not service directory) so paths include `services/<name>/`
- `.csproj` files are copied individually before restore for optimal layer caching
- Alpine variants used for minimal image size
- Non-root user (`appuser:appgroup`) for security
- `--no-restore` flag on publish to avoid double restore

#### `Web App` (Next.js)

3-stage build for the Next.js standalone output:

| Stage | Base Image | Purpose |
|-------|-----------|---------|
| `deps` | `node:22-alpine` | Copy package.json + lock, `npm ci` |
| `builder` | `node:22-alpine` | Copy source, `npm run build` |
| `runner` | `node:22-alpine` | Non-root user, copy standalone + static + public |

**File:** `apps/web/Dockerfile`

**Key design decisions:**
- `npm ci --ignore-scripts` for reproducible dependency installation
- `NEXT_TELEMETRY_DISABLED=1` in build and runtime stages
- Standalone output copies only: `.next/standalone/`, `.next/static/`, `public/`
- `node server.js` as entrypoint (no npm overhead)

### docker-compose.production.yml

Full production compose file with infrastructure and all application services:

| Service | Image/Build | Port |
|---------|-------------|------|
| postgres | `postgres:16-alpine` | 5432 (internal) |
| rabbitmq | `rabbitmq:3-management-alpine` | 5672 (internal) |
| redis | `redis:7-alpine` | 6379 (internal) |
| ai-api | Built from `services/ai-api/Dockerfile` | `${AI_API_PORT:-5010}` → 8080 |
| orgs-api | Built from `services/orgs-api/Dockerfile` | `${ORGS_API_PORT:-5020}` → 8080 |
| web | Built from `apps/web/Dockerfile` | `${WEB_PORT:-3000}` → 3000 |

**Features:**
- All config via environment variable substitution (no secrets in file)
- Health checks on all services (infrastructure + application)
- `depends_on` with `condition: service_healthy` ensures startup ordering
- Named volumes with `_prod_` prefix to avoid conflicts with dev volumes
- Container names use `saastemplate-prod-` prefix

### Root .dockerignore

**File:** `.dockerignore`

Required because production Dockerfiles build from repo root context (not service directories). Without this, host `obj/` directories (containing Windows-specific `project.assets.json`) were copied into the Docker build and overwrote the Docker-restored assets, causing NuGet restore failures referencing Windows paths inside Linux containers.

**Excludes:** `**/bin/`, `**/obj/`, `**/node_modules/`, `apps/web/.next/`, `services/*/tests/`, docs, IDE files, env files (except `.env.example`), scripts.

### next.config.ts Modification

Added `output: 'standalone'` to enable Next.js standalone mode, which produces a self-contained output directory that can run without `node_modules`.

## Image Sizes

| Image | Size | Base Image Size | App Layer |
|-------|------|-----------------|-----------|
| orgs-api | 203 MB | ~185 MB (aspnet:10.0-alpine) | ~24 MB |
| ai-api | 209 MB | ~185 MB (aspnet:10.0-alpine) | ~24 MB |
| web | 333 MB | ~172 MB (node:22-alpine) | ~80 MB |

Compared to dev images: .NET reduced from 2.4 GB to ~206 MB (12x), web reduced from 1.6 GB to 333 MB (5x).

## Issues Encountered

### Windows obj/ Poisoning in Docker Builds

**Problem:** First Docker build attempt failed with `Unable to find fallback package folder 'C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages'`. Host Windows `obj/` directories (containing `project.assets.json` with Windows-specific NuGet paths) were copied into the Linux Docker container via `COPY services/*/src/ services/*/src/`, overwriting the Docker-restored assets.

**Root cause:** Per-service `.dockerignore` files only apply when the service directory is the build context. When building from repo root (required for our Dockerfile structure), only the root `.dockerignore` is used.

**Fix:** Created `.dockerignore` at repo root with `**/bin/` and `**/obj/` exclusions.

## Verification

```
Docker builds:
  orgs-api:  Built successfully (203 MB)
  ai-api:    Built successfully (209 MB)
  web:       Built successfully (333 MB)

Smoke tests:
  ai-api:    Container starts, reaches infrastructure connection phase
  web:       Container starts, responds HTTP 307 on port 3000

Existing tests (no regressions):
  ai-api:    137 passed (76 unit + 5 architecture + 56 integration)
  orgs-api:  146 passed (90 unit + 5 architecture + 51 integration)
  web:       Build successful, all pages compiled
```

## Files Created

- `services/ai-api/Dockerfile`
- `services/orgs-api/Dockerfile`
- `apps/web/Dockerfile`
- `docker-compose.production.yml`
- `.dockerignore`

## Files Modified

- `apps/web/next.config.ts` (added `output: 'standalone'`)
