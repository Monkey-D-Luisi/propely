## [1.1.0](https://github.com/Monkey-D-Luisi/propely/compare/v1.0.0...v1.1.0) (2026-02-22)

### Features

* **orgs:** add authorization policies and permission management API ([#0009](https://github.com/Monkey-D-Luisi/propely/issues/0009)) ([9c9db55](https://github.com/Monkey-D-Luisi/propely/commit/9c9db559365af48034eef7764a7cdac5d2dfec64))

### Bug Fixes

* **orgs:** address PR review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0011) ([08fdd36](https://github.com/Monkey-D-Luisi/propely/commit/08fdd36b900a87f57ecb0e483637792f16bc2248)), closes [#cr-0011](https://github.com/Monkey-D-Luisi/propely/issues/cr-0011)

## 1.0.0 (2026-02-22)

### Features

* **ci:** update CI/CD pipelines for all 6 services ([#0006](https://github.com/Monkey-D-Luisi/propely/issues/0006)) ([2b2a0ab](https://github.com/Monkey-D-Luisi/propely/commit/2b2a0ab7a909f94fecb93d5318ee198a242797d4))
* **infra:** add Docker Compose and infrastructure support for all 6 services ([#0005](https://github.com/Monkey-D-Luisi/propely/issues/0005)) ([2101877](https://github.com/Monkey-D-Luisi/propely/commit/2101877cbc7c3e31623704b0a92a880548a591f7))
* **infra:** rename SaasTemplate to Propely across entire codebase ([#0002](https://github.com/Monkey-D-Luisi/propely/issues/0002)) ([173e0f7](https://github.com/Monkey-D-Luisi/propely/commit/173e0f709958e3b1c6cbbc7a919c8b896520831f))
* **infra:** scaffold properties-api, publishing-api, contacts-api, appointments-api ([#0003](https://github.com/Monkey-D-Luisi/propely/issues/0003)) ([c11b85c](https://github.com/Monkey-D-Luisi/propely/commit/c11b85c59007587af9ea5013ab180d2279e72940))
* **orgs-api:** add Agency entity hierarchy and rename Member to Agent ([#0007](https://github.com/Monkey-D-Luisi/propely/issues/0007)) ([19b8cb2](https://github.com/Monkey-D-Luisi/propely/commit/19b8cb2861279dc2d9a919172f9adb06ae6f1389))
* **orgs:** add NuGet SDK client infrastructure for inter-service communication ([#0004](https://github.com/Monkey-D-Luisi/propely/issues/0004)) ([cd72bff](https://github.com/Monkey-D-Luisi/propely/commit/cd72bff234b25932bd098c74858c4ca1468786f5))
* **orgs:** add permission domain model with role-based defaults and overrides ([#0008](https://github.com/Monkey-D-Luisi/propely/issues/0008)) ([79abc04](https://github.com/Monkey-D-Luisi/propely/commit/79abc04c0868353bd58d077d29056199ff27832b))

### Bug Fixes

* **ci:** add per-service coverage thresholds to CI matrix ([bc0e167](https://github.com/Monkey-D-Luisi/propely/commit/bc0e1676a4c23d827844977c62ba076aef416b90))
* **ci:** address PR [#17](https://github.com/Monkey-D-Luisi/propely/issues/17) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0007) ([3188bdb](https://github.com/Monkey-D-Luisi/propely/commit/3188bdb64f93bf8a0eed43043b38b71c93caa087)), closes [#cr-0007](https://github.com/Monkey-D-Luisi/propely/issues/cr-0007)
* **ci:** build Docker images sequentially to prevent BuildKit crash ([83bddb7](https://github.com/Monkey-D-Luisi/propely/commit/83bddb778d6f0792e8c5cb38d189238d4536b5d6))
* **ci:** handle Windows Hyper-V port reservation in E2E tests ([e3f9358](https://github.com/Monkey-D-Luisi/propely/commit/e3f93581b4f30d038ca902075b1e526edc095ac2))
* **ci:** handle Windows Hyper-V port reservation in E2E tests ([1426b74](https://github.com/Monkey-D-Luisi/propely/commit/1426b74bb67fed6f8edf6ddcd00fb20066c85be1))
* **ci:** use !override for publishing-api port in CI compose ([19a4ad1](https://github.com/Monkey-D-Luisi/propely/commit/19a4ad1c004511be9eecee025fa2af21e16c50df))
* **docs:** address PR [#11](https://github.com/Monkey-D-Luisi/propely/issues/11) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0001) ([82b81a3](https://github.com/Monkey-D-Luisi/propely/commit/82b81a34ed4f348bafdd84496bae1d02366a81e5)), closes [#cr-0001](https://github.com/Monkey-D-Luisi/propely/issues/cr-0001)
* **docs:** address PR [#12](https://github.com/Monkey-D-Luisi/propely/issues/12) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0002) ([c62de23](https://github.com/Monkey-D-Luisi/propely/commit/c62de233eedaec10db3cd807559bb9095414a8a3)), closes [#cr-0002](https://github.com/Monkey-D-Luisi/propely/issues/cr-0002)
* **infra:** address PR [#13](https://github.com/Monkey-D-Luisi/propely/issues/13) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0003) ([db4d026](https://github.com/Monkey-D-Luisi/propely/commit/db4d0268cc1b67f6d75b137ad14ba4c3f08afb04)), closes [#cr-0003](https://github.com/Monkey-D-Luisi/propely/issues/cr-0003)
* **infra:** address PR [#14](https://github.com/Monkey-D-Luisi/propely/issues/14) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0004) ([57ee7f1](https://github.com/Monkey-D-Luisi/propely/commit/57ee7f1943e10dffd6710bad2ae9756d98dc9dce)), closes [#cr-0004](https://github.com/Monkey-D-Luisi/propely/issues/cr-0004)
* **infra:** address PR [#16](https://github.com/Monkey-D-Luisi/propely/issues/16) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0006) ([76c1198](https://github.com/Monkey-D-Luisi/propely/commit/76c1198f294a50b7454c69bdbe6bce488bf37d51)), closes [#cr-0006](https://github.com/Monkey-D-Luisi/propely/issues/cr-0006)
* **orgs-api:** address PR [#18](https://github.com/Monkey-D-Luisi/propely/issues/18) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0008) ([cbfc621](https://github.com/Monkey-D-Luisi/propely/commit/cbfc621557ecbc166381ff3f9440eb27992f81b3)), closes [#cr-0008](https://github.com/Monkey-D-Luisi/propely/issues/cr-0008)
* **orgs-api:** address PR [#18](https://github.com/Monkey-D-Luisi/propely/issues/18) review feedback pass 2 ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0009) ([849a942](https://github.com/Monkey-D-Luisi/propely/commit/849a94247330a33b05ce85b7261bb92f97d20ea9)), closes [#cr-0009](https://github.com/Monkey-D-Luisi/propely/issues/cr-0009)
* **orgs:** address PR [#15](https://github.com/Monkey-D-Luisi/propely/issues/15) review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0005) ([b37b118](https://github.com/Monkey-D-Luisi/propely/commit/b37b11825efeb40fcf0d245dd10f218fdd712ff4)), closes [#cr-0005](https://github.com/Monkey-D-Luisi/propely/issues/cr-0005)
* **orgs:** address PR review feedback ([#cr](https://github.com/Monkey-D-Luisi/propely/issues/cr)-0010) ([e3400df](https://github.com/Monkey-D-Luisi/propely/commit/e3400dfa9a59a5bf2fa38c83a0aad5ec8b9e0f9e)), closes [#cr-0010](https://github.com/Monkey-D-Luisi/propely/issues/cr-0010)
* **web:** rename role value 'member' to 'agent' to match backend ([261b114](https://github.com/Monkey-D-Luisi/propely/commit/261b114cf605dd5767c1b1aa5da6df90b0e35043))
* **web:** sync package-lock.json to resolve missing @swc/helpers ([183131c](https://github.com/Monkey-D-Luisi/propely/commit/183131ce36e350f16cd37a717bc75401338ee168))

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
