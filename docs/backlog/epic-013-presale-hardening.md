# Epic 013: Pre-Sale Product Hardening

## Overview

Close all product-level gaps that a buyer would encounter in their first session with the template. This epic addresses configuration completeness, legal compliance, UX polish, branding, and developer experience — ensuring that every sale converts into a positive review.

This epic was created after a deep-dive audit comparing the implemented codebase against buyer expectations for commercial SaaS starter kits (ShipFast, SaaS Pegasus, Next.js SaaS Boilerplates). The engineering is solid (Epics 001-009), but the product packaging has gaps that would cause friction or negative perception.

## Success Criteria

- A buyer can configure the full stack (auth, billing, email, OAuth) using only `.env.example` and inline comments — no guessing
- Zero personal data or author-specific references in the distributed codebase
- Privacy Policy and Terms of Service pages render with professional, real content
- Account deletion works end-to-end with a polished UI flow (Stitch design, modal confirmation, feedback)
- `THIRD_PARTY_NOTICES.md` lists all redistributed dependency licenses
- Product branding is centralized and renamed from "SaaS Template" to the definitive product name
- A new buyer can go from clone to running app with a single command (`make dev` or equivalent)
- Production hardening and recovery documentation exists for Stripe, JWT, CSRF secret rotation and Cloud SQL restore

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0066 - Complete .env.example and configuration guide
- **Status:** DONE
- **GitHub Issue:** #271
- **Dependencies:** None
- **File:** `docs/tasks/0066-env-example-config.md`
- **Scope:** Add all missing environment variables to `.env.example` with descriptive comments: JWT secret, CSRF secret, Stripe config (SecretKey, PublishableKey, WebhookSecret), billing mode, SendGrid API key, email provider mode, OpenAI model ID. Create a companion `docs/configuration.md` explaining every variable, its purpose, and valid values.
- **Roadmap Phase:** C-0.1

### Task 0067 - Neutralize personal data in scripts and examples
- **Status:** DONE
- **GitHub Issue:** #272
- **Dependencies:** None
- **File:** `docs/tasks/0067-neutralize-personal-data.md`
- **Scope:** Audit all scripts, seed files, configuration defaults, and documentation for author-specific data (emails, names, URLs). Replace with neutral placeholders (e.g., `invitee@example.com`, `your-org.example.com`). Includes `scripts/seed-dev.sh`, any Terraform examples, and Docker Compose defaults.
- **Roadmap Phase:** C-0.1

### Task 0068 - Privacy Policy and Terms of Service pages
- **Status:** DONE
- **GitHub Issue:** #273
- **Dependencies:** 0071 (needs product name for legal pages)
- **File:** `docs/tasks/0068-privacy-terms-pages.md`
- **Scope:** Create `/privacy` and `/terms` pages with professionally written, real content specific to a SaaS starter kit product. Content must cover: data collection, cookies, third-party services (Stripe, SendGrid, Google OAuth, GitHub OAuth, OpenAI), user rights (GDPR/RGPD), data retention, and contact information. Pages must be i18n-ready (EN/ES). Follow the existing design system and Stitch workflow.
- **Roadmap Phase:** C-0.3

### Task 0069 - Self-service account deletion (full flow)
- **Status:** DONE
- **GitHub Issue:** #274
- **Dependencies:** 0066
- **File:** `docs/tasks/0069-account-deletion.md`
- **Scope:** Wire up the existing `User.SoftDelete()` domain method into a complete flow: `DELETE /auth/me` endpoint with password confirmation, cascade handling (memberships, owned orgs transfer/delete), confirmation email. Frontend: Stitch design for the profile page danger zone, modal with typed confirmation ("DELETE"), success feedback, and automatic logout. Include i18n (EN/ES).
- **Roadmap Phase:** C-0.4

