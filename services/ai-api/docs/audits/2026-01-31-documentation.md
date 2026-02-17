# Documentation Audit — 2026-01-31

## Scope
- README: `README.md`
- Quick start guide: `QUICKSTART.md`
- Architecture docs: `docs/architecture/*`
- Task/walkthrough alignment: `docs/tasks/` vs `docs/walkthroughs/`
- XML documentation coverage for public APIs in `src/`

## Summary
- Infrastructure and messaging documentation drift from the current Docker Compose defaults, `.env.example`, and runtime configuration.
- Architecture references for repository layout, read-model schema, and AI client integration are out of sync with the current codebase.
- Task/walkthrough coverage is incomplete for code review tasks.
- XML documentation exists on many public types, but XML doc output is not enabled and member-level coverage is inconsistent.

## Findings

### F-001 — Infrastructure specs are out of sync with Compose defaults and `.env.example`
**Severity:** High

**Evidence**
- `docs/architecture/infrastructure-specs.md` documents:
  - `appuser`/`guest` defaults and passwords not matching current defaults.
  - Redis without a password and `redis-server --appendonly yes`.
  - Port bindings without loopback.
  - An `.env.example` template with `JWT_*`, `AI_*`, `ASPNETCORE_URLS`, and `ALLOW_ANONYMOUS` entries.
- `docker-compose.yml` binds services to `127.0.0.1`, defaults to `aiapi`/`aiapi_dev_password`, and requires a Redis password.
- `.env.example` only includes database, RabbitMQ, and Redis connection variables with `CHANGEME` placeholders.

**Impact**
Developers following the infrastructure spec can misconfigure credentials, miss required Redis auth, or expose services unintentionally.

**Recommendation**
Update `docs/architecture/infrastructure-specs.md` to reflect the current Docker Compose defaults, Redis password requirement, loopback bindings, and the actual `.env.example` content. Remove or clearly mark variables that are not currently wired in the runtime configuration.

---

### F-002 — Repository structure documentation marks implemented directories as “future”
**Severity:** Medium

**Evidence**
- `docs/architecture/repo-structure.md` labels `src/` and `tests/` as “future.”
- `src/` and `tests/` contain the current application and test projects.

**Impact**
New contributors may assume the implementation does not exist or is incomplete.

**Recommendation**
Update the repository structure doc to describe the actual projects in `src/` and `tests/`, and remove the “future” qualifier.

---

### F-003 — Vertical slice read-model schema no longer matches migrations
**Severity:** Medium

**Evidence**
- `docs/architecture/vertical-slice.md` defines `work_items_read` with `updated_at_utc` and a 20-character `status`.
- The migration `AddWorkItemsReadAndProcessedEvents` defines `last_projected_at_utc` and a 50-character `status` column.

**Impact**
Readers may build incorrect schemas or misunderstand the projection lifecycle fields.

**Recommendation**
Update the read-model schema section in `docs/architecture/vertical-slice.md` to reflect the current columns and sizes.

---

### F-004 — Messaging DLQ documentation uses outdated credential variable names
**Severity:** Medium

**Evidence**
- `docs/architecture/messaging-dlq-policy.md` uses `RABBITMQ_PASS` and `guest` in example commands.
- `.env.example` and `docker-compose.yml` use `RABBITMQ_PASSWORD` with default `aiapi` credentials.

**Impact**
Operators following the guide will fail to authenticate to the management API unless they translate variable names and credentials.

**Recommendation**
Update messaging documentation to use the actual environment variables and defaults from `.env.example`/Compose.

---

### F-005 — AI client integration doc references interfaces/config that do not exist
**Severity:** Medium

**Evidence**
- `docs/architecture/ai-client-integration.md` describes `IAiClient` and AI provider configuration in `.env.example`.
- No `IAiClient` interface or AI client implementation exists in `src/`, and `.env.example` does not include AI settings.

**Impact**
Readers may assume AI integration is implemented when it is not.

**Recommendation**
Either implement the AI client and add config entries to `.env.example`, or explicitly mark the document as an aspirational design for future work.

---

### F-006 — README test count is stale
**Severity:** Low

**Evidence**
- `README.md` claims “51 passing tests.”
- Current Fact/Theory count is higher (see Appendix A.2).

**Impact**
Static test counts quickly become inaccurate and reduce confidence in documentation.

**Recommendation**
Remove the static test count or replace it with a CI badge.

---

### F-007 — Missing walkthroughs for code review tasks
**Severity:** High

**Evidence**
- `docs/tasks/` contains `cr-0001` through `cr-0025`, but no corresponding walkthroughs exist.

