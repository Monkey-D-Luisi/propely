# Documentation Audit — 2026-01-30

## Scope
- README: `README.md`
- Quick start guide: `QUICKSTART.md`
- Architecture docs: `docs/architecture/*`
- Task/walkthrough alignment: `docs/tasks/` vs `docs/walkthroughs/`
- XML documentation coverage for public APIs in `src/`

## Summary
- Architecture documentation includes multiple mismatches with current scripts/configuration and environment defaults.
- Task coverage is incomplete: code review tasks do not have matching walkthroughs.
- XML documentation exists on many public types, but XML documentation output is not enabled and member-level coverage is uneven.

## Findings

### F-001 — Infrastructure specs out of sync with docker-compose and .env defaults
**Severity:** High

**Evidence**
- `docs/architecture/infrastructure-specs.md` documents Compose port bindings without loopback, Redis without a password, and a different `.env` template (CHANGEME defaults and variables such as `REDIS_CONNECTION_STRING` without password, `JWT_*`, `AI_*`, `ASPNETCORE_URLS`, `ALLOW_ANONYMOUS`).
- `docker-compose.yml` binds services to `127.0.0.1`, requires Redis password, and uses different default credentials.
- `.env.example` contains concrete defaults (not CHANGEME) and includes passwords in connection strings.

**Impact**
Developers following the architecture spec may configure incorrect environment variables or miss required Redis credentials.

**Recommendation**
Align `docs/architecture/infrastructure-specs.md` with the current `docker-compose.yml` and `.env.example` defaults. Remove or clearly mark unused variables.

---

### F-002 — Repo structure doc claims `src/` and `tests/` are “future”
**Severity:** Medium

**Evidence**
- `docs/architecture/repo-structure.md` labels `src/` and `tests/` as future directories, but projects exist in both.

**Impact**
New contributors may assume the codebase is empty or incomplete when it is already implemented.

**Recommendation**
Update `docs/architecture/repo-structure.md` to describe the actual projects in `src/` and `tests/`.

---

### F-003 — AI client integration doc references config not present in `.env.example`
**Severity:** Medium

**Evidence**
- `docs/architecture/ai-client-integration.md` lists `AI_PROVIDER`, `AI_API_KEY`, and `AI_MODEL` in the `.env.example` template section.
- `.env.example` does not contain these AI variables.

**Impact**
Readers may assume AI client integration is already wired when it is not.

**Recommendation**
Either add the AI configuration entries to `.env.example` and implement the interface, or mark the AI integration document as aspirational with a clear “not yet implemented” notice.

---

### F-004 — README contains a static test count that is now stale
**Severity:** Low

**Evidence**
- `README.md` states “51 passing tests (37 unit + 14 integration).”
- Current test counts (Fact/Theory attributes) exceed that number (see Appendix A).

**Impact**
Static counts are easy to forget to update and erode trust in the documentation.

**Recommendation**
Replace the static count with a build badge or remove the explicit count.

---

### F-005 — Missing walkthroughs for code review tasks
**Severity:** High

**Evidence**
- `docs/tasks/` includes `cr-0001` through `cr-0011`, but no corresponding walkthrough files exist.

**Impact**
Breaks the repository rule that every task requires a matching walkthrough.

**Recommendation**
Add walkthroughs for each code review task or mark the tasks as superseded with an explicit note.

---

## Task/Walkthrough Alignment
- Missing walkthroughs for:
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

## XML Documentation Coverage (Public APIs)
- Many public API types include XML summaries and parameter docs (controllers, DTOs, commands, domain entities).
- XML documentation output is not enabled in project files, so generated docs are not produced.
- Member-level XML coverage is inconsistent (e.g., enum values and DbSet properties lack XML comments).

**Recommendation**
Enable XML documentation output in project files and add missing member-level documentation for public API surfaces where it aids consumers.

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
```

### A.2 Test attribute counts (Fact/Theory)
```
$ rg "\[(Fact|Theory)\]" -c tests
SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs:8
SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs:8
SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemStatusTests.cs:5
SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs:13
SaasTemplate.AiApi.UnitTests/Infrastructure/Messaging/OutboxDispatcherServiceTests.cs:8
SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs:6
SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs:8
SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs:6
SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs:7
SaasTemplate.AiApi.IntegrationTests/Caching/RedisCacheServiceTests.cs:5
```

Total Fact/Theory attributes counted: 74.