### Task 0070 - Third-party notices (dependency licenses)
- **Status:** DONE
- **GitHub Issue:** #275
- **Dependencies:** None
- **File:** `docs/tasks/0070-third-party-notices.md`
- **Scope:** Generate `THIRD_PARTY_NOTICES.md` at the repository root listing all redistributed dependencies and their licenses. Cover: .NET NuGet packages (both services), npm packages (web app), and any vendored code. Include a script to regenerate the file (`scripts/generate-third-party-notices.ps1`/`.sh`). Add CI check to warn when notices are stale.
- **Roadmap Phase:** C-0.2

### Task 0071 - Product rebranding
- **Status:** DONE
- **GitHub Issue:** #276
- **Dependencies:** None
- **File:** `docs/tasks/0071-product-rebranding.md`
- **Scope:** Rename from "SaaS Template" to the definitive product name across the entire codebase. Centralize the product name, tagline, and URLs in a single configuration point so future rebranding is trivial. Update: README, package.json, docker-compose.yml, Terraform descriptions, landing page, email templates, EULA/LICENSE, all documentation, CI workflow names, and meta tags. Do NOT rename .NET namespaces or project files (those stay as `SaasTemplate.*` for stability).
- **Roadmap Phase:** C-0.2

### Task 0072 - One-command developer onboarding
- **Status:** DONE
- **GitHub Issue:** #277
- **Dependencies:** 0066
- **File:** `docs/tasks/0072-one-command-onboarding.md`
- **Scope:** Create a single entry point that takes a buyer from clone to running app: `make dev` (cross-platform via Makefile or equivalent). This command should: check prerequisites (Docker, Node, .NET), copy `.env.example` to `.env` if missing, pull/build containers, start infrastructure, wait for health checks, seed dev data, and open the browser. Support Windows (PowerShell), macOS, and Linux. Keep existing `dev-up` scripts as the underlying implementation.
- **Roadmap Phase:** C-0.3

### Task 0073 - Secret rotation and production hardening guide
- **Status:** DONE
- **GitHub Issue:** #278
- **Dependencies:** 0066
- **File:** `docs/tasks/0073-secret-rotation-guide.md`
- **Scope:** Write `docs/production-hardening.md` covering: JWT secret rotation (zero-downtime with dual-key window), CSRF secret rotation, Stripe webhook secret rotation, OAuth client secret rotation, database credential rotation via Secret Manager, pin Secret Manager versions in Terraform (not `latest`), pre-deploy security checklist. Target audience: a developer deploying to GCP for the first time.
- **Roadmap Phase:** C-0.5

### Task 0074 - Recovery runbook
- **Status:** DONE
- **GitHub Issue:** #279
- **Dependencies:** 0073
- **File:** `docs/tasks/0074-recovery-runbook.md`
- **Scope:** Write `docs/recovery-runbook.md` covering: Cloud SQL point-in-time recovery (step-by-step), Cloud Run revision rollback, RabbitMQ message recovery (DLQ replay), Redis cache invalidation, secret compromise response procedure, RPO/RTO targets for each component. Include both CLI commands and Console UI steps.
- **Roadmap Phase:** C-0.6

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Quick wins (C-0.1) | 0066, 0067 | 2 | 0 |
| Branding & legal prep (C-0.2) | 0070, 0071 | 2 | 0 |
| UX & DX (C-0.3-C-0.4) | 0068, 0069, 0072 | 3 | 0 |
| Production docs (C-0.5-C-0.6) | 0073, 0074 | 2 | 0 |
| **Total** | **9** | **9** | **0** |

## Dependency Graph

```
No dependencies (can start immediately):
  0066 (.env.example) ─────────┬──► 0069 (Account deletion)
                                ├──► 0072 (One-command onboarding)
                                └──► 0073 (Secret rotation) ──► 0074 (Recovery runbook)
  0067 (Neutralize data) ────── independent
  0070 (Third-party notices) ── independent
  0071 (Rebranding) ──────────► 0068 (Privacy/Terms pages)

Parallelization opportunities:
  Wave 1: 0066, 0067, 0070, 0071 (all independent)
  Wave 2: 0068, 0069, 0072, 0073 (after their deps)
  Wave 3: 0074 (after 0073)
```
