# Task: 0056 - Getting Started Guide (15-Min Quickstart)

## Metadata
- ID: 0056
- Type: Documentation
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #208
- Epic: `docs/backlog/epic-010-docs-onboarding.md`
- Old Issue: #42
- Milestone: v1.0

## Goal
Write a step-by-step getting started guide that takes a developer from cloning the repo to a fully running application in under 15 minutes.

## Context
New developers picking up this SaaS template need clear instructions to get up and running. The project has Docker Compose orchestration, multiple services, and environment configuration. Without a guide, the setup process involves reading multiple files to understand the stack.

### Current Resources
- `CLAUDE.md`: Agent instructions with commands
- `scripts/`: dev-up, dev-down, dev-reset, run-* scripts
- `.env`: Environment configuration
- `docker-compose.yml`: Infrastructure and services

## Scope
### In scope
- Prerequisites section (Docker, Node.js, .NET SDK, Git)
- Clone and initial setup (references `make dev` / `bootstrap.ps1` from task 0072)
- Environment configuration (.env) with section explaining key variable groups:
  - Infrastructure (Postgres, RabbitMQ, Redis)
  - Auth (JWT, CSRF, OAuth providers)
  - Billing (Stripe mode, keys, webhooks)
  - Email (provider, SendGrid/SMTP)
  - AI (OpenAI API key, model)
- Starting infrastructure (docker-compose)
- Starting services (individually or all at once)
- Verifying everything works (health checks, web UI)
- Seeding development data
- Configuring Stripe billing (test mode, webhook forwarding with Stripe CLI)
- Configuring OAuth providers (Google Cloud Console, GitHub OAuth App — step by step)
- First deployment to staging (Terraform init + apply, referencing task 0054)
- Troubleshooting common issues
- Quick tour of the running application

### Out of scope
- Architecture deep-dive (task 0057 cookbook)
- Production hardening (task 0073)
- Contributing guide

## Requirements
- R1: A developer with prerequisites can go from clone to running app following the guide
- R2: All commands are copy-pasteable
- R3: Both Windows (PowerShell) and macOS/Linux (bash) commands provided
- R4: Troubleshooting section covers common issues
- R5: Guide is concise but complete

## Acceptance Criteria
- AC1: Document exists at `docs/getting-started.md`
- AC2: Following the guide results in a running application
- AC3: All three services accessible (web:3000, ai-api:5010, orgs-api:5020)
- AC4: Both Windows and Unix commands provided
- AC5: Troubleshooting section covers: port conflicts, Docker issues, .env problems

## Implementation Steps

1. **Write prerequisites section**: Docker Desktop, Node.js 20+, .NET 10 SDK, Git
2. **Write clone & setup section**: git clone, npm install, dotnet restore
3. **Write configuration section**: Copy .env, explain key variables
4. **Write startup section**: dev-up scripts, verify health checks
5. **Write verification section**: Open web app, register, create org
6. **Write seed data section**: Run seed-dev script
7. **Write troubleshooting section**: Common issues and fixes
8. **Test the guide**: Follow it from scratch on a clean machine

## Files to Create

- `docs/getting-started.md`
- `docs/walkthroughs/0056-getting-started.md`

## Definition of Done Checklist
- [x] Guide tested by following from scratch
- [x] Both OS variants documented
- [x] Troubleshooting covers common issues
- [x] Walkthrough updated