**Impact**
Violates the repository rule that every task must have a matching walkthrough.

**Recommendation**
Add walkthroughs for each code review task or explicitly mark those tasks as superseded with links to the replacement walkthroughs.

---

### F-008 — XML documentation output is disabled and member-level docs are inconsistent
**Severity:** Medium

**Evidence**
- None of the `*.csproj` files enable XML documentation output.
- Several public members lack XML comments (e.g., public properties in `AppDbContext`, enum values in `WorkItemStatus`, public properties in `OutboxMessage`).

**Impact**
Public API documentation cannot be generated and consumers lack consistent in-code guidance.

**Recommendation**
Enable `GenerateDocumentationFile` in project files and add missing member-level XML comments for public APIs.

---

## Task/Walkthrough Alignment
Missing walkthroughs for:
- `cr-0001-ft0001-review-fixes.md`
- `cr-0002-application-layer-review.md`
- `cr-0003-infrastructure-layer-review.md`
- `cr-0004-presentation-layer-review.md`
- `cr-0005-quickstart-docs-review.md`
- `cr-0006-windows-compatibility-review.md`
- `cr-0007-outbox-messaging-review.md`
- `cr-0008-event-consumer-review.md`
- `cr-0009-redis-caching-review.md`
- `cr-0010-opentelemetry-review.md`
- `cr-0011-integration-tests-fix-review.md`
- `cr-0012-clean-architecture-audit-review.md`
- `cr-0013-performance-audit-review.md`
- `cr-0014-devops-readiness-audit-review.md`
- `cr-0015-audit-action-workflow-review.md`
- `cr-0016-audit-0001-credentials-review.md`
- `cr-0017-jwt-authentication-review.md`
- `cr-0018-https-hsts-review.md`
- `cr-0019-ci-pipeline-review.md`
- `cr-0020-read-model-repository-review.md`
- `cr-0021-projector-refactor-review.md`
- `cr-0022-redis-testcontainers-review.md`
- `cr-0023-task-directory-fix-review.md`
- `cr-0024-correlation-id-review.md`
- `cr-0025-event-metadata-review.md`

## XML Documentation Coverage (Public APIs)
- Many public types include XML summaries (controllers, commands, domain entities).
- XML documentation output is disabled in all project files.
- Public members that could benefit from XML comments include DbSet properties, enum values, and public model properties.

## Appendix A — Evidence Snapshots

### A.1 Task vs Walkthrough diff
```
$ comm -3 <(ls docs/tasks | sort) <(ls docs/walkthroughs | sort)
cr-0001-ft0001-review-fixes.md
cr-0002-application-layer-review.md
cr-0003-infrastructure-layer-review.md
cr-0004-presentation-layer-review.md
cr-0005-quickstart-docs-review.md
cr-0006-windows-compatibility-review.md
cr-0007-outbox-messaging-review.md
cr-0008-event-consumer-review.md
cr-0009-redis-caching-review.md
cr-0010-opentelemetry-review.md
cr-0011-integration-tests-fix-review.md
cr-0012-clean-architecture-audit-review.md
cr-0013-performance-audit-review.md
cr-0014-devops-readiness-audit-review.md
cr-0015-audit-action-workflow-review.md
cr-0016-audit-0001-credentials-review.md
cr-0017-jwt-authentication-review.md
cr-0018-https-hsts-review.md
cr-0019-ci-pipeline-review.md
cr-0020-read-model-repository-review.md
cr-0021-projector-refactor-review.md
cr-0022-redis-testcontainers-review.md
cr-0023-task-directory-fix-review.md
cr-0024-correlation-id-review.md
cr-0025-event-metadata-review.md
```

### A.2 Test attribute counts (Fact/Theory)
```
$ rg "\[(Fact|Theory)\]" -c tests
tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs:10
tests/SaasTemplate.AiApi.IntegrationTests/Api/Middleware/CorrelationIdMiddlewareTests.cs:6
tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs:6
tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs:7
tests/SaasTemplate.AiApi.IntegrationTests/Caching/RedisCacheServiceIntegrationTests.cs:6
tests/SaasTemplate.AiApi.IntegrationTests/Caching/RedisCacheServiceTests.cs:5
tests/SaasTemplate.AiApi.UnitTests/Infrastructure/Messaging/OutboxDispatcherServiceTests.cs:8
tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs:8
tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs:8
tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemStatusTests.cs:5
tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs:13
tests/SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs:6
```

### A.3 AI client interface search
```
$ rg -n "IAiClient" -S src
```
