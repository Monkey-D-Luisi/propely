# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Propely product roadmap with phases P0-P7
- Backlog epic files for all development phases
- Architecture documentation for 6-service topology
- Agent workflow documentation (autonomous, code review, fast track, PR)
- Getting-started guide for Propely onboarding

### Changed
- Rebranded from SaaS Starter Kit to Propely
- Updated all documentation for Propely's real estate management domain
- Updated agent instructions (`.agent.md`, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`)
- Updated `README.md` with Propely architecture and setup

### Removed
- Template-era documentation artifacts (old tasks, walkthroughs, audits)

## [1.0.0] - 2026-02-17

### Initial Release

Foundation inherited from SaaS Starter Kit, serving as the base for Propely development.

#### Frontend (Next.js 16 + Tailwind CSS v4)
- Full authentication UI: login, register, forgot/reset password, email verification
- OAuth social login (Google, GitHub)
- Organization management: create, settings, delete
- Team member management: invite, change role, remove, leave
- User profile with name editing and password change
- Billing management with Stripe integration
- Internationalization (English and Spanish)
- Responsive design system with semantic design tokens

#### Backend -- Orgs API (.NET 10)
- Clean Architecture + CQRS (MediatR)
- Authentication: register, login, logout, refresh token rotation
- Email verification and password reset flows
- OAuth provider integration (Google, GitHub) with account linking
- Organization CRUD with soft delete
- Membership management with RBAC (Owner, Admin, Member)
- Invitation system with email delivery
- Stripe billing: subscriptions, one-time payments, webhook handling
- Multi-tenancy with EF Core global query filters

#### Backend -- AI API (.NET 10)
- Clean Architecture + CQRS (MediatR)
- OpenAI integration with graceful degradation
- Multi-tenant isolation via OrgContext middleware

#### Infrastructure
- Docker Compose with PostgreSQL 16, RabbitMQ 3, Redis 7, MailHog, Aspire Dashboard
- One-command bootstrap for developer onboarding
- OpenTelemetry tracing via Aspire Dashboard
- GCP Terraform modules for Cloud Run deployment

#### CI/CD (GitHub Actions)
- Unit, integration, and architecture tests
- Vulnerability scanning: NuGet audit, npm audit, Trivy container scan
- Container image publishing to GHCR
- Automated deployment to Cloud Run
