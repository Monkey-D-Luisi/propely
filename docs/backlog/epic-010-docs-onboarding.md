# Epic 010: Documentation & Onboarding

## Overview

Create user-facing documentation for developers who will use this SaaS template. Includes a comprehensive getting started guide, developer cookbook with extension recipes, and an enhanced README with screenshots and badges.

## Success Criteria

- A developer can go from clone to running app in under 15 minutes following the guide
- Cookbook covers common extension scenarios (add a new entity, add a new API endpoint, add a new page, add a new module)
- README is visually appealing with screenshots, architecture diagram, and CI badges
- Documentation is in English with clear, concise writing

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0056 - Getting started guide (Buyer Quickstart)
- **Status:** DONE
- **GitHub Issue:** #208
- **Dependencies:** 0049, 0066, 0072
- **File:** `docs/tasks/0056-getting-started.md`
- **Scope:** Write step-by-step buyer quickstart: prerequisites, clone, one-command setup (`make dev`), configure .env for billing/OAuth/email, Stripe test mode with Stripe CLI, OAuth provider setup (Google + GitHub step by step), first deployment to staging via Terraform. Include troubleshooting. Target: clone to working app in 15 minutes.
- **Old Issue:** #42
- **Roadmap Phase:** C4

### Task 0057 - Developer cookbook / extension recipes
- **Status:** DONE
- **GitHub Issue:** #209
- **Dependencies:** 0056
- **File:** `docs/tasks/0057-developer-cookbook.md`
- **Scope:** Write recipes: add a new domain entity, add a new CQRS command/query, add a new API endpoint, add a new frontend page, add a new module (scaffold-module.ps1), add a new locale, configure feature flags.
- **Old Issue:** #43
- **Roadmap Phase:** C5

### Task 0058 - README with screenshots and badges (Buyer README)
- **Status:** DONE
- **GitHub Issue:** #210
- **Dependencies:** 0056, 0071
- **File:** `docs/tasks/0058-readme-screenshots.md`
- **Scope:** Professional buyer-facing README: value proposition with differentiators, complete feature matrix, architecture diagram, 10+ screenshots, CI/tech stack badges, comparison table vs competing templates, quick links to all documentation (getting started, cookbook, configuration, production hardening, recovery runbook).
- **Old Issue:** #52
- **Roadmap Phase:** C6

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Documentation | 0056-0058 | 3 | 0 |
| **Total** | **3** | **3** | **0** |

## Dependency Graph

```
0049 (Work items, epic 007) ──► 0056 (Getting started) ──┬──► 0057 (Cookbook)
0066 (.env.example, epic 013) ─┘                         └──► 0058 (README)
0072 (One-command, epic 013) ──┘                              │
                                                 0071 (Rebrand, epic 013) ─┘
```
