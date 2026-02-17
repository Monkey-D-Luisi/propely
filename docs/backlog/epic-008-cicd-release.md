# Epic 008: CI/CD & Release Automation

## Overview

Extend the existing CI pipeline with container image publishing to GHCR, semantic release for automated versioning, and E2E smoke tests with Playwright. These capabilities are needed for a production-grade deployment pipeline.

## Success Criteria

- Docker images automatically published to GHCR on main branch push
- Semantic versioning with automated changelog generation
- E2E smoke tests run in CI (basic happy-path flows)
- All three services have tagged images in GHCR
- Release tags created automatically based on commit messages

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Container registry | GHCR | Free for public repos, integrated with GitHub |
| Versioning | semantic-release | Automated based on conventional commits (already used) |
| E2E testing | Playwright | Cross-browser support, good CI integration, fast |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0050 - GHCR container image publishing
- **Status:** DONE
- **GitHub Issue:** #202
- **Dependencies:** None
- **File:** `docs/tasks/0050-ghcr-publishing.md`
- **Scope:** Add GitHub Actions workflow to build and push Docker images to GHCR. Tag with commit SHA and semantic version. Push on main branch merge. Add image scanning step.
- **Old Issue:** #39

### Task 0051 - Semantic release and versioning
- **Status:** DONE
- **GitHub Issue:** #203
- **Dependencies:** 0050
- **File:** `docs/tasks/0051-semantic-release.md`
- **Scope:** Configure semantic-release for monorepo. Auto-generate changelog from conventional commits. Create GitHub releases with tags. Update version in package.json / .csproj files.
- **Old Issue:** #40
- **Roadmap Phase:** B3.1

### Task 0052 - E2E smoke tests (Playwright)
- **Status:** DONE
- **GitHub Issue:** #204
- **Dependencies:** 0051
- **File:** `docs/tasks/0052-e2e-smoke-tests.md`
- **Scope:** Set up Playwright in web app. Write smoke tests for critical flows: register, login, create org, invite member. Configure CI to run E2E tests against docker-compose. Add test report to CI artifacts.
- **Old Issue:** #36
- **Roadmap Phase:** B3.2

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| CI/CD | 0050-0052 | 3 | 0 |
| **Total** | **3** | **3** | **0** |

## Dependency Graph

```
0050 (GHCR) ──┬──► 0051 (Semantic release) ──► 0052 (E2E tests)
              └──► 0054 (Cloud Run, epic 009)
```
