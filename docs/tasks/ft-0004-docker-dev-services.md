# Task: ft-0004 — Docker Dev Services

## Metadata
- ID: ft-0004
- Type: Fast Track
- Status: DONE
- Created: 2026-02-06
- Walkthrough: `docs/walkthroughs/ft-0004-docker-dev-services.md`

## Goal
Add all three application services (ai-api, orgs-api, web) to Docker Compose with hot reload for development, so `docker compose --profile apps up` starts the full stack.

## Context
The monorepo had Docker Compose for infrastructure only (Postgres, RabbitMQ, Redis, Aspire, Mailhog). Application services were run individually via scripts on the host. The user requested all services to run in Docker.

## Scope
### In scope
- Dev Dockerfiles (`Dockerfile.dev`) for ai-api, orgs-api, web
- `.dockerignore` files for each service
- `.env.docker` override file for container-name-based hostnames
- Docker Compose services with profiles, health checks, volume mounts
- Script updates (`dev-up` with `-Apps`/`--apps`, `dev-down`, `dev-reset`)
- Documentation updates (CLAUDE.md, AGENTS.md, GEMINI.md)

### Out of scope
- Production Dockerfiles (task 0020)
- CI/CD pipelines
- Kubernetes manifests

## Acceptance Criteria
- AC1: `docker compose --profile apps up --build` starts all 3 app services + infrastructure
- AC2: `docker compose up` still starts only infrastructure (backward-compatible)
- AC3: All health checks pass (ai-api, orgs-api, web)
- AC4: Hot reload works (edit source, container rebuilds automatically)
- AC5: Host-based development still works (existing run scripts unchanged)

## Files to Create / Modify
See walkthrough for full list.
