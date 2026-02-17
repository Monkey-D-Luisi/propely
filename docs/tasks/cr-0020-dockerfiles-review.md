# Code Review: cr-0020 - Dockerfiles PR Review

## PR Metadata
- **PR:** #160
- **Branch:** `feat/0020-dockerfiles` → `main`
- **CI Status:** All 4 checks passed (Detect Changes, AI API Build & Test, Orgs API Build & Test, Web Build)

## Changed Files
- `.agent/rules/autonomous-workflow.md`
- `.dockerignore`
- `apps/web/Dockerfile`
- `apps/web/next.config.ts`
- `docker-compose.production.yml`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/walkthroughs/0020-dockerfiles.md`
- `services/ai-api/Dockerfile`
- `services/orgs-api/Dockerfile`

## Review Comments

| # | Source | Reviewer | File | Summary |
|---|--------|----------|------|---------|
| 1 | Inline | Codex (P1) | `docker-compose.production.yml:138` | NEXT_PUBLIC_* must be set at build time, not just runtime |
| 2 | Inline | Gemini (security-medium) | `docker-compose.production.yml:108` | Shared infra credentials across services violates least privilege |
| 3 | Inline | Gemini (medium) | `apps/web/Dockerfile:11` | Copy only needed files for dependency install |
| 4 | Inline | Gemini (medium) | `docker-compose.production.yml:147` | Use `wget --spider` instead of `wget -qO-` for healthchecks |
| 5 | Issue | User | — | PR should be the very last step so updated docs are included |

## Comment Resolution Plan

### MUST_FIX

- [ ] **#1 - NEXT_PUBLIC_* build args**: Pass NEXT_PUBLIC_* as build args in docker-compose.production.yml and declare ARG/ENV in web Dockerfile so they're inlined during `next build`. Valid concern — NEXT_PUBLIC_* are compile-time constants.
- [ ] **#5 - Workflow step ordering**: Swap Steps 8 and 9 in autonomous-workflow.md so "Mark Task Complete" (commit docs) happens before "Create Pull Request" (push + PR). This ensures the DONE status and final walkthrough are included in the PR.

### SHOULD_FIX

- [ ] **#4 - wget --spider healthcheck**: Use `wget -q --spider` instead of `wget -qO-` for web healthcheck. More efficient — sends HEAD request instead of downloading body.

### OUT_OF_SCOPE

- [ ] **#2 - Per-service credentials**: Valid for hardened production but adds significant complexity (separate Postgres users, init scripts). The compose file uses env var substitution so operators can set different credentials. Real production would use a secrets manager, not compose env vars. Deferred.

### NO_ACTION

- [ ] **#3 - Dockerfile caching**: Gemini suggests exactly what's already done. The deps stage already copies only `package.json` and `package-lock.json`. The builder stage copies all source which is expected. No change needed.
