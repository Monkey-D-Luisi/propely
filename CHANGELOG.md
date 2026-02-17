## [1.1.0](https://github.com/Monkey-D-Luisi/saas-template/compare/v1.0.0...v1.1.0) (2026-02-17)

### Features

* **arch:** add FluentValidation command validators for defense-in-depth ([cfbc04f](https://github.com/Monkey-D-Luisi/saas-template/commit/cfbc04f09a54df4706aa4b426f21ef3ec7121366))
* **arch:** add LoggingBehaviour to MediatR pipeline ([3b555bc](https://github.com/Monkey-D-Luisi/saas-template/commit/3b555bc2e5e1db9e018e120c3081764054037dc7))
* **arch:** add pagination to payment history endpoint ([714a499](https://github.com/Monkey-D-Luisi/saas-template/commit/714a49912ecf332414302ddbddbe6a02492208c9))

### Bug Fixes

* **arch:** use ExecuteUpdateAsync for bulk invitation cancellation ([1a43bcc](https://github.com/Monkey-D-Luisi/saas-template/commit/1a43bcc24fa454edb4ba2d7ab6e99105bd11b18d))
* **audit-logs:** cap pageSize, push export limit to DB, fix JsonSerializerOptions allocation ([a9fe620](https://github.com/Monkey-D-Luisi/saas-template/commit/a9fe6201e3c4ffc0c266eb82958dd5a46edcfd35))
* **ci:** add EF migration for invitation index and update integration tests ([669c5f3](https://github.com/Monkey-D-Luisi/saas-template/commit/669c5f31e86aa719bf90de93d9ccac1d8f314cd3))
* **docs:** add Swagger UI in development and fix placeholder URLs ([406a808](https://github.com/Monkey-D-Luisi/saas-template/commit/406a808d97e90cc84b08c152944f21c81f2028d0))
* **ops:** add health check log filter and bind ports to localhost ([5074119](https://github.com/Monkey-D-Luisi/saas-template/commit/5074119c353e0aca3ff1d7cd7028f4aea0117beb))
* **perf:** add composite index on invitations for common queries ([fb45c7d](https://github.com/Monkey-D-Luisi/saas-template/commit/fb45c7d88451cf2960f3dc43558c9425f94c7d3d))
* **perf:** batch outbox SaveChangesAsync for processed messages ([0347af5](https://github.com/Monkey-D-Luisi/saas-template/commit/0347af5ef39637dc4f5bf0b7458dbced7f58afa2))
* **perf:** batch-load memberships and orgs in DeleteAccount handler ([d686c7d](https://github.com/Monkey-D-Luisi/saas-template/commit/d686c7dc5de33027fc11230e1d580fab149d5d7c))
* **perf:** switch to AddDbContextPool for connection reuse ([53870bb](https://github.com/Monkey-D-Luisi/saas-template/commit/53870bbb2b46debe0831a2df9a27bdcc84f6abd4))
* **review:** address PR [#307](https://github.com/Monkey-D-Luisi/saas-template/issues/307) review feedback ([#cr](https://github.com/Monkey-D-Luisi/saas-template/issues/cr)-0081) ([48577a3](https://github.com/Monkey-D-Luisi/saas-template/commit/48577a3edcf1fc672832d26e4b91e3c2d7f7e18c)), closes [#cr-0081](https://github.com/Monkey-D-Luisi/saas-template/issues/cr-0081)
* **security:** add org membership verification to X-Org-Id header ([d55dabc](https://github.com/Monkey-D-Luisi/saas-template/commit/d55dabc7e5bb9658f3d906df0ae4673c46e509de))
* **security:** add request body size limit to Stripe webhook endpoint ([8a96dc6](https://github.com/Monkey-D-Luisi/saas-template/commit/8a96dc67e1dd09b77b005ad11c4ec2820d71bd6d))
* **security:** explicitly set BCrypt work factor to 12 ([8f5885c](https://github.com/Monkey-D-Luisi/saas-template/commit/8f5885cb25633626db486be40f034d9e2b3e38ab))
* **security:** log warning when CSRF falls back to JWT secret ([246f3f2](https://github.com/Monkey-D-Luisi/saas-template/commit/246f3f2e0355b68e81dcd9f59816f1171c525c05))
* **security:** mask email PII in failed login log messages ([6d98ae8](https://github.com/Monkey-D-Luisi/saas-template/commit/6d98ae882b588bfaf21b3add97caaf40107f876e))
* **security:** replace ILike with exact equality in FeatureFlag lookup ([e9821a3](https://github.com/Monkey-D-Luisi/saas-template/commit/e9821a32926ef698317eb558f5867bd921709458))
* **security:** stop returning invitation URL in API response ([50742aa](https://github.com/Monkey-D-Luisi/saas-template/commit/50742aaebbe78ca2089360216e707a1e8f7eea08))
* **ux:** accessibility and code quality improvements across frontend ([7665ab9](https://github.com/Monkey-D-Luisi/saas-template/commit/7665ab9cf8d852eb5f804b00d18c24efcbc766b7))

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-17

### Initial Release

The complete SaaS Starter Kit — a production-ready monorepo with authentication, billing, teams, AI-powered forms, and full cloud infrastructure.

#### Frontend (Next.js 16 + Tailwind CSS v4)
- Landing page with pricing section
- Full authentication UI: login, register, forgot/reset password, email verification
- OAuth social login (Google, GitHub)
- Organization management: create, settings, delete
- Team member management: invite, change role, remove, leave
- User profile with name editing and password change
- Billing management with Stripe integration
- Admin panel: feature flags, audit log viewer with filtering and export
- Work items CRUD with AI Smart Fill (7 fields: title, description, status, priority, type, due date, estimated effort)
- Multi-tenant work items with automatic organization context
- In-app notifications system
- Internationalization (English and Spanish)
- Responsive design system with semantic design tokens
- Error boundaries, custom error pages, loading skeletons
- Accessibility audit applied across all screens

#### Backend — Orgs API (.NET 10)
- Clean Architecture + CQRS (MediatR)
- Authentication: register, login, logout, refresh token rotation
- Email verification and password reset flows
- OAuth provider integration (Google, GitHub) with account linking
- Organization CRUD with soft delete
- Membership management with RBAC (Owner, Admin, Member)
- Invitation system with email delivery
- Stripe billing: subscriptions, one-time payments, webhook handling, entitlement enforcement
- Feature flags infrastructure
- Audit logging with structured events
- Rate limiting (per-endpoint and global)
- CSRF protection (HMAC-signed tokens with TTL)
- Transactional emails via SMTP (dev) or SendGrid (production)
- Localized email templates (English and Spanish)
- Multi-tenancy with EF Core global query filters
- Pagination and search on list endpoints
- Auto-migration on startup with advisory locking

#### Backend — AI API (.NET 10)
- Clean Architecture + CQRS (MediatR)
- OpenAI integration with graceful degradation
- AI Smart Fill: natural language input auto-populates structured work item fields (priority, type, due date, estimated effort)
- Work items domain with CRUD operations and CQRS read model projection
- EF Core migrations for work item schema (including priority, type, due date, effort columns)
- Multi-tenant isolation via OrgContext middleware (X-Org-Id header)
- Cross-origin Bearer token authentication with automatic refresh
- CORS configuration for cross-origin requests

#### Infrastructure — Local Development
- Docker Compose with PostgreSQL 16, RabbitMQ 3, Redis 7, MailHog, Aspire Dashboard
- One-command bootstrap for developer onboarding (PowerShell + Bash)
- Cross-platform scripts: dev-up, dev-down, dev-reset, run per-service
- Healthchecks on all containers
- Seed scripts for development and demo data
- OpenTelemetry tracing via Aspire Dashboard

#### Infrastructure — Cloud (GCP via Terraform)
- Cloud Run deployment for all three services
- Cloud SQL (PostgreSQL) with Auth Proxy
- Memorystore (Redis) for caching
- Secret Manager with version pinning
- VPC networking
- IAM with least-privilege service accounts
- Workload Identity Federation for keyless CI/CD authentication
- Separate staging and production environments
- Bootstrap module for initial GCP project setup

#### CI/CD (GitHub Actions)
- Path-based change detection for targeted builds
- Unit, integration, and architecture tests
- E2E tests with Playwright against full Docker stack
- Code coverage with minimum threshold enforcement
- Vulnerability scanning: NuGet audit, npm audit, Trivy container scan
- Container image publishing to GHCR
- Automated deployment to Cloud Run (staging on merge, production on tag)
- Semantic versioning with automated releases
- Rollback workflow with manual trigger
- Terraform validation workflow
- Self-hosted Windows runner support with Git Bash shell configuration

#### Documentation
- Comprehensive README with architecture diagrams and competitor comparison
- Quick start guide (QUICKSTART.md)
- Configuration reference (docs/configuration.md)
- Developer cookbook with extension recipes (docs/cookbook.md)
- Production hardening checklist (docs/production-hardening.md)
- Disaster recovery runbook (docs/recovery-runbook.md)
- Licensing guide in plain language (docs/licensing-guide.md)
- Demo video recording guide with AI Smart Fill scene script
- Third-party license notices (auto-generated)

#### Legal
- Proprietary license (LICENSE)
- End User License Agreement (EULA.md)
- Privacy policy and terms of service pages
- License headers on all source files
