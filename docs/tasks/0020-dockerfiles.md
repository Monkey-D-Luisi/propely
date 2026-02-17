# Task: 0020 - Dockerfiles for All Services

## Metadata
- ID: 0020
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0020-dockerfiles.md`

## Goal
Create optimized multi-stage Dockerfiles for ai-api, orgs-api, and web. Add a docker-compose.production.yml that builds and runs all services. Optimize image sizes with proper .dockerignore files.

## Context
The monorepo has Docker Compose for infrastructure (Postgres, Redis, RabbitMQ) but no Dockerfiles for the actual services. To be production-ready, every service needs a containerized build that can be deployed anywhere.

## Scope
### In scope
- Multi-stage Dockerfile for orgs-api (.NET 10): restore, build, publish, runtime
- Multi-stage Dockerfile for ai-api (.NET 10): restore, build, publish, runtime
- Multi-stage Dockerfile for web (Next.js 16): install, build, standalone runner
- `.dockerignore` files for each service
- `docker-compose.production.yml` that builds all services and connects to infra
- Use minimal base images (alpine variants where possible)
- Configure non-root user in runtime stage
- Expose correct ports

### Out of scope
- Kubernetes manifests
- CI/CD deployment pipeline
- Container registry push
- SSL/TLS termination (handled by reverse proxy)

## Requirements
- R1: .NET Dockerfiles use `mcr.microsoft.com/dotnet/sdk:10.0` for build and `mcr.microsoft.com/dotnet/aspnet:10.0` for runtime
- R2: Next.js Dockerfile uses `node:22-alpine` and Next.js standalone output
- R3: Final images run as non-root user
- R4: .NET images restore only .csproj files first (layer caching for NuGet)
- R5: Next.js image uses `next.config.ts` output: 'standalone' mode
- R6: `docker compose -f docker-compose.production.yml up --build` starts everything

## Acceptance Criteria
- AC1: `docker build -f services/orgs-api/Dockerfile .` succeeds from repo root
- AC2: `docker build -f services/ai-api/Dockerfile .` succeeds from repo root
- AC3: `docker build -f apps/web/Dockerfile .` succeeds from repo root
- AC4: `docker compose -f docker-compose.production.yml up --build` starts all services
- AC5: All services respond to health checks in containers
- AC6: Final image sizes are reasonable (< 200MB for .NET, < 300MB for Next.js)

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- No hardcoded credentials in Dockerfiles (use env vars).
- Update walkthrough.

## Proposed Approach (high-level)
1. Create .NET Dockerfile pattern (shared between both APIs)
2. Create Next.js Dockerfile
3. Create .dockerignore for each
4. Create docker-compose.production.yml
5. Test full stack build

## Implementation Steps
1. Create `services/orgs-api/Dockerfile`:
   ```dockerfile
   # Stage 1: Restore
   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
   WORKDIR /src
   COPY *.sln .
   COPY src/*/*.csproj ./
   # restore trick: create directory structure from csproj paths
   RUN dotnet restore

   # Stage 2: Build
   FROM restore AS build
   COPY src/ src/
   RUN dotnet publish src/SaasTemplate.OrgsApi.Api -c Release -o /app/publish

   # Stage 3: Runtime
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
   RUN adduser --disabled-password --gecos "" appuser
   USER appuser
   WORKDIR /app
   COPY --from=build /app/publish .
   EXPOSE 8080
   ENTRYPOINT ["dotnet", "SaasTemplate.OrgsApi.Api.dll"]
   ```
2. Create `services/ai-api/Dockerfile` (same pattern)
3. Create `apps/web/Dockerfile`:
   ```dockerfile
   FROM node:22-alpine AS deps
   WORKDIR /app
   COPY package*.json ./
   RUN npm ci

   FROM node:22-alpine AS builder
   WORKDIR /app
   COPY --from=deps /app/node_modules ./node_modules
   COPY . .
   RUN npm run build

   FROM node:22-alpine AS runner
   RUN adduser -D appuser
   USER appuser
   WORKDIR /app
   COPY --from=builder /app/.next/standalone ./
   COPY --from=builder /app/.next/static ./.next/static
   COPY --from=builder /app/public ./public
   EXPOSE 3000
   CMD ["node", "server.js"]
   ```
4. Create `.dockerignore` files for each service
5. Create `docker-compose.production.yml`
6. Add `output: 'standalone'` to `next.config.ts` if not already present
7. Test full build

## Files to Create / Modify
- `services/orgs-api/Dockerfile` (create)
- `services/orgs-api/.dockerignore` (create)
- `services/ai-api/Dockerfile` (create)
- `services/ai-api/.dockerignore` (create)
- `apps/web/Dockerfile` (create)
- `apps/web/.dockerignore` (create)
- `docker-compose.production.yml` (create)
- `apps/web/next.config.ts` (modify - add standalone output if needed)

## Testing Plan
- Manual verification: Build each Dockerfile, run docker-compose.production.yml
- Verify health check endpoints respond

## Security & Privacy
- Runtime images use non-root user
- No secrets baked into images
- All config via environment variables

## Observability
- Container logs write to stdout/stderr (docker logging driver picks them up)

## Rollback Plan
Remove Dockerfiles and compose file. No impact on development workflow.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
